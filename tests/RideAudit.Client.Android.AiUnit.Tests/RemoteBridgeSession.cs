// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Net.Sockets;
using System.Text.Json;
using Avalonia.RemoteControl.Protocol;
using Avalonia.RemoteControl.Protocol.V1;
using Google.Protobuf;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed class RemoteBridgeSession : IDisposable
{
    // Package 0.7.4 defaults to 1 MiB. The wireframe-aligned tree serializes above that.
    private const int MaxAcceptedFrameLength = 8 * 1024 * 1024;

    private readonly string token;

    private RemoteBridgeSession(string token)
    {
        this.token = token;
    }

    public static RemoteBridgeSession Connect(AdbDeviceSession session)
    {
        session.ShellPublic("forward tcp:47100 tcp:47100");
        Exception? last = null;
        for (var attempt = 1; attempt <= 8; attempt++)
        {
            RemoteBridgeSession? bridge = null;
            try
            {
                bridge = Open(session);
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

    private static RemoteBridgeSession Open(AdbDeviceSession session)
    {
        var marker = session.ShellPublic("shell run-as " + AdbDeviceSession.PackageName + " cat files/avalonia-remote-control.json");
        if (marker.ExitCode != 0)
        {
            throw new IOException("RemoteControl marker was not readable. Storyboard driving fails closed.");
        }

        try
        {
            using var json = JsonDocument.Parse(marker.Text);
            var token = json.RootElement.GetProperty("token").GetString()
                ?? throw new InvalidOperationException("RemoteControl marker has no token.");
            return new RemoteBridgeSession(token);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            throw new IOException("RemoteControl marker could not be read. The token is not included in this error.");
        }
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
        tcp.Connect("127.0.0.1", 47100);
        using var stream = tcp.GetStream();
        BridgeFrameCodec.WriteAsync(stream, request).GetAwaiter().GetResult();
        var response = BridgeFrameCodec.ReadAsync(
            stream,
            BridgeResponse.Parser,
            maxFrameLength: MaxAcceptedFrameLength).GetAwaiter().GetResult();
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
