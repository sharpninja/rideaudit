using System.Text;
using RideAudit.Attest;
using RideAudit.Chain;
using RideAudit.Contracts;
using RideAudit.Seal;
using RideAudit.Server.Admission;
using RideAudit.Server.Identity;

namespace RideAudit.TestSupport;

public sealed record RegisteredDriver(string DriverId, string TenantId, string Email, string Token, string RecoveryCode);

public sealed class ServerWorld
{
    public static readonly DateTimeOffset Start = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);
    public const string PackageName = "app.rideaudit.capture";
    public const string CertDigest = "sha256:rideaudit-play-cert-fixture";

    private ServerWorld(AdmissionServerOptions options, AdmissionComposition app, ScriptedTimer timer)
    {
        Options = options;
        App = app;
        Timer = timer;
    }

    public AdmissionServerOptions Options { get; }
    public AdmissionComposition App { get; }
    public ScriptedTimer Timer { get; }
    public FakeClock Clock => App.Clock ?? throw new InvalidOperationException("Test world requires a fake clock.");

    public static ServerWorld Create(bool fixtureCalendar = true, bool fixturePlay = true, ScriptedTimer? timer = null, bool fixtureL2 = false)
    {
        var options = new AdmissionServerOptions
        {
            EnvironmentName = "Development",
            AllowInsecureDevHttp = true,
            UseFixtureCalendar = fixtureCalendar,
            UseFixtureL2 = fixtureL2,
            UseFixturePlayIntegrity = fixturePlay,
            EdgeTerminatesTls = false
        };
        var clock = new FakeClock(Start);
        var scripted = timer ?? new ScriptedTimer();
        var app = AdmissionHost.Compose(options, scripted, clock);
        return new ServerWorld(options, app, scripted);
    }

    public RegisteredDriver Register(string? email = null)
    {
        var result = App.Identity.Register(
            email ?? "driver-" + Guid.NewGuid().ToString("N")[..8] + "@example.com",
            "Test Driver",
            "US-CA",
            "court-audit",
            true,
            "I consent to sealed custody of this collection.");
        App.Redactor.Track(result.AccessToken);
        App.Redactor.Track(result.RecoveryCode);
        return new RegisteredDriver(result.DriverId, result.TenantId, result.Email, result.AccessToken, result.RecoveryCode);
    }

    public VehicleRecord AddVehicle(RegisteredDriver driver, string? vin = null)
    {
        var principal = Require(driver);
        return App.Identity.RegisterVehicle(principal, vin ?? "VIN-" + Guid.NewGuid().ToString("N")[..8], "Audit car", "Test", "Model", 2024, true);
    }

    public void PutProfile(RegisteredDriver driver, string vehicleId, string chainProfile = ChainProfileIds.BtcOts, bool valid = true)
    {
        App.Identity.PutProfile(Require(driver), vehicleId, "US-CA", chainProfile, valid, "test profile");
    }

    public AuditSessionRecord OpenSession(RegisteredDriver driver, string vehicleId)
    {
        return App.Identity.OpenSession(Require(driver), vehicleId, true);
    }

    public string IssueToken(string sessionId, string nonce, Action<FixturePlayIntegrityDecoder.FixturePayload>? mutate = null)
    {
        var payload = new FixturePlayIntegrityDecoder.FixturePayload
        {
            Provider = RideAuditPolicy.PlayProvider,
            Nonce = nonce,
            ObtainedUnixMillis = Clock.UtcNow.ToUnixTimeMilliseconds(),
            PackageName = PackageName,
            CertDigest = CertDigest,
            Verdict = RideAuditPolicy.MeetsDeviceIntegrity,
            BoundSessionId = sessionId
        };
        mutate?.Invoke(payload);
        var token = FixturePlayIntegrityDecoder.Issue(payload);
        App.Redactor.Track(token);
        return token;
    }

    public SealedPackage Seal(RegisteredDriver driver, AuditSessionRecord session, byte[]? plaintext = null, string? token = null, string? nonce = null, string keyScope = "session")
    {
        nonce ??= "nonce-" + Guid.NewGuid().ToString("N");
        token ??= IssueToken(session.SessionId, nonce);
        var package = App.Sealer.Seal(new SealRequest
        {
            Plaintext = plaintext ?? Encoding.UTF8.GetBytes("rideaudit-sample-plaintext-" + Guid.NewGuid().ToString("N")),
            KeyScope = keyScope,
            ScopeBinding = session.SessionId,
            TenantId = driver.TenantId,
            DriverId = driver.DriverId,
            CollectorId = driver.DriverId,
            VehicleId = session.VehicleId,
            SessionId = session.SessionId,
            ProvenanceTag = RideAuditPolicy.ProvenanceTag,
            PolicyVersion = Options.PolicyVersion,
            AttestationToken = token,
            ExpectedNonce = nonce,
            CustodianIds = ["custodian-a", "custodian-b", "custodian-c"]
        });
        return package;
    }

    public (SealedPackage Package, string Token, string Nonce, byte[] Plaintext) SealReady(RegisteredDriver driver, AuditSessionRecord session, byte[]? plaintext = null)
    {
        var nonce = "nonce-" + Guid.NewGuid().ToString("N");
        var token = IssueToken(session.SessionId, nonce);
        var body = plaintext ?? Encoding.UTF8.GetBytes("rideaudit-sample-plaintext-" + Guid.NewGuid().ToString("N"));
        var package = Seal(driver, session, body, token, nonce);
        return (package, token, nonce, body);
    }

    public AdmissionOutcome Submit(RegisteredDriver driver, SealedPackage package, string token, string nonce, string? idempotencyKey = null)
    {
        return App.Admission.Submit(Command(driver, package, token, nonce, idempotencyKey));
    }

    public SubmitSealedCommand Command(RegisteredDriver driver, SealedPackage package, string token, string nonce, string? idempotencyKey = null)
    {
        var parsed = EnvelopeFormat.Parse(package.EnvelopeBytes);
        var command = new SubmitSealedCommand
        {
            Principal = Require(driver),
            RawBearerToken = driver.Token,
            ClientIp = "127.0.0.1",
            IdempotencyKey = idempotencyKey ?? Ids.New("idem-"),
            ContentType = package.ContentType,
            EnvelopeBytes = package.EnvelopeBytes,
            SubmittedReceiptBytes = package.ReceiptCoreBytes,
            AttestationToken = token,
            AttestationNonce = nonce,
            BoundKeyId = parsed.Header.KeyId,
            SessionId = parsed.Header.SessionId,
            VehicleId = parsed.Header.VehicleId
        };
        return command;
    }

    public (RegisteredDriver Driver, VehicleRecord Vehicle, AuditSessionRecord Session) Enroll()
    {
        var driver = Register();
        var vehicle = AddVehicle(driver);
        PutProfile(driver, vehicle.VehicleId);
        var session = OpenSession(driver, vehicle.VehicleId);
        return (driver, vehicle, session);
    }

    public DriverPrincipal Require(RegisteredDriver driver) =>
        App.Identity.Authenticate(driver.Token) ?? throw new InvalidOperationException("Token did not authenticate.");

    public static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "RideAudit.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("RideAudit.sln was not found.");
    }
}
