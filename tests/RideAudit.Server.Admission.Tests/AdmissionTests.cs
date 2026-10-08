using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Authentication;
using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using RideAudit.Contracts;
using RideAudit.Ingest;
using RideAudit.Protos.Admission.V1;
using RideAudit.Seal;
using RideAudit.Sec;
using RideAudit.Server.Admission;
using RideAudit.TestSupport;

namespace RideAudit.Server.Admission.Tests;

/// <summary>TEST-RIDE-021. FR-RIDE-032, FR-RIDE-033, FR-RIDE-034.</summary>
public class TestRide021Identity
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-021")]
    [Trait("FR", "FR-RIDE-032")]
    [Trait("AC", "AC-RIDE-032-001")]
        [Trait("AC", "AC-RIDE-032-003")]
        [Trait("AC", "AC-RIDE-SERVER-001-001")]
        [Trait("AC", "AC-RIDE-SERVER-001-002")]
        [Trait("AC", "AC-RIDE-PRIV-001-001")]
        [Trait("AC", "AC-UC-014-002")]
    public void Driver_registration_requires_consent_and_recovery_rotates_the_token()
    {
        var world = ServerWorld.Create();
        var ex = Assert.Throws<RideAuditException>(() => world.App.Identity.Register("a@example.com", "A", "US-CA", "audit", false, "no"));
        Assert.Equal(ErrorCodes.ValidationFailed, ex.Code);
        var driver = world.Register();
        Assert.StartsWith("ten-", driver.TenantId, StringComparison.Ordinal);
        var recovered = world.App.Identity.Recover(driver.Email, driver.RecoveryCode);
        Assert.Null(world.App.Identity.Authenticate(driver.Token));
        Assert.NotNull(world.App.Identity.Authenticate(recovered.AccessToken));
        var dump = world.App.Identity.Database.DumpForAudit();
        Assert.True(Encoding.UTF8.GetString(dump).Contains(driver.DriverId, StringComparison.Ordinal));
        Assert.DoesNotContain(driver.Token, Encoding.UTF8.GetString(dump), StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-021")]
    [Trait("FR", "FR-RIDE-033")]
    [Trait("FR", "FR-RIDE-034")]
    [Trait("AC", "AC-RIDE-033-002")]
        [Trait("AC", "AC-RIDE-034-002")]
        [Trait("AC", "AC-RIDE-SERVER-002-001")]
        [Trait("AC", "AC-RIDE-SERVER-002-002")]
        [Trait("AC", "AC-RIDE-033-001")]
        [Trait("AC", "AC-RIDE-034-001")]
        [Trait("AC", "AC-TEST-021-001")]
        [Trait("AC", "AC-TEST-021-002")]
        [Trait("AC", "AC-UC-014-001")]
        [Trait("AC", "AC-UC-014-002")]
    public void Vehicle_history_is_kept_and_a_missing_profile_blocks_the_session()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var denied = Assert.Throws<RideAuditException>(() => world.App.Identity.RegisterVehicle(world.Require(driver), "VIN-NO", "No consent", "Test", "Model", 2024, false));
        Assert.Equal(ErrorCodes.ValidationFailed, denied.Code);
        var vehicle = world.AddVehicle(driver);
        Assert.Equal(driver.DriverId, vehicle.DriverId);
        Assert.False(string.IsNullOrWhiteSpace(vehicle.VinOrPlateKey));
        var updated = world.App.Identity.UpdateVehicle(world.Require(driver), vehicle.VehicleId, "Renamed", "Test", "Model", 2025, "plate correction");
        Assert.Equal(2, updated.Changes.Count);
        var blocked = Assert.Throws<RideAuditException>(() => world.OpenSession(driver, vehicle.VehicleId));
        Assert.Equal(ErrorCodes.ConfigProfileInvalid, blocked.Code);
        world.PutProfile(driver, vehicle.VehicleId, valid: false);
        blocked = Assert.Throws<RideAuditException>(() => world.OpenSession(driver, vehicle.VehicleId));
        Assert.Equal(ErrorCodes.ConfigProfileInvalid, blocked.Code);
    }
}

/// <summary>TEST-RIDE-022. FR-RIDE-035, FR-RIDE-036, FR-RIDE-061.</summary>
public class TestRide022SealedAdmission
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-022")]
    [Trait("FR", "FR-RIDE-035")]
        [Trait("AC", "AC-RIDE-035-001")]
        [Trait("AC", "AC-RIDE-SERVER-003-001")]
        [Trait("AC", "AC-RIDE-SERVER-003-002")]
        [Trait("AC", "AC-RIDE-SERVER-004-002")]
        [Trait("AC", "AC-RIDE-061-002")]
        [Trait("AC", "AC-UC-030-001")]
        [Trait("AC", "AC-TEST-022-002")]
    public void Plaintext_and_unauthorized_vehicle_are_rejected_without_storing_a_body()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x11, 0x22, 0x33, 0x44 };
        var plain = Assert.Throws<RideAuditException>(() => world.App.Admission.Submit(new SubmitSealedCommand
        {
            Principal = world.Require(enrolled.Driver),
            RawBearerToken = enrolled.Driver.Token,
            IdempotencyKey = "idem-plain",
            ContentType = "image/jpeg",
            EnvelopeBytes = jpeg,
            SubmittedReceiptBytes = new byte[] { 1 },
            AttestationToken = "fixture.v1.not-used",
            AttestationNonce = "n",
            SessionId = enrolled.Session.SessionId,
            VehicleId = enrolled.Vehicle.VehicleId
        }));
        Assert.Equal(ErrorCodes.PlaintextRejected, plain.Code);
        Assert.Empty(world.App.Identity.Database.SubmissionCiphertexts);
        Assert.Contains(world.App.Journal.FailureAudit, line => line.Contains("plaintext", StringComparison.OrdinalIgnoreCase));

        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var other = world.Register();
        var otherVehicle = world.AddVehicle(other);
        world.PutProfile(other, otherVehicle.VehicleId);
        var otherSession = world.OpenSession(other, otherVehicle.VehicleId);
        var foreign = world.SealReady(other, otherSession);
        var rejected = Assert.Throws<RideAuditException>(() => world.App.Admission.Submit(new SubmitSealedCommand
        {
            Principal = world.Require(enrolled.Driver),
            RawBearerToken = enrolled.Driver.Token,
            IdempotencyKey = "idem-foreign",
            ContentType = foreign.Package.ContentType,
            EnvelopeBytes = foreign.Package.EnvelopeBytes,
            SubmittedReceiptBytes = foreign.Package.ReceiptCoreBytes,
            AttestationToken = foreign.Token,
            AttestationNonce = foreign.Nonce,
            BoundKeyId = foreign.Package.KeyId,
            SessionId = enrolled.Session.SessionId,
            VehicleId = enrolled.Vehicle.VehicleId
        }));
        Assert.True(rejected.Code is ErrorCodes.TenantIsolation or ErrorCodes.ReceiptInvalid or ErrorCodes.SessionInvalid);
        _ = ready;
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-022")]
    [Trait("FR", "FR-RIDE-036")]
    [Trait("FR", "FR-RIDE-061")]
    [Trait("AC", "AC-RIDE-036-001")]
        [Trait("AC", "AC-RIDE-036-002")]
        [Trait("AC", "AC-RIDE-SERVER-004-001")]
    public void Happy_path_checks_receipt_hash_chain_attestation_and_a_mutated_receipt_is_not_admitted()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);
        Assert.True(outcome.Admitted);
        Assert.Contains("chain", outcome.Checks);
        Assert.Contains("attestation", outcome.Checks);
        Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);

        var mutated = ready.Package.ReceiptCoreBytes.ToArray();
        mutated[^1] ^= 0x5A;
        var ex = Assert.Throws<RideAuditException>(() => world.App.Admission.Submit(new SubmitSealedCommand
        {
            Principal = world.Require(enrolled.Driver),
            IdempotencyKey = "idem-mutated-receipt",
            ContentType = ready.Package.ContentType,
            EnvelopeBytes = ready.Package.EnvelopeBytes,
            SubmittedReceiptBytes = mutated,
            AttestationToken = ready.Token,
            AttestationNonce = ready.Nonce,
            BoundKeyId = ready.Package.KeyId,
            SessionId = enrolled.Session.SessionId,
            VehicleId = enrolled.Vehicle.VehicleId
        }));
        Assert.Equal(ErrorCodes.ReceiptInvalid, ex.Code);
    }
}

/// <summary>TEST-RIDE-019 admission partition. FR-RIDE-026, FR-RIDE-027.</summary>
public class TestRide019Admission
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-019")]
    [Trait("FR", "FR-RIDE-026")]
    [Trait("FR", "FR-RIDE-027")]
    [Trait("AC", "AC-RIDE-026-003")]
    [Trait("AC", "AC-RIDE-027-001")]
    [Trait("AC", "AC-UC-012-001")]
    [Trait("AC", "AC-UC-012-002")]
    [Trait("AC", "AC-TEST-019-001")]
    [Trait("AC", "AC-TEST-019-002")]
    public void Stale_or_nonce_mismatched_attestation_is_not_admitted_and_success_binds_the_hash()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        world.Clock.Advance(TimeSpan.FromMinutes(10));
        var stale = Assert.Throws<RideAuditException>(() => world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, "idem-stale"));
        Assert.Equal(ErrorCodes.AttestationFailed, stale.Code);
        Assert.Null(stale.SubmissionId);

        var freshWorld = ServerWorld.Create();
        var fresh = freshWorld.Enroll();
        var sealedFresh = freshWorld.SealReady(fresh.Driver, fresh.Session);
        var mismatch = Assert.Throws<RideAuditException>(() => freshWorld.Submit(fresh.Driver, sealedFresh.Package, sealedFresh.Token, "other-nonce", "idem-nonce"));
        Assert.Equal(ErrorCodes.AttestationFailed, mismatch.Code);

        var ok = freshWorld.Submit(fresh.Driver, sealedFresh.Package, sealedFresh.Token, sealedFresh.Nonce);
        Assert.True(ok.Admitted);
        Assert.Equal(32, sealedFresh.Package.Receipt.Core.AttestationEvidenceHash.Length);
        Assert.Equal(ServerWorld.PackageName, sealedFresh.Package.Receipt.Core.PackageIdentity);
        Assert.DoesNotContain(sealedFresh.Token, sealedFresh.Package.ReceiptCoreBytes.AsSpan() is var _ ? Encoding.UTF8.GetString(sealedFresh.Package.ReceiptCoreBytes) : "", StringComparison.Ordinal);

        freshWorld.App.Allowlist.Rotate("allowlist-2", new[] { new RideAudit.Attest.AllowlistEntry(ServerWorld.PackageName, "sha256:rotated") }, "security", freshWorld.Clock.UtcNow.ToUnixTimeMilliseconds());
        Assert.Equal(2, freshWorld.App.Allowlist.History.Count);
    }
}

/// <summary>TEST-RIDE-024. FR-RIDE-039, FR-RIDE-040, FR-RIDE-218.</summary>
public class TestRide024AbuseAndTenant
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-024")]
    [Trait("FR", "FR-RIDE-039")]
    [Trait("FR", "FR-RIDE-218")]
    [Trait("AC", "AC-RIDE-039-001")]
        [Trait("AC", "AC-RIDE-218-002")]
        [Trait("AC", "AC-RIDE-SERVER-005-001")]
        [Trait("AC", "AC-RIDE-SERVER-005-002")]
        [Trait("AC", "AC-RIDE-218-001")]
        [Trait("AC", "AC-RIDE-039-003")]
        [Trait("AC", "AC-TEST-024-001")]
        [Trait("AC", "AC-TEST-024-002")]
    public void Rate_limit_replay_and_backpressure_never_admit()
    {
        var world = ServerWorld.Create();
        world.App.Abuse.LimitPerMinute = 1;
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var first = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, "idem-once");
        Assert.True(first.Admitted);
        Assert.NotNull(world.App.Journal.Find(first.SubmissionId));
        var limited = Assert.Throws<RideAuditException>(() => world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, "idem-limited"));
        Assert.Equal(ErrorCodes.RateLimited, limited.Code);

        var replayWorld = ServerWorld.Create();
        var replay = replayWorld.Enroll();
        var body = replayWorld.SealReady(replay.Driver, replay.Session);
        replayWorld.Submit(replay.Driver, body.Package, body.Token, body.Nonce, "idem-a");
        var duplicate = Assert.Throws<RideAuditException>(() => replayWorld.Submit(replay.Driver, body.Package, body.Token, body.Nonce, "idem-b"));
        Assert.Equal(ErrorCodes.DuplicateReplay, duplicate.Code);

        var pressure = ServerWorld.Create();
        pressure.App.Abuse.ForceBackpressure = true;
        var person = pressure.Enroll();
        var jpeg = Assert.Throws<RideAuditException>(() => pressure.App.Admission.Submit(new SubmitSealedCommand
        {
            Principal = pressure.Require(person.Driver),
            IdempotencyKey = "idem-jpeg",
            ContentType = "image/jpeg",
            EnvelopeBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0x01 },
            SubmittedReceiptBytes = new byte[] { 1 },
            AttestationToken = "x",
            AttestationNonce = "n",
            SessionId = person.Session.SessionId,
            VehicleId = person.Vehicle.VehicleId
        }));
        Assert.Equal(ErrorCodes.PlaintextRejected, jpeg.Code);
        Assert.Contains(world.App.Logs.Lines, line => line.Contains("admitted submission=", StringComparison.Ordinal));
        Assert.Contains(pressure.App.Admission.FailureAudit, line => line.Contains("plaintext", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(pressure.App.Identity.Database.SubmissionCiphertexts);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-024")]
    [Trait("FR", "FR-RIDE-040")]
        [Trait("AC", "AC-RIDE-040-001")]
        [Trait("AC", "AC-RIDE-SERVER-006-001")]
        [Trait("AC", "AC-RIDE-032-002")]
        [Trait("AC", "AC-RIDE-040-002")]
        [Trait("AC", "AC-UC-015-002")]
    public void Cross_tenant_status_reads_are_isolated()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);
        var other = world.Register();
        var isolated = Assert.Throws<RideAuditException>(() => world.App.Admission.GetStatus(world.Require(other), outcome.SubmissionId));
        Assert.Equal(ErrorCodes.TenantIsolation, isolated.Code);
        Assert.Contains(world.App.AccessLog.Entries, entry => entry.Action == "get-admission" && !entry.Allowed);
        Assert.DoesNotContain(typeof(AppendOnlyAccessLog).GetMethods().Select(method => method.Name), name => name is "Remove" or "Clear" or "Delete");
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-024")]
    public void Chunk_upload_is_resumable_and_conflicts_on_rewritten_bytes()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var envelope = ready.Package.EnvelopeBytes;
        var split = envelope.Length / 2;
        var first = envelope[..split];
        var second = envelope[split..];
        var upload = "upload-1";
        var partial = world.App.Admission.UploadChunk(
            world.Require(enrolled.Driver), enrolled.Driver.Token, "127.0.0.1", upload, 0, 2, first, Ids.Sha256(first),
            ready.Package.ContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-chunk",
            null, null, null, null);
        Assert.False(partial.Complete);
        var conflict = Assert.Throws<RideAuditException>(() => world.App.Admission.UploadChunk(
            world.Require(enrolled.Driver), enrolled.Driver.Token, "127.0.0.1", upload, 0, 2, new byte[first.Length], Ids.Sha256(new byte[first.Length]),
            ready.Package.ContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-chunk",
            null, null, null, null));
        Assert.Equal(ErrorCodes.ChunkOutOfOrder, conflict.Code);
        var done = world.App.Admission.UploadChunk(
            world.Require(enrolled.Driver), enrolled.Driver.Token, "127.0.0.1", upload, 1, 2, second, Ids.Sha256(second),
            ready.Package.ContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-chunk",
            ready.Package.ReceiptCoreBytes, ready.Token, ready.Nonce, ready.Package.KeyId);
        Assert.True(done.Complete);
        Assert.True(done.Decision!.Admitted);
    }
}

/// <summary>TEST-RIDE-029 server partition. FR-RIDE-201, TR-RIDE-SEC-001. Full geolocation UI remains S7.</summary>
public class TestRide029Security
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-029")]
    [Trait("FR", "FR-RIDE-201")]
    [Trait("AC", "AC-RIDE-201-001")]
        [Trait("AC", "AC-RIDE-201-002")]
        [Trait("AC", "AC-RIDE-SEC-001-002")]
        [Trait("AC", "AC-RIDE-SEC-001-001")]
    public void Tls_policy_vault_and_log_redaction_hold()
    {
        #pragma warning disable SYSLIB0039
        Assert.Throws<InvalidOperationException>(() => TransportSecurityPolicy.EnsureTls12OrHigher(SslProtocols.Tls11));
#pragma warning restore SYSLIB0039
        TransportSecurityPolicy.EnsureTls12OrHigher(SslProtocols.Tls12 | SslProtocols.Tls13);
        var production = new AdmissionServerOptions
        {
            EnvironmentName = "Production",
            UseFixtureCalendar = true,
            EdgeTerminatesTls = true
        };
        Assert.Throws<InvalidOperationException>(() => production.Validate());

        var world = ServerWorld.Create();
        var vault = new MemorySecretsVault();
        vault.Set("api-token", "super-secret-token-value");
        world.App.Redactor.Track(vault.Get("api-token"));
        world.App.Logs.Write("token=" + vault.Get("api-token"));
        var driver = world.Register();
        world.App.Logs.Write("bearer " + driver.Token);
        var text = string.Join('\n', world.App.Logs.Lines);
        Assert.DoesNotContain("super-secret-token-value", text, StringComparison.Ordinal);
        Assert.DoesNotContain(driver.Token, text, StringComparison.Ordinal);
        Assert.Contains("[redacted]", text, StringComparison.Ordinal);
    }
}

/// <summary>TEST-RIDE-036. FR-RIDE-059, FR-RIDE-061. gRPC on .NET 10, sealed-only, no decrypt at ingest.</summary>
public class TestRide036Grpc
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-036")]
    [Trait("FR", "FR-RIDE-059")]
    [Trait("FR", "FR-RIDE-061")]
    [Trait("AC", "AC-RIDE-059-001")]
    [Trait("AC", "AC-RIDE-059-002")]
    [Trait("AC", "AC-RIDE-061-001")]
    public async Task Grpc_health_and_sealed_submit_do_not_decrypt()
    {
        var root = ServerWorld.RepoRoot();
        var docker = await File.ReadAllTextAsync(Path.Combine(root, "deploy/containers/admission/Dockerfile"));
        var props = await File.ReadAllTextAsync(Path.Combine(root, "Directory.Build.props"));
        Assert.Contains("mcr.microsoft.com/dotnet/aspnet:10.0", docker, StringComparison.Ordinal);
        Assert.Contains("mcr.microsoft.com/dotnet/sdk:10.0", docker, StringComparison.Ordinal);
        Assert.Contains("net10.0", props, StringComparison.Ordinal);
        var coordinator = await File.ReadAllTextAsync(Path.Combine(root, "src/RideAudit.Server.Admission/AdmissionCoordinator.cs"));
        Assert.DoesNotContain("AuthorizedDecryptor", coordinator, StringComparison.Ordinal);
        var ingest = await File.ReadAllTextAsync(Path.Combine(root, "src/RideAudit.Ingest/IngestSlice.cs"));
        Assert.DoesNotContain("lyft.com", ingest, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("undocumented Lyft private APIs", IngestSlice.Constraint, StringComparison.Ordinal);

        var world = ServerWorld.Create();
        var app = AdmissionHost.Build(world.Options, builder =>
        {
            builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, string.Empty);
            builder.WebHost.ConfigureKestrel(kestrel =>
            {
                kestrel.Listen(IPAddress.Loopback, 0, listen => listen.Protocols = HttpProtocols.Http2);
            });
        }, world.App);
        await app.StartAsync();
        try
        {
            var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            var address = addresses!.Addresses.Single(value => value.StartsWith("http://", StringComparison.Ordinal));
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress(address);
            var client = new RideAudit.Protos.Admission.V1.Admission.AdmissionClient(channel);
            var health = await client.HealthAsync(new HealthRequest());
            Assert.Equal("ok", health.Status);
            Assert.Equal("grpc-protobuf", health.ContractAuthority);
            Assert.Equal("non-authoritative-companion", health.OpenapiRole);
            Assert.Equal("net10.0", health.Framework);

            var enrolled = world.Enroll();
            var ready = world.SealReady(enrolled.Driver, enrolled.Session);
            var headers = new Metadata { { "authorization", "Bearer " + enrolled.Driver.Token } };
            var decision = await client.SubmitSealedAsync(new SubmitSealedRequest
            {
                SessionId = enrolled.Session.SessionId,
                VehicleId = enrolled.Vehicle.VehicleId,
                IdempotencyKey = "idem-grpc",
                ContentType = ready.Package.ContentType,
                SealedEnvelope = Google.Protobuf.ByteString.CopyFrom(ready.Package.EnvelopeBytes),
                ReceiptCore = ready.Package.Receipt.Core,
                Attestation = new AttestationSubmission
                {
                    Provider = RideAuditPolicy.PlayProvider,
                    Token = ready.Token,
                    Nonce = ready.Nonce,
                    BoundKeyId = ready.Package.KeyId,
                    BoundSessionId = enrolled.Session.SessionId,
                    PackageName = ServerWorld.PackageName,
                    CertDigest = ServerWorld.CertDigest
                }
            }, headers);
            Assert.True(decision.Admitted);
            Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);

            var rpc = await Assert.ThrowsAsync<RpcException>(async () => await client.SubmitSealedAsync(new SubmitSealedRequest
            {
                SessionId = enrolled.Session.SessionId,
                VehicleId = enrolled.Vehicle.VehicleId,
                IdempotencyKey = "idem-grpc-plain",
                ContentType = "video/mp4",
                SealedEnvelope = Google.Protobuf.ByteString.CopyFrom(new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 }),
                ReceiptCore = ready.Package.Receipt.Core
            }, headers));
            Assert.Equal(StatusCode.FailedPrecondition, rpc.StatusCode);
            Assert.Contains(ErrorCodes.PlaintextRejected, rpc.Status.Detail, StringComparison.Ordinal);
            Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);
        }
        finally
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }
}
