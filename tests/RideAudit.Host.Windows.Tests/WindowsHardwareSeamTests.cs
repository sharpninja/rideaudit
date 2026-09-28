using RideAudit.Bt;
using RideAudit.Client.Core;
using RideAudit.Contracts;
using RideAudit.Video;

namespace RideAudit.Host.Windows.Tests;

public class WindowsHardwareSeamTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-053")]
    public void Windows_ble_does_not_invent_peers()
    {
        using var bus = WindowsBleDiscoveryBus.Create();
        Assert.Equal("windows-ble", bus.TransportKind);
        if (!bus.RadioAvailable)
        {
            var ex = Assert.Throws<RideAuditFailClosedException>(() => bus.Advertise(new Advertisement
            {
                Address = "local",
                DisplayName = "probe",
                Service = ApiBoundary.RideAuditBluetooth
            }));
            Assert.Equal(ErrorCodes.BluetoothDisabled, ex.Code);
            return;
        }

        bus.Advertise(new Advertisement
        {
            Address = "local",
            DisplayName = "probe",
            Service = ApiBoundary.RideAuditBluetooth
        });
        var seen = bus.Scan("FFFFFFFFFFFF");
        Assert.DoesNotContain(seen, ad => ad.Address is "00:00:00:00:00:00" or "000000000000");
    }

    [Fact]
    [Trait("FR", "FR-RIDE-041")]
    public void Windows_camera_does_not_return_fixture_bytes_when_unavailable()
    {
        var camera = WindowsCameraSource.Create();
        Assert.Equal("windows-media-capture", camera.SourceKind);
        if (!camera.CameraAvailable)
        {
            var ex = Assert.Throws<RideAuditFailClosedException>(() =>
                camera.Capture(new CameraCaptureRequest("s", "d", "a", "rear")));
            Assert.Equal(ErrorCodes.CameraUnavailable, ex.Code);
            return;
        }

        try
        {
            var stream = camera.Capture(new CameraCaptureRequest("s", "d", "a", "rear"));
            Assert.True(stream.Payload.Length > 0);
            Assert.False(System.Text.Encoding.UTF8.GetString(stream.Payload).Contains("PLAINTEXT_MARKER", StringComparison.Ordinal));
        }
        catch (RideAuditFailClosedException ex)
        {
            Assert.Equal(ErrorCodes.CameraUnavailable, ex.Code);
        }
    }
}
