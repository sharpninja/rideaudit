using System.Security.Cryptography;
using System.Text;
using RideAudit.Bt;
using RideAudit.Capture;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.Client.Seal;
using RideAudit.Contracts;
using RideAudit.PlayIntegrity;
using RideAudit.Protos.Admission.V1;
using RideAudit.Server.Identity;
using RideAudit.TestSupport;
using RideAudit.Video;

namespace RideAudit.Server.Admission.Tests;

/// <summary>
/// B03: CanonicalAdmissionRequestFactory builds the request; AdmissionGrpcService
/// is reached through an in-process Kestrel server. ServerAdmissionRequestFactory
/// is not used on this path.
/// </summary>
public class CanonicalGrpcAdmissionTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-035")]
    [Trait("FR", "FR-RIDE-036")]
    [Trait("TR", "TR-RIDE-SERVER-003")]
    [Trait("TR", "TR-RIDE-SERVER-004")]
    [Trait("AC", "AC-RIDE-035-001")]
    [Trait("AC", "AC-RIDE-035-002")]
    [Trait("AC", "AC-RIDE-SERVER-003-001")]
    [Trait("AC", "AC-RIDE-SERVER-004-001")]
    [Trait("AC", "AC-RIDE-PLAY-002-001")]
    public async Task Canonical_factory_request_is_admitted_through_admission_grpc_service()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var (publicKey, privateKey) = EscrowKeyFactory.CreateEphemeral("escrow-canonical-grpc");
        var clock = new FixedClock(ServerWorld.Start);
        var allowlist = new PackageAllowlist(1, [new AllowlistEntry(ServerWorld.PackageName, ServerWorld.CertDigest)]);
        var play = new IssuedFixturePlayClient(world, enrolled.Session.SessionId);
        var frames = new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(33), TimeSpan.FromMilliseconds(66) };
        var driver = new PhoneNode("aa:bb:cc:dd:ee:11", "driver-phone", PhoneRole.Driver);
        var passenger = new PhoneNode("aa:bb:cc:dd:ee:12", "passenger-phone", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, enrolled.Session.SessionId);
        passenger.Confirm(PhoneRole.Passenger, enrolled.Session.SessionId);

        var factory = new CanonicalAdmissionRequestFactory(new CanonicalAdmissionIdentity
        {
            TenantId = enrolled.Driver.TenantId,
            DriverId = enrolled.Driver.DriverId,
            PolicyVersion = world.Options.PolicyVersion
        });

        await using var host = await RaesHelpers.Start(world);
        using var transport = RaesHelpers.Client(host, enrolled.Driver.Token);

        var capture = new DualPhoneCaptureSession(new InMemoryDiscoveryBus(), transport.Admission, clock, factory).Run(
            new CaptureRequest
            {
                Driver = driver,
                Passenger = passenger,
                SessionId = enrolled.Session.SessionId,
                VehicleId = enrolled.Vehicle.VehicleId,
                CollectorIdentity = enrolled.Driver.DriverId,
                DriverPlay = play,
                PassengerPlay = play,
                Allowlist = allowlist,
                Clock = clock,
                Probe = new ExplicitDeviceProbe(12, 80_000_000, 77, 36),
                Escrow = publicKey,
                DriverStream = Stream("driver-stream", "device-driver", frames, "canonical-grpc-driver"),
                PassengerStream = Stream("passenger-stream", "device-passenger", frames, "canonical-grpc-passenger"),
                Samples = [new TelematicsSample(TimeSpan.Zero, 0.1, 0.2, 0.9, 1.0, null, null, null)],
                SealRaw = false,
                RawConsent = false,
                TenantId = enrolled.Driver.TenantId,
                DriverId = enrolled.Driver.DriverId,
                PolicyVersion = world.Options.PolicyVersion,
                RequestFactory = factory,
                EscrowDeposit = new HsmRaesDeposit(world, enrolled.Driver.TenantId, privateKey)
            });

        Assert.True(capture.Submission.Response.Admitted);
        Assert.Equal("", capture.Submission.Response.RejectCode);
        Assert.Equal(enrolled.Driver.TenantId, capture.Submission.Request.ReceiptCore.TenantId);
        Assert.Equal(enrolled.Driver.DriverId, capture.Submission.Request.ReceiptCore.DriverId);
        Assert.StartsWith("fixture.v1.", capture.Submission.Request.Attestation.Token, StringComparison.Ordinal);
        Assert.NotEqual(capture.SealedComposite.Receipt.AttestationTokenHash, capture.Submission.Request.Attestation.Token);
        Assert.False(capture.Submission.Response.Anchor.LiveBitcoinMetadata);
        Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);
        Assert.DoesNotContain("ServerAdmissionRequestFactory", nameof(CanonicalAdmissionRequestFactory));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-035")]
    [Trait("AC", "AC-RIDE-035-001")]
    [Trait("AC", "AC-RIDE-SERVER-003-002")]
    [Trait("AC", "AC-RIDE-SERVER-004-002")]
    public async Task Canonical_factory_request_is_rejected_over_grpc_without_raw_token()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var factory = new CanonicalAdmissionRequestFactory(new CanonicalAdmissionIdentity
        {
            TenantId = enrolled.Driver.TenantId,
            DriverId = enrolled.Driver.DriverId,
            PolicyVersion = world.Options.PolicyVersion
        });
        var sealedRecord = RaesHelpers.SealRaes(enrolled.Driver.DriverId, enrolled.Session.SessionId).Record;
        var request = factory.Create(
            sealedRecord,
            MinimalRequest(enrolled, factory),
            new AttestationEvidence(
                RideAuditPolicy.PlayProvider,
                sealedRecord.Receipt.AttestationTokenHash,
                ready.Nonce,
                world.Clock.UtcNow,
                ServerWorld.PackageName,
                ServerWorld.CertDigest,
                RideAuditPolicy.MeetsDeviceIntegrity,
                true,
                true,
                null)
            {
                RawTokenMaterial = ready.Token
            });

        Assert.StartsWith("fixture.v1.", request.Attestation.Token, StringComparison.Ordinal);

        request.Attestation.Token = sealedRecord.Receipt.AttestationTokenHash;
        request.IdempotencyKey = "idem-canonical-hash-only";

        await using var host = await RaesHelpers.Start(world);
        using var transport = RaesHelpers.Client(host, enrolled.Driver.Token);
        var decision = transport.Admission.SubmitSealed(request);
        Assert.False(decision.Admitted);
        Assert.Equal(ErrorCodes.AttestationFailed, decision.RejectCode);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-026")]
    [Trait("AC", "AC-RIDE-PLAY-001-002")]
    public void Canonical_factory_still_refuses_hash_only_token_material()
    {
        var factory = new CanonicalAdmissionRequestFactory(new CanonicalAdmissionIdentity
        {
            TenantId = "tenant-canonical",
            DriverId = "driver-canonical",
            PolicyVersion = RideAuditPolicy.Version
        });
        var fixture = RaesHelpers.SealRaes("driver-canonical", "session-canonical");
        var ex = Assert.Throws<RideAuditFailClosedException>(() =>
            factory.Create(
                fixture.Record,
                new CaptureRequest
                {
                    Driver = new PhoneNode("aa:bb:cc:dd:ee:11", "driver-phone", PhoneRole.Driver),
                    Passenger = new PhoneNode("aa:bb:cc:dd:ee:12", "passenger-phone", PhoneRole.Passenger),
                    SessionId = "session-canonical",
                    VehicleId = "vehicle-1",
                    CollectorIdentity = "driver-canonical",
                    DriverPlay = new FixturePlayIntegrityClient(),
                    PassengerPlay = new FixturePlayIntegrityClient(),
                    Allowlist = PackageAllowlist.CreateDevelopmentDefault(),
                    Clock = new FixedClock(ServerWorld.Start),
                    Probe = new ExplicitDeviceProbe(12, 80_000_000, 77, 36),
                    Escrow = new EscrowPublicKey("escrow-1", fixture.Record.Receipt.PublicKeyPem),
                    DriverStream = Stream("d", "dev-d", [TimeSpan.Zero], "d"),
                    PassengerStream = Stream("p", "dev-p", [TimeSpan.Zero], "p"),
                    Samples = [],
                    SealRaw = false,
                    RawConsent = false,
                    TenantId = "tenant-canonical",
                    DriverId = "driver-canonical",
                    PolicyVersion = RideAuditPolicy.Version
                },
                new AttestationEvidence(
                    PlayIntegrityProviders.Fixture,
                    fixture.Record.Receipt.AttestationTokenHash,
                    fixture.Record.Receipt.Nonce,
                    ServerWorld.Start,
                    ApprovedPackage.PackageName,
                    ApprovedPackage.CertDigest,
                    "MEETS_DEVICE_INTEGRITY",
                    true,
                    true,
                    FixturePlayIntegrityClient.Notice)));
        Assert.Equal(ErrorCodes.AttestationFailed, ex.Code);
    }

    private static CaptureRequest MinimalRequest(
        (RegisteredDriver Driver, VehicleRecord Vehicle, AuditSessionRecord Session) enrolled,
        IAdmissionRequestFactory factory)
    {
        var driver = new PhoneNode("aa:bb:cc:dd:ee:11", "driver-phone", PhoneRole.Driver);
        var passenger = new PhoneNode("aa:bb:cc:dd:ee:12", "passenger-phone", PhoneRole.Passenger);
        return new CaptureRequest
        {
            Driver = driver,
            Passenger = passenger,
            SessionId = enrolled.Session.SessionId,
            VehicleId = enrolled.Vehicle.VehicleId,
            CollectorIdentity = enrolled.Driver.DriverId,
            DriverPlay = new FixturePlayIntegrityClient(),
            PassengerPlay = new FixturePlayIntegrityClient(),
            Allowlist = PackageAllowlist.CreateDevelopmentDefault(),
            Clock = new FixedClock(ServerWorld.Start),
            Probe = new ExplicitDeviceProbe(12, 80_000_000, 77, 36),
            Escrow = new EscrowPublicKey("escrow-min", "-----BEGIN PUBLIC KEY-----\nMIIB\n-----END PUBLIC KEY-----"),
            DriverStream = Stream("d", "dev-d", [TimeSpan.Zero], "d"),
            PassengerStream = Stream("p", "dev-p", [TimeSpan.Zero], "p"),
            Samples = [],
            SealRaw = false,
            RawConsent = false,
            TenantId = enrolled.Driver.TenantId,
            DriverId = enrolled.Driver.DriverId,
            PolicyVersion = RideAuditPolicy.Version,
            RequestFactory = factory
        };
    }

    private static SourceStream Stream(string id, string device, IReadOnlyList<TimeSpan> frames, string payload) =>
        new(
            id,
            device,
            "attest-ref",
            new CameraMetadata(id + "-cam", "rear", 1280, 720),
            frames,
            Encoding.UTF8.GetBytes(payload));

    private sealed class IssuedFixturePlayClient : IPlayIntegrityClient
    {
        private readonly ServerWorld _world;
        private readonly string _sessionId;

        public IssuedFixturePlayClient(ServerWorld world, string sessionId)
        {
            _world = world;
            _sessionId = sessionId;
        }

        public PlayTokenResult RequestToken(string nonce, RideAudit.Client.Core.IClock clock)
        {
            var token = _world.IssueToken(_sessionId, nonce);
            return new PlayTokenResult(
                RideAuditPolicy.PlayProvider,
                token,
                nonce,
                clock.UtcNow,
                ServerWorld.PackageName,
                ServerWorld.CertDigest,
                RideAuditPolicy.MeetsDeviceIntegrity,
                true,
                true,
                "Documented fixture.v1. token for in-process AdmissionGrpcService. Not a live Play Integrity JWT.");
        }
    }

    private sealed class HsmRaesDeposit : IDeviceEscrowDeposit
    {
        private readonly ServerWorld _world;
        private readonly string _tenantId;
        private readonly RSA _privateKey;

        public HsmRaesDeposit(ServerWorld world, string tenantId, RSA privateKey)
        {
            _world = world;
            _tenantId = tenantId;
            _privateKey = privateKey;
        }

        public void Deposit(SealedRecord record) =>
            RaesHelpers.Escrow(_world, _tenantId, record, _privateKey);
    }
}
