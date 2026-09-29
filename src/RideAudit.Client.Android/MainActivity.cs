// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Avalonia.Android;

namespace RideAudit.Client.Android;

[Activity(
    Label = "RideAudit",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
#if DEBUG
    private const string LogTag = "RideAuditRemote";
    private readonly object bridgeSync = new();
    private AndroidRemoteControlHost? bridgeHost;
    private CancellationTokenSource? bridgeStart;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        bridgeStart = new CancellationTokenSource();
        _ = StartBridgeAsync(bridgeStart.Token);
    }

    protected override void OnDestroy()
    {
        bridgeStart?.Cancel();
        AndroidRemoteControlHost? host;
        lock (bridgeSync)
        {
            host = bridgeHost;
            bridgeHost = null;
        }

        host?.Dispose();
        bridgeStart?.Dispose();
        bridgeStart = null;
        base.OnDestroy();
    }

    private async Task StartBridgeAsync(CancellationToken cancellationToken)
    {
        try
        {
            Log.Info(LogTag, "Starting Avalonia.RemoteControl loopback bridge.");
            var host = await AndroidRemoteControlHost.StartAsync(this, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                host.Dispose();
                return;
            }

            lock (bridgeSync)
            {
                bridgeHost?.Dispose();
                bridgeHost = host;
            }

            Log.Info(LogTag, "Avalonia.RemoteControl loopback bridge is listening.");
        }
        catch (System.OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Log.Info(LogTag, "Avalonia.RemoteControl bridge startup canceled.");
        }
        catch (Exception exception)
        {
            Log.Error(LogTag, exception.ToString());
        }
    }
#endif
}
