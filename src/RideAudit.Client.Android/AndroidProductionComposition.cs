// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Android.Content;
using Android.Util;
using RideAudit.Bt;
using RideAudit.Capture;
using RideAudit.Client.Contracts;
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
    public const string TlsLogTag = "RideAuditTls";

    public static CaptureRuntime Install(Context context)
    {
        CaptureRuntime runtime;
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
            var missingPermissions = AndroidCaptureHardware.MissingRuntimePermissions(context);
            if (missingPermissions.Count > 0)
            {
                graph = graph.WithExtraUnavailable(
                    "UnavailablePermissions: " + string.Join(",", missingPermissions));
            }

            runtime = new CaptureRuntime { Graph = graph };
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
            runtime = new CaptureRuntime { Graph = graph };
        }

        App.CaptureRuntime = runtime;
        App.Mode = ShellMode.Capture;
        StartEdgeTlsProbe(context, runtime);
        return runtime;
    }

    private static void StartEdgeTlsProbe(Context context, CaptureRuntime runtime)
    {
        var address = Environment.GetEnvironmentVariable("RIDEAUDIT_ADMISSION_ADDRESS") ?? "";
        Log.Info(TlsLogTag, "probe start address=" + address + " counsel=not-configured");
        var pem = ReadAsset(context, "caddy-lab-root.pem");
        var intermediate = ReadAsset(context, "caddy-lab-intermediate.pem");
        _ = Task.Run(() =>
        {
            string line;
            try
            {
                var result = CaptureAdmissionChannel.ProbeEdgeTls(
                    CaptureAdmissionOptions.FromEnvironment(),
                    pem,
                    TimeSpan.FromSeconds(12),
                    intermediate);
                line = result.Display;
            }
            catch (Exception ex)
            {
                line = "EDGE_TLS FAIL " + ex.GetType().Name + ": " + ex.Message;
            }

            Log.Info(TlsLogTag, line);
            var handler = new global::Android.OS.Handler(global::Android.OS.Looper.MainLooper!);
            handler.Post(() => runtime.ReportEdgeTls(line));
        });
    }

    private static byte[] ReadAsset(Context context, string name)
    {
        try
        {
            using var stream = context.Assets?.Open(name);
            if (stream is null)
            {
                Log.Info(TlsLogTag, "lab asset missing: " + name);
                return [];
            }

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return buffer.ToArray();
        }
        catch (Exception ex)
        {
            Log.Info(TlsLogTag, "lab asset unreadable: " + name + " " + ex.GetType().Name);
            return [];
        }
    }
}
