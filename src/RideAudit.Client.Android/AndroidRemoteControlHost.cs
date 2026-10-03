// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Net;
using System.Security.Cryptography;
using Android.Content;
using Android.Util;
using Avalonia.Controls;
using Avalonia.RemoteControl.Server;
using Avalonia.RemoteControl.Server.Bridge;
using Microsoft.Extensions.DependencyInjection;
using RideAudit.Shared.Ui;

namespace RideAudit.Client.Android;

/// <summary>
/// Debug-only loopback bridge for SharpNinja.Avalonia.RemoteControl.Runtime.
/// The listener requests port 0. The marker records the port the OS assigned.
/// The token is random per process and is written only to the package-private marker.
/// </summary>
public sealed class AndroidRemoteControlHost : IDisposable
{
    private const string LogTag = "RideAuditRemote";

    private readonly ServiceProvider serviceProvider;
    private readonly RemoteControlBridgeTcpListener listener;
    private readonly string markerPath;

    private AndroidRemoteControlHost(
        ServiceProvider serviceProvider,
        RemoteControlBridgeTcpListener listener,
        string markerPath)
    {
        this.serviceProvider = serviceProvider;
        this.listener = listener;
        this.markerPath = markerPath;
    }

    public static Task<AndroidRemoteControlHost> StartAsync(
        Context context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.Run(() => StartCoreAsync(context, cancellationToken), cancellationToken);
    }

    public void Dispose()
    {
        // The TCP listener is IAsyncDisposable only. Sync ServiceProvider.Dispose throws.
        try
        {
            listener.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        finally
        {
            try
            {
                DeleteMarker(markerPath);
            }
            finally
            {
                serviceProvider.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }
    }

    private static async Task<AndroidRemoteControlHost> StartCoreAsync(
        Context context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var markerDirectory = context.FilesDir?.AbsolutePath
            ?? throw new InvalidOperationException("Android package files directory is unavailable.");
        var markerPath = Path.Combine(markerDirectory, RemoteControlBridgeEndpointMarker.FileName);
        DeleteMarker(markerPath);

        var services = new ServiceCollection();
        services.AddAvaloniaRemoteControlRuntime(options =>
        {
            options.IsEnabled = true;
            options.Host = IPAddress.Loopback;
            options.Port = 0;
            options.RequireAuthentication = true;
            options.AuthenticationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            options.IsAdbTunnel = true;
            options.AllowRemoteActions = true;
            options.AllowRemoteFrames = true;
            options.AllowRemoteInput = true;
            options.AllowedMutableProperties.Add("CheckBox.IsChecked");
        });
        services.AddSingleton<IRemoteControlRootProvider>(new ShellRootProvider());
        services.AddLogging();

        var provider = services.BuildServiceProvider();
        try
        {
            var listener = provider.GetRequiredService<RemoteControlBridgeTcpListener>();
            await listener.StartAsync(cancellationToken).ConfigureAwait(false);
            var marker = listener.CreateEndpointMarker();
            await marker.WriteAsync(markerDirectory, cancellationToken).ConfigureAwait(false);
            Log.Info(LogTag, "Avalonia.RemoteControl bound loopback port " + marker.DevicePort + ".");
            return new AndroidRemoteControlHost(provider, listener, markerPath);
        }
        catch (Exception startFailure)
        {
            Exception? cleanupFailure = null;
            try
            {
                DeleteMarker(markerPath);
            }
            catch (Exception ex)
            {
                cleanupFailure = ex;
            }

            try
            {
                await provider.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                cleanupFailure = cleanupFailure is null ? ex : new AggregateException(cleanupFailure, ex);
            }

            if (cleanupFailure is not null)
            {
                throw new AggregateException(startFailure, cleanupFailure);
            }

            throw;
        }
    }

    private static void DeleteMarker(string markerPath)
    {
        if (File.Exists(markerPath))
        {
            File.Delete(markerPath);
        }
    }

    private sealed class ShellRootProvider : IRemoteControlRootProvider
    {
        public Control? GetRootControl() => App.ShellRoot;
    }
}
