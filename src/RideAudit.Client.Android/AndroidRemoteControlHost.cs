// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Net;
using System.Security.Cryptography;
using Android.Content;
using Avalonia.Controls;
using Avalonia.RemoteControl.Server;
using Avalonia.RemoteControl.Server.Bridge;
using Microsoft.Extensions.DependencyInjection;
using RideAudit.Shared.Ui;

namespace RideAudit.Client.Android;

/// <summary>
/// Debug-only loopback bridge for SharpNinja.Avalonia.RemoteControl.
/// The token is random per process and is written only to the package-private marker.
/// </summary>
public sealed class AndroidRemoteControlHost : IDisposable
{
    public const int DevicePort = 47100;

    private readonly ServiceProvider serviceProvider;
    private readonly RemoteControlBridgeTcpListener listener;

    private AndroidRemoteControlHost(
        ServiceProvider serviceProvider,
        RemoteControlBridgeTcpListener listener)
    {
        this.serviceProvider = serviceProvider;
        this.listener = listener;
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
        listener.DisposeAsync().AsTask().GetAwaiter().GetResult();
        serviceProvider.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private static async Task<AndroidRemoteControlHost> StartCoreAsync(
        Context context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var services = new ServiceCollection();
        services.AddAvaloniaRemoteControlRuntime(options =>
        {
            options.IsEnabled = true;
            options.Host = IPAddress.Loopback;
            options.Port = DevicePort;
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
            var markerDirectory = context.FilesDir?.AbsolutePath
                ?? throw new InvalidOperationException("Android package files directory is unavailable.");
            await listener.CreateEndpointMarker()
                .WriteAsync(markerDirectory, cancellationToken)
                .ConfigureAwait(false);
            return new AndroidRemoteControlHost(provider, listener);
        }
        catch (Exception startFailure)
        {
            Exception? disposeFailure = null;
            try
            {
                await provider.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                disposeFailure = ex;
            }

            if (disposeFailure is not null)
            {
                throw new AggregateException(startFailure, disposeFailure);
            }

            throw;
        }
    }

    private sealed class ShellRootProvider : IRemoteControlRootProvider
    {
        public Control? GetRootControl() => App.ShellRoot;
    }
}
