// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Android.Content;
using RideAudit.Bt;
using RideAudit.Capture;
using RideAudit.PlayIntegrity;
using RideAudit.Shared.Ui;
using RideAudit.Video;

namespace RideAudit.Client.Android;

/// <summary>
/// Production APK composition root. Wires BLE, camera, Play, canonical admission,
/// seal/escrow, and gRPC seams. Missing hardware or configuration is Unavailable*,
/// never a silent success.
/// </summary>
public static class AndroidProductionComposition
{
    public static CaptureRuntime Install(Context context)
    {
        try
        {
            var discovery = AndroidDiscoveryBus.Create(context);
            var camera = AndroidCameraSource.Create(context);
            IDiscoveryBus bus = discovery.RadioAvailable ? discovery : new UnavailableDiscoveryBus();
            ICameraSource frames = camera.CameraAvailable ? camera : new UnavailableCameraSource();
            var graph = ProductionCaptureGraph.FromEnvironment(
                bus,
                frames,
                discovery.ProbeDetail,
                camera.ProbeDetail);
            var runtime = new CaptureRuntime { Graph = graph };
            App.CaptureRuntime = runtime;
            App.Mode = ShellMode.Capture;
            return runtime;
        }
        catch (Exception ex)
        {
            var graph = ProductionCaptureGraph.Wire(
                new UnavailableDiscoveryBus(),
                new UnavailableCameraSource(),
                new UnavailablePlayIntegrityClient(),
                CanonicalAdmissionRequestFactory.FromEnvironment(),
                null,
                null,
                ex.GetType().Name + ": " + ex.Message,
                "camera probe did not complete");
            var runtime = new CaptureRuntime { Graph = graph };
            App.CaptureRuntime = runtime;
            App.Mode = ShellMode.Capture;
            return runtime;
        }
    }
}
