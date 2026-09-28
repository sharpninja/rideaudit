// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using RideAudit.Bt;
using RideAudit.Capture;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.Client.Seal;
using RideAudit.Client.Tests.Support;
using RideAudit.Contracts;
using RideAudit.PlayIntegrity;
using RideAudit.Shared.Ui;
using RideAudit.Shared.Ui.Views;
using RideAudit.Video;
using RideAudit.Viewer;
using Xunit;

namespace RideAudit.Client.Tests;

public class PathTo98RemediationTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-025")]
    [Trait("AC", "AC-RIDE-025-002")]
    public void Fixture_play_fails_court_ready_play_attestation()
    {
        var fixture = Fixtures.CaptureHappy();
        Assert.Equal(PlayIntegrityProviders.Fixture, fixture.Capture.SealedComposite.Receipt.AttestationProvider);
        Assert.False(string.IsNullOrEmpty(fixture.Capture.SealedComposite.Receipt.StubNotice));
        var outcome = Fixtures.ViewerFor(fixture).Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        Assert.Contains(outcome.Report.Checks, check => check.Name == "play_attestation" && !check.Passed);
        Assert.False(outcome.DisplayAllowed);
        Assert.Equal(ReviewPhase.FailClosed, outcome.Phase);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-035")]
    [Trait("AC", "AC-RIDE-035-002")]
    public void Canonical_admission_sends_tenant_and_raw_token_not_hash()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 9, 28, 12, 0, 0, TimeSpan.Zero));
        var allowlist = PackageAllowlist.CreateDevelopmentDefault();
        var (publicKey, _) = EscrowKeyFactory.CreateEphemeral("escrow-canonical");
        var driver = new PhoneNode("aa:bb:cc:dd:ee:11", "driver-phone", PhoneRole.Driver);
        var passenger = new PhoneNode("aa:bb:cc:dd:ee:12", "passenger-phone", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, "intent-canonical");
        passenger.Confirm(PhoneRole.Passenger, "intent-canonical");
        var frames = new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(33), TimeSpan.FromMilliseconds(66) };
        var factory = new CanonicalAdmissionRequestFactory(new CanonicalAdmissionIdentity
        {
            TenantId = "tenant-canonical",
            DriverId = "driver-canonical",
            PolicyVersion = RideAuditPolicy.Version
        });
        var session = new DualPhoneCaptureSession(
            new InMemoryDiscoveryBus(),
            new InterimInProcessAdmissionClient(),
            clock,
            factory);
        var capture = session.Run(new CaptureRequest
        {
            Driver = driver,
            Passenger = passenger,
            SessionId = "session-canonical",
            VehicleId = "vehicle-1",
            CollectorIdentity = "driver-1",
            DriverPlay = new FixturePlayIntegrityClient(),
            PassengerPlay = new FixturePlayIntegrityClient(),
            Allowlist = allowlist,
            Clock = clock,
            Probe = new ExplicitDeviceProbe(12, 80_000_000, 77, 36),
            Escrow = publicKey,
            DriverStream = Stream("driver-stream", "device-driver", frames, "canonical-driver"),
            PassengerStream = Stream("passenger-stream", "device-passenger", frames, "canonical-passenger"),
            Samples = [new TelematicsSample(TimeSpan.Zero, 0.1, 0.2, 0.9, 1.0, null, null, null)],
            SealRaw = false,
            RawConsent = false,
            TenantId = "tenant-canonical",
            DriverId = "driver-canonical",
            PolicyVersion = RideAuditPolicy.Version,
            RequestFactory = factory
        });

        Assert.Equal("tenant-canonical", capture.Submission.Request.ReceiptCore.TenantId);
        Assert.Equal("driver-canonical", capture.Submission.Request.ReceiptCore.DriverId);
        Assert.Equal(RideAuditPolicy.Version, capture.Submission.Request.ReceiptCore.PolicyVersion);
        Assert.StartsWith(FixturePlayIntegrityClient.TokenPrefix, capture.Submission.Request.Attestation.Token, StringComparison.Ordinal);
        Assert.NotEqual(capture.SealedComposite.Receipt.AttestationTokenHash, capture.Submission.Request.Attestation.Token);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-035")]
    [Trait("AC", "AC-RIDE-035-001")]
    public void Canonical_factory_fail_closes_without_tenant_or_raw_token()
    {
        Environment.SetEnvironmentVariable("RIDEAUDIT_TENANT_ID", null);
        Environment.SetEnvironmentVariable("RIDEAUDIT_DRIVER_ID", null);
        var factory = CanonicalAdmissionRequestFactory.FromEnvironment();
        var fixture = Fixtures.CaptureHappy();
        var missingIdentity = Assert.Throws<RideAuditFailClosedException>(() =>
            factory.Create(fixture.Capture.SealedComposite, new CaptureRequest
            {
                Driver = fixture.Capture.Pairing.Driver,
                Passenger = fixture.Capture.Pairing.Passenger,
                SessionId = "session-1",
                VehicleId = "vehicle-1",
                CollectorIdentity = "driver-1",
                DriverPlay = new FixturePlayIntegrityClient(),
                PassengerPlay = new FixturePlayIntegrityClient(),
                Allowlist = fixture.Allowlist,
                Clock = fixture.Clock,
                Probe = new ExplicitDeviceProbe(12, 80_000_000, 77, 36),
                Escrow = new EscrowPublicKey("escrow-1", fixture.Capture.SealedComposite.Receipt.PublicKeyPem),
                DriverStream = fixture.Capture.Composite.Sources[0],
                PassengerStream = fixture.Capture.Composite.Sources[1],
                Samples = fixture.Samples,
                SealRaw = false,
                RawConsent = false
            }, new AttestationEvidence(
                    PlayIntegrityProviders.Fixture,
                    fixture.Capture.SealedComposite.Receipt.AttestationTokenHash,
                    fixture.Capture.SealedComposite.Receipt.Nonce,
                    fixture.Clock.UtcNow,
                    ApprovedPackage.PackageName,
                    ApprovedPackage.CertDigest,
                    "MEETS_DEVICE_INTEGRITY",
                    true,
                    true,
                    FixturePlayIntegrityClient.Notice)));
        Assert.Equal(ErrorCodes.AuthRequired, missingIdentity.Code);

        var identified = new CanonicalAdmissionRequestFactory(new CanonicalAdmissionIdentity
        {
            TenantId = "tenant-canonical",
            DriverId = "driver-canonical",
            PolicyVersion = RideAuditPolicy.Version
        });
        var missingToken = Assert.Throws<RideAuditFailClosedException>(() =>
            identified.Create(fixture.Capture.SealedComposite, new CaptureRequest
            {
                Driver = fixture.Capture.Pairing.Driver,
                Passenger = fixture.Capture.Pairing.Passenger,
                SessionId = "session-1",
                VehicleId = "vehicle-1",
                CollectorIdentity = "driver-1",
                DriverPlay = new FixturePlayIntegrityClient(),
                PassengerPlay = new FixturePlayIntegrityClient(),
                Allowlist = fixture.Allowlist,
                Clock = fixture.Clock,
                Probe = new ExplicitDeviceProbe(12, 80_000_000, 77, 36),
                Escrow = new EscrowPublicKey("escrow-1", fixture.Capture.SealedComposite.Receipt.PublicKeyPem),
                DriverStream = fixture.Capture.Composite.Sources[0],
                PassengerStream = fixture.Capture.Composite.Sources[1],
                Samples = fixture.Samples,
                SealRaw = false,
                RawConsent = false,
                TenantId = "tenant-canonical",
                DriverId = "driver-canonical",
                PolicyVersion = RideAuditPolicy.Version
            }, new AttestationEvidence(
                PlayIntegrityProviders.Fixture,
                fixture.Capture.SealedComposite.Receipt.AttestationTokenHash,
                fixture.Capture.SealedComposite.Receipt.Nonce,
                fixture.Clock.UtcNow,
                ApprovedPackage.PackageName,
                ApprovedPackage.CertDigest,
                "MEETS_DEVICE_INTEGRITY",
                true,
                true,
                FixturePlayIntegrityClient.Notice)));
        Assert.Equal(ErrorCodes.AttestationFailed, missingToken.Code);
    }

    [AvaloniaFact]
    [Trait("FR", "FR-RIDE-056")]
    [Trait("AC", "AC-RIDE-056-001")]
    [Trait("AC", "AC-RIDE-053-001")]
    public void Production_graph_records_unavailable_seams_and_does_not_default_to_success()
    {
        var graph = ProductionCaptureGraph.Wire(
            new UnavailableDiscoveryBus(),
            new UnavailableCameraSource(),
            new UnavailablePlayIntegrityClient(),
            CanonicalAdmissionRequestFactory.FromEnvironment(),
            null,
            null,
            "radio missing",
            "camera missing");
        Assert.False(graph.ProductionReady);
        Assert.Contains(graph.UnavailableSeams, item => item.Contains("UnavailableDiscovery", StringComparison.Ordinal));
        Assert.Contains(graph.UnavailableSeams, item => item.Contains("UnavailableCamera", StringComparison.Ordinal));
        Assert.Contains(graph.UnavailableSeams, item => item.Contains("UnavailablePlayIntegrity", StringComparison.Ordinal));
        Assert.Contains(graph.UnavailableSeams, item => item.Contains("AdmissionUnavailable", StringComparison.Ordinal));
        var runtime = new CaptureRuntime { Graph = graph };
        var view = runtime.CreateShell();
        Assert.Contains("PRODUCTION_UNAVAILABLE", view.FindControl<TextBlock>("FailClosedText")!.Text);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-056")]
    public void Android_application_installs_production_composition()
    {
        var application = File.ReadAllText(Path.Combine(Repo.Root(), "src/RideAudit.Client.Android/Application.cs"));
        var composition = File.ReadAllText(Path.Combine(Repo.Root(), "src/RideAudit.Client.Android/AndroidProductionComposition.cs"));
        Assert.Contains("AndroidProductionComposition.Install", application);
        Assert.Contains("ProductionCaptureGraph.FromEnvironment", composition);
        Assert.Contains("UnavailableDiscoveryBus", composition);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-043")]
    [Trait("AC", "AC-RIDE-043-001")]
    [Trait("AC", "AC-RIDE-221-001")]
    public void Source_payload_container_binds_hashes_to_embedded_media()
    {
        var composite = Fixtures.CaptureHappy().Capture.Composite;
        Assert.Equal(CompositeSourceContainer.CodecId, composite.Codec);
        Assert.True(CompositeSourceContainer.TryParse(composite.CanonicalBytes, out var parsed));
        var hashes = composite.Sources.Select(source => Hashes.Sha256Hex(source.Payload)).ToList();
        var ids = composite.Sources.Select(source => source.StreamId).ToList();
        var bind = CompositeSourceContainer.VerifyBindings(composite.CanonicalBytes, ids, hashes);
        Assert.True(bind.Ok);
        Assert.Equal(composite.Sources[0].Payload, parsed.DriverPayload);
        Assert.Equal(composite.Sources[1].Payload, parsed.PassengerPayload);
        Assert.Contains("not-h264", System.Text.Encoding.UTF8.GetString(composite.CanonicalBytes));
    }

    [Fact]
    public void Compose_and_cutover_enforce_recorded_digest_and_health_probe()
    {
        var root = Repo.Root();
        var compose = File.ReadAllText(Path.Combine(root, "deploy/omarchy/compose.yaml"));
        var cutover = File.ReadAllText(Path.Combine(root, "deploy/omarchy/Confirm-Cutover.ps1"));
        var digest = File.ReadAllText(Path.Combine(root, "deploy/omarchy/RECORDED-IMAGE-DIGEST")).Trim();
        var receipt = File.ReadAllText(Path.Combine(root, "docs/receipts/distribution/legion2-omarchy-20260928.md"));
        Assert.DoesNotContain("\n    build:", compose.Replace("\r\n", "\n"));
        Assert.Contains("@sha256:031a4a6e21cc0424a6276a59b9d38cabe15f7c5670468d3a99e4dcb8aad9fee5", compose);
        Assert.Equal("sha256:031a4a6e21cc0424a6276a59b9d38cabe15f7c5670468d3a99e4dcb8aad9fee5", digest);
        Assert.Contains("up -d --no-build --pull never", cutover);
        Assert.Contains("CUTOVER_HTTP", cutover);
        Assert.Contains("http://127.0.0.1:18080/", cutover);
        Assert.Contains("grep -q $recordedDigest", cutover);
        Assert.Contains("cutover checkout is `2612693`", receipt);
        Assert.DoesNotContain("Checkout on Omarchy: `f51f454`", receipt);
    }

    private static SourceStream Stream(string id, string device, IReadOnlyList<TimeSpan> frames, string payload) =>
        new(
            id,
            device,
            "attest-ref",
            new CameraMetadata(id + "-cam", "rear", 1280, 720),
            frames,
            System.Text.Encoding.UTF8.GetBytes(payload));
}
