// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Xunit;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed class RemoteBridgeMarkerTests
{
    [Fact]
    public void Marker_publishes_the_bound_device_port()
    {
        var endpoint = RemoteBridgeSession.ParseMarker(
            """
            {"schemaVersion":"1","devicePort":54321,"token":"session-token","bridgeProtocol":"arc-protobuf-v1"}
            """);

        Assert.Equal(54321, endpoint.DevicePort);
        Assert.Equal("session-token", endpoint.Token);
    }

    [Fact]
    public void Marker_without_a_bound_port_fails_closed_and_omits_the_token()
    {
        const string token = "SECRET-TOKEN-VALUE";
        var ex = Assert.Throws<IOException>(() => RemoteBridgeSession.ParseMarker(
            "{\"schemaVersion\":\"1\",\"token\":\"" + token + "\"}"));

        Assert.Contains("no bound device port", ex.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(token, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Marker_port_zero_is_not_a_bound_port()
    {
        var ex = Assert.Throws<IOException>(() => RemoteBridgeSession.ParseMarker(
            "{\"devicePort\":0,\"token\":\"session-token\"}"));

        Assert.Contains("no bound device port", ex.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("session-token", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Forward_output_is_the_host_port_adb_assigned()
    {
        Assert.Equal(43123, RemoteBridgeSession.ParseForwardedHostPort("43123\r\n"));
    }

    [Fact]
    public void Forward_output_without_a_port_fails_closed()
    {
        var ex = Assert.Throws<IOException>(() => RemoteBridgeSession.ParseForwardedHostPort("error: cannot bind\n"));
        Assert.Contains("did not return a host port", ex.Message, StringComparison.Ordinal);
    }
}
