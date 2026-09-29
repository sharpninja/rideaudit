using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RideAudit.Bt;
using RideAudit.Capture;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.Client.Seal;
using RideAudit.Contracts;
using RideAudit.PlayIntegrity;
using RideAudit.Seal;
using RideAudit.TestSupport;
using RideAudit.Video;

namespace RideAudit.Server.Admission.Tests;

/// <summary>
/// Dual-phone capture submits an untouched RAES envelope through GrpcSealedAdmissionClient
/// to in-process AdmissionHost. Fixture.v1. attestation and in-process HSM escrow only.
/// </summary>
public class CaptureGrpcAdmissionTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-035")]
    public async Task Capture_session_admits_sealed_raes_over_grpc()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var (publicKey, privateKey) = EscrowKeyFactory.CreateEphemeral("escrow-capture");
        var clock = new FixedClock(ServerWorld.Start);
        var play = new FixturePlayIntegrityClient();
        var allowlist = PackageAllowlist.CreateDevelopmentDefault();
        var frames = new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(33), TimeSpan.FromMilliseconds(66) };
        var driver = new PhoneNode("aa:bb:cc:dd:ee:01", "driver-phone", PhoneRole.Driver);
        var passenger = new PhoneNode("aa:bb:cc:dd:ee:02", "passenger-phone", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, enrolled.Session.SessionId);
        passenger.Confirm(PhoneRole.Passenger, enrolled.Session.SessionId);

        var factory = new ServerAdmissionRequestFactory(new ServerAdmissionBinding
        {
            TenantId = enrolled.Driver.TenantId,
            DriverId = enrolled.Driver.DriverId,
            PolicyVersion = world.Options.PolicyVersion,
            PackageIdentity = ServerWorld.PackageName,
            SigningCertDigest = ServerWorld.CertDigest,
            IssueToken = (sessionId, nonce) => world.IssueToken(sessionId, nonce),
            ObtainedUnixMillis = world.Clock.UtcNow.ToUnixTimeMilliseconds()
        });

        await using var host = await RaesHelpers.Start(world);
        using var transport = RaesHelpers.Client(host, enrolled.Driver.Token);

        var request = new CaptureRequest
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
            DriverStream = Stream("driver-stream", "device-driver", frames, "capture-driver"),
            PassengerStream = Stream("passenger-stream", "device-passenger", frames, "capture-passenger"),
            Samples =
            [
                new TelematicsSample(TimeSpan.Zero, 0.1, 0.2, 0.9, 1.0, null, null, null),
                new TelematicsSample(TimeSpan.FromMilliseconds(66), 0.4, -0.2, 0.8, 2.0, null, null, null)
            ],
            SealRaw = false,
            RawConsent = false,
            SourceCommitNotice = "capture-e2e",
            RequestFactory = factory,
            EscrowDeposit = new HsmRaesDeposit(world, enrolled.Driver.TenantId, privateKey)
        };

        var session = new DualPhoneCaptureSession(new InMemoryDiscoveryBus(), transport.Admission, clock, factory);
        var capture = session.Run(request);

        Assert.True(capture.Submission.Response.Admitted);
        Assert.Equal("", capture.Submission.Response.RejectCode);
        Assert.StartsWith("fixture.v1.", capture.Submission.Request.Attestation.Token, StringComparison.Ordinal);
        Assert.True(RaesEnvelopeFormat.HasMagic(capture.SealedComposite.Envelope));
        Assert.False(capture.SealedComposite.Envelope.AsSpan().StartsWith(EnvelopeFormat.Magic));
        Assert.False(capture.Submission.Response.Anchor.LiveBitcoinMetadata);
        Assert.Equal(ProofSources.DocumentedFixture, capture.Submission.Response.Anchor.ProofSource);
        Assert.StartsWith("fixture:", capture.Submission.Response.Anchor.TransactionReference, StringComparison.Ordinal);

        var stored = world.App.Journal.Find(capture.Submission.Response.SubmissionId);
        Assert.NotNull(stored);
        Assert.True(stored!.EnvelopeBytes.AsSpan().SequenceEqual(capture.SealedComposite.Envelope));
        Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);
    }

    [Fact]
    public void Capture_admission_channel_fail_closes_without_endpoint()
    {
        var missing = Assert.Throws<RideAuditFailClosedException>(() =>
            CaptureAdmissionChannel.Connect(new CaptureAdmissionOptions()));
        Assert.Equal(ErrorCodes.AdmissionUnavailable, missing.Code);

        var noBearer = Assert.Throws<RideAuditFailClosedException>(() =>
            CaptureAdmissionChannel.Connect(new CaptureAdmissionOptions { AdmissionAddress = "http://127.0.0.1:1" }));
        Assert.Equal(ErrorCodes.AuthRequired, noBearer.Code);
    }

    [Fact]
    public void Edge_tls_probe_refuses_plaintext_and_ngrok()
    {
        var plaintext = Assert.Throws<RideAuditFailClosedException>(() =>
            CaptureAdmissionChannel.RequireEdgeTlsAddress("http://192.168.0.149:28443"));
        Assert.Equal(ErrorCodes.AdmissionUnavailable, plaintext.Code);
        Assert.Contains("non-https", plaintext.Message, StringComparison.Ordinal);

        var admissionPort = Assert.Throws<RideAuditFailClosedException>(() =>
            CaptureAdmissionChannel.RequireEdgeTlsAddress("https://192.168.0.149:28080"));
        Assert.Contains("28080", admissionPort.Message, StringComparison.Ordinal);
        var counselPort = Assert.Throws<RideAuditFailClosedException>(() =>
            CaptureAdmissionChannel.RequireEdgeTlsAddress("https://192.168.0.149:28081"));
        Assert.Contains("28081", counselPort.Message, StringComparison.Ordinal);
        Assert.Throws<RideAuditFailClosedException>(() =>
            CaptureAdmissionChannel.RequireEdgeTlsAddress("https://lab.ngrok.io"));
        CaptureAdmissionChannel.RequireEdgeTlsAddress("https://192.168.0.149:28443");
    }

    [Fact]
    public void Lab_root_trust_rejects_unrelated_and_name_mismatch()
    {
        using var root = LoadPem("caddy-lab-root.pem");
        using var unrelated = CreateSelfSigned("unrelated.example");
        Assert.False(LabRootTrust.Accepts(
            unrelated,
            presented: null,
            SslPolicyErrors.RemoteCertificateChainErrors,
            root));
        Assert.False(LabRootTrust.Accepts(
            root,
            presented: null,
            SslPolicyErrors.RemoteCertificateNameMismatch,
            root));
        Assert.True(LabRootTrust.Accepts(
            root,
            presented: null,
            SslPolicyErrors.RemoteCertificateChainErrors,
            root));
        using var intermediate = LoadPem("caddy-lab-intermediate.pem");
        Assert.True(LabRootTrust.Accepts(
            intermediate,
            presented: null,
            SslPolicyErrors.RemoteCertificateChainErrors,
            root,
            new[] { intermediate },
            out var detail));
        Assert.Contains("root", detail, StringComparison.OrdinalIgnoreCase);
    }

    private static X509Certificate2 LoadPem(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "RideAudit.Client.Android", fileName);
            if (File.Exists(candidate))
            {
                return X509Certificate2.CreateFromPem(File.ReadAllText(candidate));
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(fileName);
    }

    private static X509Certificate2 CreateSelfSigned(string commonName)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=" + commonName,
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using var created = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        return X509CertificateLoader.LoadCertificate(created.Export(X509ContentType.Cert));
    }

    private static SourceStream Stream(string id, string device, IReadOnlyList<TimeSpan> frames, string payload) =>
        new(
            id,
            device,
            "attest-ref",
            new CameraMetadata(id + "-cam", "rear", 1280, 720),
            frames,
            Encoding.UTF8.GetBytes(payload));

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
