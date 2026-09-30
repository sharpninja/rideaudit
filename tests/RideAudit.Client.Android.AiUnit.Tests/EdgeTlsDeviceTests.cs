// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Xunit;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed class EdgeTlsDeviceTests
{
    public const string EdgeSerial = "adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp";

    [Fact]
    public void Device_edge_records_admission_tls_line()
    {
        var previous = Environment.GetEnvironmentVariable("RIDEAUDIT_ADB_SERIAL");
        Environment.SetEnvironmentVariable("RIDEAUDIT_ADB_SERIAL", EdgeSerial);
        try
        {
            var session = AdbDeviceSession.Open();
            Assert.Equal(EdgeSerial, session.Serial);
            session.RestartApp();
            Thread.Sleep(12000);
            using var remote = RemoteBridgeSession.Connect(session);
            remote.Click("AboutButton");
            Thread.Sleep(800);
            var lines = remote.CaptureTree().Nodes
                .Select(node => node.Properties.FirstOrDefault(property => property.Name == "Text")?.Value ?? "")
                .Where(text => text.Contains("EDGE_TLS", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var directory = Path.Combine(VisualAssetCatalog.RepoRoot(), "artifacts", "aiunit-device");
            Directory.CreateDirectory(directory);
            var report = lines.Count == 0
                ? "EDGE_TLS text was absent after About opened."
                : string.Join(Environment.NewLine, lines);
            File.WriteAllText(Path.Combine(directory, "edge-tls.txt"), session.Model + Environment.NewLine + report);
            Assert.True(lines.Count > 0, session.Model + " " + report);
        }
        finally
        {
            Environment.SetEnvironmentVariable("RIDEAUDIT_ADB_SERIAL", previous);
        }
    }
}
