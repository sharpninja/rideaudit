// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Bt;
using RideAudit.Client.Core;
using RideAudit.Client.Tests.Support;
using Xunit;

namespace RideAudit.Client.Tests;

[Trait("Partition", "TEST-RIDE-025")]
public class TestRide025SyncTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-041")]
    [Trait("AC", "AC-RIDE-041-001")]
    [Trait("AC", "AC-TEST-025-001")]
    [Trait("AC", "AC-TEST-034-001")]
    [Trait("AC", "AC-UC-017-001")]
    public void Two_phones_collect_one_session()
    {
        var fixture = Fixtures.CaptureHappy();
        Assert.Equal(PhoneRole.Driver, fixture.Capture.Pairing.Driver.IntendedRole);
        Assert.Equal(PhoneRole.Passenger, fixture.Capture.Pairing.Passenger.IntendedRole);
        Assert.Equal(2, fixture.Capture.Composite.Sources.Count);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-041")]
    [Trait("AC", "AC-RIDE-041-002")]
    public void Each_stream_keeps_device_camera_and_attestation()
    {
        var sources = Fixtures.CaptureHappy().Capture.Composite.Sources;
        Assert.All(sources, source =>
        {
            Assert.False(string.IsNullOrWhiteSpace(source.DeviceId));
            Assert.False(string.IsNullOrWhiteSpace(source.AttestationReference));
            Assert.False(string.IsNullOrWhiteSpace(source.Camera.CameraId));
            Assert.NotEmpty(source.FrameTimestamps);
        });
    }

    [Fact]
    [Trait("FR", "FR-RIDE-042")]
    [Trait("AC", "AC-RIDE-042-001")]
    public void Sync_clock_offset_drift_and_uncertainty_are_recorded()
    {
        var sync = Fixtures.CaptureHappy().Capture.Sync;
        Assert.Equal(TimeSpan.FromMilliseconds(1), sync.Drift);
        Assert.Equal(TimeSpan.FromMilliseconds(5), sync.Uncertainty);
        Assert.True(sync.Offset.Duration() < TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-042")]
    [Trait("AC", "AC-RIDE-042-002")]
    public void Dropped_intervals_are_exposed()
    {
        var clock = new FixedClock(DateTimeOffset.Parse("2026-09-28T12:00:00Z"));
        var joiner = new VideoSyncJoiner();
        var master = new SessionClock(clock.UtcNow, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(2));
        var frames = new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(33), TimeSpan.FromMilliseconds(400) };
        var sync = joiner.Join(master, clock.UtcNow, TimeSpan.FromMilliseconds(33), frames);
        Assert.NotEmpty(sync.UnsyncedIntervals);
        Assert.Equal("dropped-or-unsynced", sync.UnsyncedIntervals[0].Reason);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-043")]
    [Trait("AC", "AC-RIDE-043-001")]
    public void Composite_is_produced_on_device()
    {
        var composite = Fixtures.CaptureHappy().Capture.Composite;
        Assert.True(composite.ProducedOnDevice);
        Assert.False(composite.ServerPlaintextComposite);
        Assert.StartsWith("RIDEAUDIT-COMPOSITE-v2", System.Text.Encoding.ASCII.GetString(composite.CanonicalBytes));
        Assert.True(RideAudit.Video.CompositeSourceContainer.TryParse(composite.CanonicalBytes, out _));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-043")]
    [Trait("AC", "AC-RIDE-043-002")]
    public void Public_submission_is_not_a_plaintext_composite()
    {
        var request = Fixtures.CaptureHappy().Capture.Submission.Request;
        var text = System.Text.Encoding.Latin1.GetString(request.SealedEnvelope.ToByteArray());
        Assert.DoesNotContain("RIDEAUDIT-COMPOSITE-v2", text);
        Assert.Equal(ApiBoundary.SealedContentType, request.ContentType);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-044")]
    [Trait("AC", "AC-RIDE-044-001")]
    public void Spider_graph_points_follow_video_timestamps()
    {
        var composite = Fixtures.CaptureHappy().Capture.Composite;
        Assert.Equal(composite.Sources[0].FrameTimestamps.Count, composite.Overlay.Points.Count);
        Assert.Equal(composite.Sources[0].FrameTimestamps[1], composite.Overlay.Points[1].SessionTime);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-044")]
    [Trait("AC", "AC-RIDE-044-002")]
    public void Overlay_manifest_is_versioned()
    {
        var overlay = Fixtures.CaptureHappy().Capture.Composite.Overlay;
        Assert.Equal("spider-graph-overlay-v1", overlay.Version);
        Assert.Equal("timeline-manifest-v1", overlay.TimelineManifestVersion);
    }
}

[Trait("Partition", "TEST-RIDE-034")]
public class TestRide034BluetoothTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-053")]
    [Trait("AC", "AC-RIDE-053-001")]
    public void Pairing_requires_both_roles_and_shared_intent()
    {
        var bus = new InMemoryDiscoveryBus();
        var service = new BluetoothPairingService(bus);
        var driver = new PhoneNode("11:11:11:11:11:11", "driver", PhoneRole.Driver);
        var passenger = new PhoneNode("22:22:22:22:22:22", "passenger", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, "intent");
        passenger.Confirm(PhoneRole.Passenger, "intent");
        bus.Advertise(new Advertisement { Address = passenger.Address, DisplayName = passenger.DisplayName, Service = ApiBoundary.RideAuditBluetooth });
        var result = service.Pair(driver, passenger);
        Assert.True(result.Ok);
        Assert.Equal(ApiBoundary.RideAuditBluetooth, result.Session!.Transport);
        Assert.Equal("not-used", ApiBoundary.LyftPrivateApiStatus);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-053")]
    [Trait("AC", "AC-RIDE-053-002")]
    [Trait("AC", "AC-TEST-025-002")]
    [Trait("AC", "AC-TEST-034-002")]
    [Trait("AC", "AC-UC-017-002")]
    public void Pairing_fails_closed_without_discovery_or_confirmation()
    {
        var service = new BluetoothPairingService(new InMemoryDiscoveryBus());
        var driver = new PhoneNode("11:11:11:11:11:11", "driver", PhoneRole.Driver);
        var passenger = new PhoneNode("22:22:22:22:22:22", "passenger", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, "intent");
        passenger.Confirm(PhoneRole.Passenger, "intent");
        var missing = service.Pair(driver, passenger);
        Assert.False(missing.Ok);
        Assert.Equal("BT_DISCOVERY_FAILED", missing.Code);

        var bus = new InMemoryDiscoveryBus();
        var unconfirmed = new PhoneNode("33:33:33:33:33:33", "other", PhoneRole.Passenger);
        bus.Advertise(new Advertisement { Address = unconfirmed.Address, DisplayName = unconfirmed.DisplayName, Service = ApiBoundary.RideAuditBluetooth });
        var role = new BluetoothPairingService(bus).Pair(driver, unconfirmed);
        Assert.False(role.Ok);
        Assert.Equal("BT_ROLE_UNCONFIRMED", role.Code);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-054")]
    [Trait("AC", "AC-RIDE-054-001")]
    public void Only_the_driver_starts_and_stops()
    {
        var coordinator = new SessionCoordinator();
        var ex = Assert.Throws<RideAuditFailClosedException>(() => coordinator.PassengerAttemptStart());
        Assert.Equal("FR-RIDE-054", ex.RequirementId);
        var stop = Assert.Throws<RideAuditFailClosedException>(() =>
            coordinator.Stop(PhoneRole.Passenger, "session", new FixedClock(DateTimeOffset.UnixEpoch)));
        Assert.Equal("FR-RIDE-054", stop.RequirementId);
        Assert.True(coordinator.IsClockMaster(PhoneRole.Driver));
        Assert.False(coordinator.IsClockMaster(PhoneRole.Passenger));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-054")]
    [Trait("AC", "AC-RIDE-054-002")]
    public void Driver_publishes_clock_commands()
    {
        var capture = Fixtures.CaptureHappy().Capture;
        Assert.Equal("intent-1", capture.Pairing.SessionIntentId);
        Assert.Contains(capture.Commands, command => command.Kind == CoordinationCommandKind.Clock && command.Clock is not null);
        Assert.Contains(capture.Commands, command => command.Kind == CoordinationCommandKind.Start);
        Assert.Contains(capture.Commands, command => command.Kind == CoordinationCommandKind.Stop);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-055")]
    [Trait("AC", "AC-RIDE-055-001")]
    public void Passenger_records_sync_clock_offset()
    {
        var sync = Fixtures.CaptureHappy().Capture.Sync;
        Assert.Equal(sync.Drift, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-055")]
    [Trait("AC", "AC-RIDE-055-002")]
    public void Passenger_composite_includes_overlay_before_seal()
    {
        var capture = Fixtures.CaptureHappy().Capture;
        Assert.NotEmpty(capture.Composite.Overlay.Points);
        Assert.Equal(capture.Composite.Overlay.Version, capture.SealedComposite.Receipt.CompositeMetadata!.OverlayManifestVersion);
    }
}
