// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Security.Cryptography;
using RideAudit.Bt;
using RideAudit.Capture;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.PlayIntegrity;
using RideAudit.Seal;
using RideAudit.Video;
using RideAudit.Viewer;

namespace RideAudit.Client.Tests.Support;

public static class Repo
{
    public static string Root()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "global.json")) &&
                Directory.Exists(Path.Combine(dir.FullName, "src")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}

public sealed class SealedFixture
{
    public required CaptureResult Capture { get; init; }
    public required RSA EscrowPrivate { get; init; }
    public required FixedClock Clock { get; init; }
    public required PackageAllowlist Allowlist { get; init; }
    public required MemoryBitcoinHeaders Headers { get; init; }
    public required RideBundle Bundle { get; init; }
    public required IReadOnlyList<TelematicsSample> Samples { get; init; }
}

public static class Fixtures
{
    public const string Marker = "PLAINTEXT_MARKER_9f3a";

    public static SealedFixture CaptureHappy(IPlayIntegrityClient? play = null, bool sealRaw = false, bool rawConsent = false, byte[]? raw = null)
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 9, 28, 12, 0, 0, TimeSpan.Zero));
        var allowlist = PackageAllowlist.CreateDevelopmentDefault();
        var (publicKey, privateKey) = EscrowKeyFactory.CreateEphemeral("escrow-1");
        var playClient = play ?? new FixturePlayIntegrityClient();
        var frames = new[]
        {
            TimeSpan.FromMilliseconds(0),
            TimeSpan.FromMilliseconds(33),
            TimeSpan.FromMilliseconds(66),
        };
        var samples = new List<TelematicsSample>
        {
            new(TimeSpan.Zero, 0.1, 0.2, 0.9, 1.0, null, null, null),
            new(TimeSpan.FromMilliseconds(66), 0.4, -0.2, 0.8, 2.0, null, null, null),
        };
        var driver = new PhoneNode("aa:bb:cc:dd:ee:01", "driver-phone", PhoneRole.Driver);
        var passenger = new PhoneNode("aa:bb:cc:dd:ee:02", "passenger-phone", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, "intent-1");
        passenger.Confirm(PhoneRole.Passenger, "intent-1");
        var request = new CaptureRequest
        {
            Driver = driver,
            Passenger = passenger,
            SessionId = "session-1",
            VehicleId = "vehicle-1",
            CollectorIdentity = "driver-1",
            DriverPlay = playClient,
            PassengerPlay = playClient,
            Allowlist = allowlist,
            Clock = clock,
            Probe = new ExplicitDeviceProbe(12, 80_000_000, 77, 36),
            Escrow = publicKey,
            DriverStream = Stream("driver-stream", "device-driver", frames, Marker + "-driver"),
            PassengerStream = Stream("passenger-stream", "device-passenger", frames, Marker + "-passenger"),
            Samples = samples,
            SealRaw = sealRaw,
            RawConsent = rawConsent,
            RawPayload = raw,
            SourceCommitNotice = "test-commit",
        };
        var session = new DualPhoneCaptureSession(new InMemoryDiscoveryBus(), new InterimInProcessAdmissionClient(), clock);
        var capture = session.Run(request);
        var headers = new MemoryBitcoinHeaders();
        var proof = OtsProofBuilder.Build(capture.SealedComposite.Receipt, "0000block", "sibling", headers);
        var bundle = new RideBundle
        {
            BundleId = "bundle-1",
            CaseId = "case-1",
            VehicleId = "vehicle-1",
            Records =
            [
                new ReviewRecord
                {
                    Sealed = capture.SealedComposite,
                    Ots = proof,
                    Admitted = true,
                    GpsPresent = false,
                    Obd2Present = false,
                    Telematics = samples.Select(sample => new TelematicsCue(sample.SessionTime, "spider", sample.AccelX.ToString())).ToList(),
                },
            ],
        };
        return new SealedFixture
        {
            Capture = capture,
            EscrowPrivate = privateKey,
            Clock = clock,
            Allowlist = allowlist,
            Headers = headers,
            Bundle = bundle,
            Samples = samples,
        };
    }

    public static CourtViewer ViewerFor(SealedFixture fixture, bool allowSimulated = false)
    {
        var gate = new VerificationGate(fixture.Allowlist, fixture.Headers, allowSimulated);
        var escrow = new QuorumEscrow(fixture.EscrowPrivate, fixture.Clock);
        return new CourtViewer(gate, escrow, fixture.Clock);
    }

    public static CourtReleaseAuthorization Release(FixedClock clock, string caseId = "case-1", int attestations = 2) =>
        new()
        {
            CaseId = caseId,
            Authorizer = "court-clerk",
            M = 2,
            N = 3,
            ExpiresAt = clock.UtcNow.AddHours(1),
            WorkingCopyRef = "wc-1",
            Attestations = Enumerable.Range(0, attestations)
                .Select(index => new CustodianAttestation { CustodianId = "c" + index, Statement = "release" })
                .ToList(),
        };

    private static SourceStream Stream(string id, string device, IReadOnlyList<TimeSpan> frames, string payload) =>
        new(
            id,
            device,
            "attest-ref",
            new CameraMetadata(id + "-cam", "rear", 1280, 720),
            frames,
            System.Text.Encoding.UTF8.GetBytes(payload));
}
