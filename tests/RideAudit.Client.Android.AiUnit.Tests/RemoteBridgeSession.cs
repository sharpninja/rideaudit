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
    private readonly string token;

    private RemoteBridgeSession(string token)
    {
        this.token = token;
    }

    public static RemoteBridgeSession Connect(AdbDeviceSession session)
    {
        session.ShellPublic("forward tcp:47100 tcp:47100");
        var marker = session.ShellPublic("shell run-as " + AdbDeviceSession.PackageName + " cat files/avalonia-remote-control.json");
        if (marker.ExitCode != 0)
        {
            throw new InvalidOperationException("RemoteControl marker was not readable. Storyboard driving fails closed.");
        }

        string token;
        try
        {
            using var json = JsonDocument.Parse(marker.Text);
            token = json.RootElement.GetProperty("token").GetString()
                ?? throw new InvalidOperationException("RemoteControl marker has no token.");
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            throw new InvalidOperationException("RemoteControl marker could not be read. The token is not included in this error.");
        }

        var bridge = new RemoteBridgeSession(token);
        try
        {
            bridge.Send(BridgeMethod.GetCapabilities, new GetCapabilitiesRequest(), GetCapabilitiesResponse.Parser);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "RemoteControl bridge did not answer GetCapabilities. " + bridge.Redact(ex.Message),
                ex);
        }

        return bridge;
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

    public IReadOnlyList<string> UsabilityDefects()
    {
        var snapshot = Snapshot();
        var byId = snapshot.Nodes.ToDictionary(node => node.Id, node => node);
        var visible = snapshot.Nodes.Where(node =>
            EffectivelyVisible(node, byId)
            && node.AbsoluteBounds.Width > 1
            && node.AbsoluteBounds.Height > 1).ToList();
        var defects = new List<string>();
        var buttons = visible.Where(node => node.TypeName.Contains("Button", StringComparison.Ordinal)).ToList();
        for (var i = 0; i < buttons.Count; i++)
        {
            for (var j = i + 1; j < buttons.Count; j++)
            {
                if (Overlaps(buttons[i].AbsoluteBounds, buttons[j].AbsoluteBounds))
                {
                    defects.Add("overlapping buttons " + buttons[i].Name + " and " + buttons[j].Name);
                }
            }
        }

        foreach (var node in visible.Where(item => item.TypeName.Contains("TextBlock", StringComparison.Ordinal)))
        {
            var text = node.Properties.FirstOrDefault(property => property.Name == "Text")?.Value ?? string.Empty;
            if (text.Length > 12 && node.AbsoluteBounds.Height < 10)
            {
                defects.Add("clipped text on " + node.Name);
            }
        }

        return defects;
    }

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
        var response = BridgeFrameCodec.ReadAsync(stream, BridgeResponse.Parser).GetAwaiter().GetResult();
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

    private static bool EffectivelyVisible(TreeNode node, IReadOnlyDictionary<string, TreeNode> byId)
    {
        var current = node;
        var guard = 0;
        while (current is not null && guard++ < 32)
        {
            if (!current.IsVisible)
            {
                return false;
            }

            if (string.IsNullOrEmpty(current.ParentId) || !byId.TryGetValue(current.ParentId, out current))
            {
                return true;
            }
        }

        return true;
    }

    private static bool Overlaps(Rect left, Rect right)
    {
        var x = Math.Min(left.X + left.Width, right.X + right.Width) - Math.Max(left.X, right.X);
        var y = Math.Min(left.Y + left.Height, right.Y + right.Height) - Math.Max(left.Y, right.Y);
        return x > 8 && y > 8;
    }
}
