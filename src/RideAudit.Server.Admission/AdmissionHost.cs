using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using RideAudit.Anal;
using RideAudit.Attest;
using RideAudit.Chain;
using RideAudit.Chain.EthL2;
using RideAudit.Chain.OpenTimestamps;
using RideAudit.Contracts;
using RideAudit.Escrow;
using RideAudit.Ingest;
using RideAudit.Privacy;
using RideAudit.Seal;
using RideAudit.Sec;
using RideAudit.Server.Counsel;
using RideAudit.Server.Identity;

namespace RideAudit.Server.Admission;

public sealed class AdmissionServerOptions
{
    public string EnvironmentName { get; init; } = "Production";
    public bool AllowInsecureDevHttp { get; init; }
    public bool UseFixtureCalendar { get; init; }
    public bool UseFixtureL2 { get; init; }
    public bool UseFixturePlayIntegrity { get; init; }
    public bool EdgeTerminatesTls { get; init; }
    public string? CertificatePath { get; init; }
    public string? OtsCalendarSetting { get; init; }
    public string? L2CalendarSetting { get; init; }
    public string? L2RpcSetting { get; init; }
    public string PolicyVersion { get; init; } = RideAuditPolicy.Version;
    public SslProtocols EnabledProtocols { get; init; } = SslProtocols.Tls12 | SslProtocols.Tls13;
    public int RateLimitPerMinute { get; init; } = 60;
    public int MaxPayloadBytes { get; init; } = 32 * 1024 * 1024;

    public static AdmissionServerOptions FromEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var ots = Environment.GetEnvironmentVariable("RIDEAUDIT_OTS_CALENDAR");
        var l2 = Environment.GetEnvironmentVariable("RIDEAUDIT_L2_CALENDAR");
        var l2Rpc = Environment.GetEnvironmentVariable("RIDEAUDIT_L2_RPC");
        return new AdmissionServerOptions
        {
            EnvironmentName = environment,
            AllowInsecureDevHttp = Flag("RIDEAUDIT_ALLOW_INSECURE_DEV_HTTP"),
            OtsCalendarSetting = ots,
            L2CalendarSetting = l2,
            L2RpcSetting = l2Rpc,
            UseFixtureCalendar = CalendarEndpoints.IsDocumentedFixture(ots),
            UseFixtureL2 = CalendarEndpoints.IsDocumentedFixture(l2),
            UseFixturePlayIntegrity = string.Equals(Environment.GetEnvironmentVariable("RIDEAUDIT_PLAY_INTEGRITY"), "fixture", StringComparison.Ordinal),
            EdgeTerminatesTls = Flag("RIDEAUDIT_EDGE_TLS"),
            CertificatePath = Environment.GetEnvironmentVariable("RIDEAUDIT_TLS_CERT_PATH"),
            EnabledProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        };
    }

    public void Validate()
    {
        TransportSecurityPolicy.EnsureTls12OrHigher(EnabledProtocols);
        var production = string.Equals(EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase);
        if (production && AllowInsecureDevHttp)
            throw new InvalidOperationException("Production refuses insecure HTTP.");
        if (production && UseFixtureCalendar)
            throw new InvalidOperationException("Production refuses the documented OTS fixture calendar.");
        if (production && UseFixtureL2)
            throw new InvalidOperationException("Production refuses the documented L2 fixture calendar.");
        if (production && UseFixturePlayIntegrity)
            throw new InvalidOperationException("Production refuses the Play Integrity fixture decoder.");
        if (production && string.IsNullOrWhiteSpace(CertificatePath) && !EdgeTerminatesTls)
            throw new InvalidOperationException("Production requires a TLS certificate or RIDEAUDIT_EDGE_TLS=true with TLS 1.2+ at the edge.");
    }

    private static bool Flag(string name) =>
        string.Equals(Environment.GetEnvironmentVariable(name), "true", StringComparison.OrdinalIgnoreCase);
}

public static class TransportSecurityPolicy
{
    public static void EnsureTls12OrHigher(SslProtocols protocols)
    {
        var allowed = SslProtocols.Tls12 | SslProtocols.Tls13;
        if ((protocols & allowed) == 0 || (protocols & ~allowed) != 0)
            throw new InvalidOperationException("TLS 1.2+ is required.");
    }
}

public sealed class AdmissionComposition
{
    public required FakeClock? Clock { get; init; }
    public required IClock ClockService { get; init; }
    public required SecretRedactor Redactor { get; init; }
    public required InMemoryLogSink Logs { get; init; }
    public required HsmKeyCustody Hsm { get; init; }
    public required DocumentedFixtureOtsCalendar? Calendar { get; init; }
    public required IOtsCalendar? OtsCalendar { get; init; }
    public required DriverDirectory Identity { get; init; }
    public required AdmissionCoordinator Admission { get; init; }
    public required PlayIntegrityVerifier Play { get; init; }
    public required PackageAllowlist Allowlist { get; init; }
    public required AppendOnlyAccessLog AccessLog { get; init; }
    public required CustodyJournal Journal { get; init; }
    public required OperatorAlertSink Alerts { get; init; }
    public required AbuseGuard Abuse { get; init; }
    public required LatencyMonitor Latency { get; init; }
    public required AnchoringPolicy Anchoring { get; init; }
    public required IElapsedTimer Timer { get; init; }
    public required AlgorithmRegistry Algorithms { get; init; }
    public required CollectionBoundarySealer Sealer { get; init; }
    public required DocumentedFixtureL2Calendar? L2 { get; init; }
    public required IEthL2Client? L2Client { get; init; }
    public required NormalizedStore Imports { get; init; }
    public required IngestPipeline Ingest { get; init; }
    public required AnalysisService Analysis { get; init; }
    public required CounselDesk Counsel { get; init; }
    public required PrivacyDesk Privacy { get; init; }
}

public static class AdmissionHost
{
    public static AdmissionComposition Compose(AdmissionServerOptions options, IElapsedTimer? timer = null, FakeClock? clock = null)
    {
        options.Validate();
        IClock clockService = clock is null ? new SystemClock() : clock;
        var redactor = new SecretRedactor();
        var logs = new InMemoryLogSink(redactor);
        var access = new AppendOnlyAccessLog();
        var database = new InMemoryApplicationDatabase();
        var identity = new DriverDirectory(clockService, access, database);
        var hsm = new HsmKeyCustody(clockService);
        var allowlist = new PackageAllowlist(
            "allowlist-1",
            new[] { new AllowlistEntry("app.rideaudit.capture", "sha256:rideaudit-play-cert-fixture") },
            "system",
            clockService.UtcNow.ToUnixTimeMilliseconds());
        IPlayIntegrityTokenDecoder decoder = options.UseFixturePlayIntegrity
            ? new FixturePlayIntegrityDecoder()
            : new FailClosedPlayIntegrityDecoder();
        var play = new PlayIntegrityVerifier(decoder, allowlist, clockService);
        var otsCalendar = ChainEndpointFactory.CreateOts(options);
        var l2Client = ChainEndpointFactory.CreateL2(options);
        var calendar = otsCalendar as DocumentedFixtureOtsCalendar;
        var l2 = l2Client as DocumentedFixtureL2Calendar;
        var ots = new BtcOtsAnchor(otsCalendar);
        var latency = new LatencyMonitor();
        var elapsed = timer ?? new StopwatchTimer();
        var anchoring = new AnchoringPolicy(latency, elapsed, TimeSpan.FromMinutes(2));
        var journal = new CustodyJournal();
        var abuse = new AbuseGuard(clockService)
        {
            LimitPerMinute = options.RateLimitPerMinute,
            MaxPayloadBytes = options.MaxPayloadBytes
        };
        var alerts = new OperatorAlertSink();
        var archive = new AttestationArchive();
        var admission = new AdmissionCoordinator(
            identity, play, ots, anchoring, hsm, journal, abuse, alerts, logs, redactor, access, clockService, archive, options.PolicyVersion, l2Client);
        var imports = new NormalizedStore();
        var keys = new ImportKeyRing();
        var ingest = new IngestPipeline(imports, keys, clockService);
        var analysis = new AnalysisService(imports);
        var recordSource = new JournalRecordSource(journal);
        var counsel = new CounselDesk(recordSource);
        var privacy = new PrivacyDesk(imports, keys, access, clockService, recordSource, new DirectorySubjectAccountSource(identity));
        var algorithms = new AlgorithmRegistry(RideAuditPolicy.AlgorithmId);
        var sealer = new CollectionBoundarySealer(play, hsm, hsm, clockService, algorithms);
        return new AdmissionComposition
        {
            Clock = clock,
            ClockService = clockService,
            Redactor = redactor,
            Logs = logs,
            Hsm = hsm,
            Calendar = calendar,
            OtsCalendar = otsCalendar,
            Identity = identity,
            Admission = admission,
            Play = play,
            Allowlist = allowlist,
            AccessLog = access,
            Journal = journal,
            Alerts = alerts,
            Abuse = abuse,
            Latency = latency,
            Anchoring = anchoring,
            Timer = elapsed,
            Algorithms = algorithms,
            Sealer = sealer,
            L2 = l2,
            L2Client = l2Client,
            Imports = imports,
            Ingest = ingest,
            Analysis = analysis,
            Counsel = counsel,
            Privacy = privacy
        };
    }

    public static WebApplication Build(AdmissionServerOptions options, Action<WebApplicationBuilder>? configure = null, AdmissionComposition? composition = null)
    {
        options.Validate();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = options.EnvironmentName });
        builder.WebHost.ConfigureKestrel(kestrel =>
        {
            kestrel.ConfigureEndpointDefaults(endpoint =>
            {
                endpoint.Protocols = HttpProtocols.Http1AndHttp2;
            });
        });
        configure?.Invoke(builder);
        composition ??= Compose(options);
        builder.Services.AddSingleton(composition);
        builder.Services.AddSingleton(composition.Identity);
        builder.Services.AddSingleton(composition.Admission);
        builder.Services.AddSingleton(composition.Hsm);
        builder.Services.AddSingleton(composition.Journal);
        builder.Services.AddSingleton(composition.Redactor);
        builder.Services.AddSingleton(composition.Logs);
        builder.Services.AddGrpc(grpc => grpc.Interceptors.Add<RideAuditExceptionInterceptor>());
        builder.Services.AddSingleton<RideAuditExceptionInterceptor>();
        var app = builder.Build();
        app.MapGrpcService<IdentityGrpcService>();
        app.MapGrpcService<AdmissionGrpcService>();
        app.MapGrpcService<EscrowGrpcService>();
        app.MapGrpcService<CounselGrpcService>();
        app.MapGrpcService<IngestGrpcService>();
        app.MapGrpcService<PrivacyGrpcService>();
        app.MapGet("/", () => Results.Text(
            "RideAudit admission gRPC. Contract authority: grpc-protobuf. OpenAPI is a non-authoritative companion.",
            "text/plain"));
        return app;
    }
}

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var options = AdmissionServerOptions.FromEnvironment();
        var app = AdmissionHost.Build(options);
        await app.RunAsync();
    }
}
