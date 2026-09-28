// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Net;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using RideAudit.Client.Contracts;
using RideAudit.Server.Admission;

namespace RideAudit.TestSupport;

public sealed class AdmissionTestTransport : IDisposable
{
    public AdmissionTestTransport(GrpcChannel channel, GrpcSealedAdmissionClient admission)
    {
        Channel = channel;
        Admission = admission;
    }

    public GrpcChannel Channel { get; }

    public GrpcSealedAdmissionClient Admission { get; }

    public void Dispose() => Channel.Dispose();
}

/// <summary>
/// In-process Kestrel host for authenticated AdmissionGrpcService tests.
/// Same pattern as CanonicalGrpcAdmissionTests / GrpcSealedAdmissionClient.
/// </summary>
public static class AdmissionTestHost
{
    public static AdmissionTestTransport Client(WebApplication host, string bearer)
    {
        var addresses = host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
        var address = addresses!.Addresses.Single(value => value.StartsWith("http://", StringComparison.Ordinal));
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var channel = GrpcChannel.ForAddress(address);
        var headers = new Metadata { { "authorization", "Bearer " + bearer } };
        var admission = new GrpcSealedAdmissionClient(new RideAudit.Protos.Admission.V1.Admission.AdmissionClient(channel), headers);
        return new AdmissionTestTransport(channel, admission);
    }

    public static async Task<WebApplication> Start(ServerWorld world)
    {
        var app = AdmissionHost.Build(world.Options, builder =>
        {
            builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, string.Empty);
            builder.WebHost.ConfigureKestrel(kestrel =>
            {
                kestrel.Listen(IPAddress.Loopback, 0, listen => listen.Protocols = HttpProtocols.Http2);
            });
        }, world.App);
        await app.StartAsync();
        return app;
    }
}
