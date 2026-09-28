using System.Net;
using System.Security.Cryptography;
using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using RideAudit.Attest;
using RideAudit.Client.Contracts;
using RideAudit.Client.Seal;
using RideAudit.Contracts;
using RideAudit.PlayIntegrity;
using RideAudit.Protos.Admission.V1;
using RideAudit.Seal;
using RideAudit.Server.Admission;
using RideAudit.Server.Identity;
using RideAudit.TestSupport;
using ClientScope = RideAudit.Client.Seal.KeyScope;

namespace RideAudit.Server.Admission.Tests;

/// <summary>
/// Client RAES envelopes submitted through the generated admission gRPC client.
/// Play Integrity and chain confirmation in this fixture are documented test doubles.
/// </summary>
public class RaesGrpcAdmissionTests
{
    [Fact]
    public async Task Client_raes_envelope_is_admitted_over_grpc_without_reencoding()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var raes = SealRaes(world, enrolled.Driver.DriverId, enrolled.Session.SessionId);
        var nonce = "nonce-raes-grpc";
        var token = world.IssueToken(enrolled.Session.SessionId, nonce);
        Escrow(world, enrolled.Driver.TenantId, raes);
        var request = Request(world, enrolled, raes, token, nonce);

        await using var host = await Start(world);
        using var transport = Client(host, enrolled.Driver.Token);
        var client = transport.Admission;
        var decision = client.SubmitSealed(request);

        Assert.True(decision.Admitted);
        Assert.True(decision.CollectionComplete);
        Assert.True(decision.CiphertextStored);
        Assert.Equal("", decision.RejectCode);
        Assert.NotNull(decision.Anchor);
        Assert.False(decision.Anchor.LiveBitcoinMetadata);
        Assert.Equal("documented-fixture", decision.Anchor.ProofSource);
        Assert.StartsWith("fixture:", decision.Anchor.TransactionReference, StringComparison.Ordinal);
        Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);

        var stored = world.App.Journal.Find(decision.SubmissionId);
        Assert.NotNull(stored);
        Assert.True(stored!.EnvelopeBytes.AsSpan().SequenceEqual(raes.Record.Envelope));
        Assert.True(RaesEnvelopeFormat.HasMagic(stored.EnvelopeBytes));
        Assert.False(stored.EnvelopeBytes.AsSpan().StartsWith(EnvelopeFormat.Magic));
        Assert.Equal(Ids.Hex(Ids.Sha256(raes.Record.Envelope)), raes.Record.Receipt.ContentHash);
        Assert.Equal("AES-256-GCM", raes.Record.Receipt.Algorithm);
        Assert.Equal(RideAuditPolicy.AlgorithmId, request.ReceiptCore.AlgorithmId);
        Assert.True(stored.EnvelopeBytes.AsSpan().IndexOf(Encoding.UTF8.GetBytes("raes-grpc-fixture-sample")) < 0);

        var status = client.GetAdmissionStatus(new GetAdmissionStatusRequest { SubmissionId = decision.SubmissionId });
        Assert.True(status.Admitted);
        Assert.Equal(decision.SubmissionId, status.SubmissionId);
    }

    [Fact]
    public async Task Grpc_client_fail_closes_tamper_plaintext_bad_token_and_missing_escrow()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var raes = SealRaes(world, enrolled.Driver.DriverId, enrolled.Session.SessionId);
        var nonce = "nonce-raes-reject";
        var token = world.IssueToken(enrolled.Session.SessionId, nonce);
        var request = Request(world, enrolled, raes, token, nonce);

        await using var host = await Start(world);
        using var transport = Client(host, enrolled.Driver.Token);
        var client = transport.Admission;

        var tampered = request.Clone();
        var broken = tampered.SealedEnvelope.ToByteArray();
        broken[^1] ^= 0x11;
        tampered.SealedEnvelope = Google.Protobuf.ByteString.CopyFrom(broken);
        var tamper = client.SubmitSealed(tampered);
        Assert.False(tamper.Admitted);
        Assert.Equal(ErrorCodes.ReceiptInvalid, tamper.RejectCode);

        var plain = request.Clone();
        plain.IdempotencyKey = "idem-raes-plain";
        plain.ContentType = "video/mp4";
        plain.SealedEnvelope = Google.Protobuf.ByteString.CopyFrom(new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 });
        var plaintext = client.SubmitSealed(plain);
        Assert.False(plaintext.Admitted);
        Assert.Equal(ErrorCodes.PlaintextRejected, plaintext.RejectCode);

        var badToken = request.Clone();
        badToken.IdempotencyKey = "idem-raes-token";
        badToken.Attestation.Token = raes.Record.Receipt.AttestationTokenHash;
        var attestation = client.SubmitSealed(badToken);
        Assert.False(attestation.Admitted);
        Assert.Equal(ErrorCodes.AttestationFailed, attestation.RejectCode);
        Assert.Equal(64, raes.Record.Receipt.AttestationTokenHash.Length);
        Assert.DoesNotContain('.', raes.Record.Receipt.AttestationTokenHash);

        var missingEscrow = client.SubmitSealed(request);
        Assert.False(missingEscrow.Admitted);
        Assert.Equal(ErrorCodes.EscrowUnavailable, missingEscrow.RejectCode);
        Assert.DoesNotContain(world.App.Journal.Snapshot(), record => record.State == CustodyState.Admitted);
        Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);
    }

    private static AdmissionTransport Client(WebApplication host, string bearer)
    {
        var addresses = host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
        var address = addresses!.Addresses.Single(value => value.StartsWith("http://", StringComparison.Ordinal));
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var channel = GrpcChannel.ForAddress(address);
        var headers = new Metadata { { "authorization", "Bearer " + bearer } };
        var admission = new GrpcSealedAdmissionClient(new RideAudit.Protos.Admission.V1.Admission.AdmissionClient(channel), headers);
        return new AdmissionTransport(channel, admission);
    }

    private static async Task<WebApplication> Start(ServerWorld world)
    {
        var app = AdmissionHost.Build(world.Options, builder =>
        {
            builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, string.Empty);
            builder.WebHost.ConfigureKestrel(kestrel =>
            {
                kestrel.Listen(IPAddress.Loopback, 0, listen => listen.Protocols = HttpProtocols.Http2);
            });
        }, world.App);
        await app.StartAsync();
        return app;
    }

    private static SealedRaes SealRaes(ServerWorld world, string collectorId, string sessionId)
    {
        var clock = new RideAudit.Client.Core.FixedClock(ServerWorld.Start);
        var gate = new PlayIntegrityGate(
            new FixturePlayIntegrityClient(),
            RideAudit.PlayIntegrity.PackageAllowlist.CreateDevelopmentDefault(),
            clock);
        var authorization = gate.AuthorizeKeyGeneration(AttestationRequest.Create(collectorId));
        var (publicKey, rsa) = EscrowKeyFactory.CreateEphemeral("escrow-raes");
        var record = new CollectionSealer(clock).Seal(new RideAudit.Client.Seal.SealRequest
        {
            Plaintext = Encoding.UTF8.GetBytes("raes-grpc-fixture-sample"),
            SessionId = sessionId,
            RecordId = "rec-raes-grpc",
            Scope = ClientScope.Session,
            ScopeId = sessionId,
            Authorization = authorization,
            EscrowKey = publicKey,
            CollectorIdentity = collectorId,
            ProvenanceTag = RideAuditPolicy.ProvenanceTag,
            Kind = EvidenceKind.SensorSample,
            DeviceIds = ["device-raes"],
            SourceCommitNotice = "fixture"
        });
        Assert.True(CollectionSealer.LooksSealed(record.Envelope));
        return new SealedRaes(record, rsa);
    }

    private static void Escrow(ServerWorld world, string tenantId, SealedRaes raes)
    {
        var dek = EscrowKeyFactory.Unwrap(raes.PrivateKey, raes.Record.WrappedKey.WrappedKey);
        world.App.Hsm.EscrowCollectionSecret(new EscrowSecret
        {
            KeyId = raes.Record.Receipt.KeyId,
            TenantId = tenantId,
            SealedRecordId = raes.Record.Id,
            PrivateKeyPkcs8 = raes.PrivateKey.ExportPkcs8PrivateKey(),
            Dek = dek,
            CustodianIds = ["custodian-a", "custodian-b", "custodian-c"],
            ThresholdM = 2,
            TotalN = 3
        });
    }

    private static SubmitSealedRequest Request(
        ServerWorld world,
        (RegisteredDriver Driver, VehicleRecord Vehicle, AuditSessionRecord Session) enrolled,
        SealedRaes raes,
        string token,
        string nonce)
    {
        var evidenceHash = PlayIntegrityVerifier.BindEvidence(token, raes.Record.Receipt.KeyId, enrolled.Session.SessionId, nonce);
        var receipt = ReceiptCoreCodec.Build(new ReceiptCoreInput
        {
            PolicyVersion = world.Options.PolicyVersion,
            ContentHash = Ids.Sha256(raes.Record.Envelope),
            AlgorithmId = RideAuditPolicy.AlgorithmId,
            KeyId = raes.Record.Receipt.KeyId,
            PublicKeyMaterial = Encoding.UTF8.GetBytes(raes.Record.Receipt.PublicKeyPem),
            KeyScope = raes.Record.Receipt.KeyScope,
            ScopeBinding = raes.Record.Receipt.ScopeId,
            CollectorId = raes.Record.Receipt.CollectorIdentity,
            DriverId = enrolled.Driver.DriverId,
            VehicleId = enrolled.Vehicle.VehicleId,
            SessionId = enrolled.Session.SessionId,
            TenantId = enrolled.Driver.TenantId,
            CollectionUnixMillis = raes.Record.Receipt.SealedAt.ToUnixTimeMilliseconds(),
            ProvenanceTag = raes.Record.Receipt.ProvenanceTag,
            AttestationEvidenceHash = evidenceHash,
            PackageIdentity = ServerWorld.PackageName,
            SigningCertDigest = ServerWorld.CertDigest,
            Nonce = nonce,
            SealedRecordId = raes.Record.Id
        });

        return new SubmitSealedRequest
        {
            SessionId = enrolled.Session.SessionId,
            VehicleId = enrolled.Vehicle.VehicleId,
            IdempotencyKey = "idem-" + raes.Record.Id,
            ContentType = RideAuditPolicy.SealedContentType,
            SealedEnvelope = Google.Protobuf.ByteString.CopyFrom(raes.Record.Envelope),
            ReceiptCore = receipt.Core,
            Attestation = new AttestationSubmission
            {
                Provider = RideAuditPolicy.PlayProvider,
                Token = token,
                Nonce = nonce,
                ObtainedUnixMillis = world.Clock.UtcNow.ToUnixTimeMilliseconds(),
                PackageName = ServerWorld.PackageName,
                CertDigest = ServerWorld.CertDigest,
                BoundKeyId = raes.Record.Receipt.KeyId,
                BoundSessionId = enrolled.Session.SessionId
            }
        };
    }

    private sealed record SealedRaes(SealedRecord Record, RSA PrivateKey);

    private sealed class AdmissionTransport : IDisposable
    {
        public AdmissionTransport(GrpcChannel channel, GrpcSealedAdmissionClient admission)
        {
            Channel = channel;
            Admission = admission;
        }

        public GrpcChannel Channel { get; }
        public GrpcSealedAdmissionClient Admission { get; }

        public void Dispose() => Channel.Dispose();
    }
}
