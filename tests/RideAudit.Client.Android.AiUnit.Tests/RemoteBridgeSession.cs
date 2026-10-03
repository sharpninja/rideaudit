// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Globalization;
using System.Net.Sockets;
using System.Text.Json;
using Avalonia.RemoteControl.Protocol;
using Avalonia.RemoteControl.Protocol.V1;
using Google.Protobuf;

namespace RideAudit.Client.Android.AiUnit.Tests;

public readonly record struct MarkerEndpoint(int DevicePort, string Token);

public sealed class RemoteBridgeSession : IDisposable
{
    // Protocol 0.8.0 defaults to 8 MiB. The harness still passes that cap so a smaller package default cannot truncate the wireframe tree.
    private const int MaxAcceptedFrameLength = 8 * 1024 * 1024;

    // Connect and frame reads have no socket timeout in the protocol package.
    // A wireless forward that never answers must fail closed instead of hanging the run.
    private const int ConnectTimeoutMs = 8000;
    private const int ReadTimeoutMs = 20000;

    private readonly AdbDeviceSession session;
    private readonly string token;
    private readonly int hostPort;
    private bool forwardRemoved;

    private RemoteBridgeSession(AdbDeviceSession session, string token, int hostPort)
    {
        this.session = session;
        this.token = token;
        this.hostPort = hostPort;
    }

    public static RemoteBridgeSession Connect(AdbDeviceSession session)
    {
        var marker = session.ShellPublic("shell run-as " + AdbDeviceSession.PackageName + " cat files/avalonia-remote-control.json");
        if (marker.ExitCode != 0)
        {
            throw new IOException("RemoteControl marker was not readable. Storyboard driving fails closed.");
        }

        var endpoint = ParseMarker(marker.Text);
        var forwardedHostPort = ForwardHostPort(session, endpoint.DevicePort);
        Exception? last = null;
        try
        {
            for (var attempt = 1; attempt <= 8; attempt++)
            {
                RemoteBridgeSession? bridge = null;
                try
                {
                    bridge = new RemoteBridgeSession(session, endpoint.Token, forwardedHostPort);
                    bridge.Send(BridgeMethod.GetCapabilities, new GetCapabilitiesRequest(), GetCapabilitiesResponse.Parser);
                    return bridge;
                }
                catch (Exception ex) when (IsAttachRetry(ex) && attempt < 8)
                {
                    last = ex;
                    Thread.Sleep(750);
                }
                catch (Exception ex)
                {
                    var detail = bridge is null ? ex.Message : bridge.Redact(ex.Message);
                    throw new InvalidOperationException(
                        "RemoteControl bridge did not answer GetCapabilities. " + detail,
                        ex);
                }
            }

            var message = last is null
                ? "RemoteControl bridge did not answer GetCapabilities."
                : "RemoteControl bridge did not answer GetCapabilities. " + last.Message;
            throw new InvalidOperationException(message, last);
        }
        catch
        {
            RemoveForward(session, forwardedHostPort);
            throw;
        }
    }

    public static MarkerEndpoint ParseMarker(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty("devicePort", out var portElement)
                || !portElement.TryGetInt32(out var devicePort)
                || devicePort is < 1 or > 65535)
            {
                throw new IOException("RemoteControl marker has no bound device port.");
            }

            var token = root.TryGetProperty("token", out var tokenElement)
                ? tokenElement.GetString()
                : null;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new IOException("RemoteControl marker has no token.");
            }

            return new MarkerEndpoint(devicePort, token);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException)
        {
            throw new IOException("RemoteControl marker could not be read. The token is not included in this error.");
        }
    }

    public static int ParseForwardedHostPort(string adbOutput)
    {
        foreach (var raw in adbOutput.Split('\n'))
        {
            var line = raw.Trim();
            if (int.TryParse(line, NumberStyles.None, CultureInfo.InvariantCulture, out var port)
                && port is >= 1 and <= 65535)
            {
                return port;
            }
        }

        throw new IOException("adb forward did not return a host port.");
    }

    public void Click(string name)
    {
        var snapshot = Snapshot();
        var node = snapshot.Nodes.FirstOrDefault(item =>
            string.Equals(item.Name, name, StringComparison.Ordinal)
            || string.Equals(item.AutomationId, name, StringComparison.Ordinal));
        if (node is null)
        {
            throw new InvalidOperationException("RemoteControl tree has no node named " + name + ".");
        }

        CommandResult result;
        if (node.TypeName.Contains("CheckBox", StringComparison.Ordinal) || name == "ConfirmCheck")
        {
            result = Send(
                BridgeMethod.SetProperty,
                new SetPropertyRequest
                {
                    NodeId = node.Id,
                    PropertyName = "IsChecked",
                    Value = "true",
                },
                CommandResult.Parser);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("RemoteControl could not check " + name + ": " + Redact(result.Message));
            }

            return;
        }

        result = Send(BridgeMethod.InvokeClick, new InvokeClickRequest { NodeId = node.Id }, CommandResult.Parser);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("RemoteControl click " + name + " failed: " + Redact(result.Message));
        }
    }

    public TreeSnapshot CaptureTree() => Snapshot();

    public void Dispose()
    {
        if (forwardRemoved)
        {
            return;
        }

        forwardRemoved = true;
        RemoveForward(session, hostPort);
    }

    private static int ForwardHostPort(AdbDeviceSession session, int devicePort)
    {
        var forward = session.ShellPublic(
            "forward --no-rebind tcp:0 tcp:" + devicePort.ToString(CultureInfo.InvariantCulture));
        if (forward.ExitCode != 0)
        {
            throw new IOException("adb forward failed. Storyboard driving fails closed.");
        }

        return ParseForwardedHostPort(forward.Text);
    }

    private static void RemoveForward(AdbDeviceSession session, int hostPort)
    {
        session.ShellPublic("forward --remove tcp:" + hostPort.ToString(CultureInfo.InvariantCulture));
    }

    private static bool IsAttachRetry(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is InvalidDataException)
            {
                return false;
            }

            if (current is EndOfStreamException or SocketException or IOException or TimeoutException)
            {
                return true;
            }
        }

        return false;
    }

    private TreeSnapshot Snapshot() =>
        Send(BridgeMethod.GetSnapshot, new GetSnapshotRequest(), TreeSnapshot.Parser);

    private T Send<T>(BridgeMethod method, IMessage payload, MessageParser<T> parser)
        where T : IMessage<T>
    {
        var requestId = "req-" + Guid.NewGuid().ToString("N");
        var request = new BridgeRequest
        {
            ProtocolVersion = RemoteControlProtocol.DisplayVersion,
            RequestId = requestId,
            Method = method,
            Authorization = RemoteControlBridgeProtocol.CreateBearerAuthorization(token),
            Payload = payload.ToByteString(),
        };

        using var tcp = new TcpClient();
        var connect = tcp.ConnectAsync("127.0.0.1", hostPort);
        if (!connect.Wait(ConnectTimeoutMs))
        {
            tcp.Close();
            throw new TimeoutException("RemoteControl bridge connect exceeded " + ConnectTimeoutMs + " ms.");
        }

        connect.GetAwaiter().GetResult();
        using var stream = tcp.GetStream();
        var write = BridgeFrameCodec.WriteAsync(stream, request).AsTask();
        if (!write.Wait(ConnectTimeoutMs))
        {
            tcp.Close();
            throw new TimeoutException("RemoteControl bridge write exceeded " + ConnectTimeoutMs + " ms.");
        }

        write.GetAwaiter().GetResult();
        var read = BridgeFrameCodec.ReadAsync(
            stream,
            BridgeResponse.Parser,
            maxFrameLength: MaxAcceptedFrameLength).AsTask();
        if (!read.Wait(ReadTimeoutMs))
        {
            tcp.Close();
            throw new TimeoutException("RemoteControl bridge read exceeded " + ReadTimeoutMs + " ms.");
        }

        var response = read.GetAwaiter().GetResult();
        if (response.Status != BridgeStatus.Ok)
        {
            throw new InvalidOperationException("Bridge " + method + " status " + response.Status + ": " + Redact(response.ErrorMessage));
        }

        return parser.ParseFrom(response.Payload);
    }

    private string Redact(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text.Replace(token, "REDACTED", StringComparison.Ordinal);
    }
}
