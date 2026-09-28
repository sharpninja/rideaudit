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
using RideAudit.Client.Contracts;
using RideAudit.Client.Seal;
using RideAudit.Contracts;
using RideAudit.PlayIntegrity;
using RideAudit.Server.Admission;
using RideAudit.TestSupport;
using ClientScope = RideAudit.Client.Seal.KeyScope;

namespace RideAudit.Server.Admission.Tests;

internal sealed record SealedRaes(SealedRecord Record, RSA PrivateKey, byte[] Plaintext);

internal sealed class AdmissionTransport : IDisposable
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

internal static class RaesHelpers
{
    public static AdmissionTransport Client(WebApplication host, string bearer)
    {
        var addresses = host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
        var address = addresses!.Addresses.Single(value => value.StartsWith("http://", StringComparison.Ordinal));
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var channel = GrpcChannel.ForAddress(address);
        var headers = new Metadata { { "authorization", "Bearer " + bearer } };
        var admission = new GrpcSealedAdmissionClient(new RideAudit.Protos.Admission.V1.Admission.AdmissionClient(channel), headers);
        return new AdmissionTransport(channel, admission);
    }

    public static async Task<WebApplication> Start(ServerWorld world)
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

    public static SealedRaes SealRaes(string collectorId, string sessionId, byte[]? plaintext = null)
    {
        var clock = new RideAudit.Client.Core.FixedClock(ServerWorld.Start);
        var gate = new PlayIntegrityGate(
            new FixturePlayIntegrityClient(),
            PackageAllowlist.CreateDevelopmentDefault(),
            clock);
        var authorization = gate.AuthorizeKeyGeneration(AttestationRequest.Create(collectorId));
        var (publicKey, rsa) = EscrowKeyFactory.CreateEphemeral("escrow-raes");
        var body = plaintext ?? Encoding.UTF8.GetBytes("raes-grpc-fixture-sample");
        var record = new CollectionSealer(clock).Seal(new RideAudit.Client.Seal.SealRequest
        {
            Plaintext = body.ToArray(),
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
        return new SealedRaes(record, rsa, body);
    }

    public static void Escrow(ServerWorld world, string tenantId, SealedRaes raes) =>
        Escrow(world, tenantId, raes.Record, raes.PrivateKey);

    public static void Escrow(ServerWorld world, string tenantId, SealedRecord record, RSA privateKey)
    {
        var dek = EscrowKeyFactory.Unwrap(privateKey, record.WrappedKey.WrappedKey);
        world.App.Hsm.EscrowCollectionSecret(new EscrowSecret
        {
            KeyId = record.Receipt.KeyId,
            TenantId = tenantId,
            SealedRecordId = record.Id,
            PrivateKeyPkcs8 = privateKey.ExportPkcs8PrivateKey(),
            Dek = dek,
            CustodianIds = ["custodian-a", "custodian-b", "custodian-c"],
            ThresholdM = 2,
            TotalN = 3
        });
    }
}
