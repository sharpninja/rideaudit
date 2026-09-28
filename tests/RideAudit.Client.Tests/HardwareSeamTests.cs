// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Bt;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.Contracts;
using RideAudit.Video;
using Xunit;

namespace RideAudit.Client.Tests;

public class HardwareSeamTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-053")]
    public void Unavailable_radio_fail_closes_and_does_not_invent_peers()
    {
        var bus = new UnavailableDiscoveryBus();
        Assert.False(bus.RadioAvailable);
        Assert.Equal("unavailable", bus.TransportKind);
        var advertise = Assert.Throws<RideAuditFailClosedException>(() => bus.Advertise(new Advertisement
        {
            Address = "aa:bb:cc:dd:ee:01",
            DisplayName = "driver",
            Service = ApiBoundary.RideAuditBluetooth
        }));
        Assert.Equal(ErrorCodes.BluetoothDisabled, advertise.Code);
        var scan = Assert.Throws<RideAuditFailClosedException>(() => bus.Scan("aa:bb:cc:dd:ee:01"));
        Assert.Equal(ErrorCodes.BluetoothDisabled, scan.Code);

        var driver = new PhoneNode("aa:bb:cc:dd:ee:01", "driver", PhoneRole.Driver);
        var passenger = new PhoneNode("aa:bb:cc:dd:ee:02", "passenger", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, "intent-1");
        passenger.Confirm(PhoneRole.Passenger, "intent-1");
        var pairing = new BluetoothPairingService(bus).Pair(driver, passenger);
        Assert.False(pairing.Ok);
        Assert.Equal(ErrorCodes.BluetoothDisabled, pairing.Code);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-041")]
    public void Unavailable_camera_fail_closes_and_does_not_return_fixture_frames()
    {
        var source = new UnavailableCameraSource();
        Assert.False(source.CameraAvailable);
        var ex = Assert.Throws<RideAuditFailClosedException>(() =>
            CaptureMedia.Require(source, new CameraCaptureRequest("s", "d", "a", "rear")));
        Assert.Equal(ErrorCodes.CameraUnavailable, ex.Code);
    }

    [Fact]
    public void In_memory_bus_is_labeled_and_not_a_radio()
    {
        var bus = new InMemoryDiscoveryBus();
        Assert.Equal("in-memory", bus.TransportKind);
        Assert.True(bus.RadioAvailable);
    }

    [Fact]
    public void Capture_admission_options_fail_close_without_env()
    {
        var options = new CaptureAdmissionOptions();
        var ex = Assert.Throws<RideAuditFailClosedException>(() => options.EnsureReady());
        Assert.Equal(ErrorCodes.AdmissionUnavailable, ex.Code);
    }
}
