// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed class AdbDeviceSession
{
    public const string PreferredSerial = "RFCW7078MVZ";
    public const string PackageName = "org.rideaudit.app";
    public const string Activity = "org.rideaudit.app/crc64ab21e1e23ed2ea51.MainActivity";

    private static readonly Regex Bounds = new(@"\[(?<x1>\d+),(?<y1>\d+)\]\[(?<x2>\d+),(?<y2>\d+)\]", RegexOptions.Compiled);

    private readonly string adb;

    private AdbDeviceSession(string adb, string serial, string model)
    {
        this.adb = adb;
        Serial = serial;
        Model = model;
    }

    public string Serial { get; }

    public string Model { get; }

    public static AdbDeviceSession Open()
    {
        var adb = ResolveAdb();
        var devices = Run(adb, "devices -l", TimeSpan.FromSeconds(20));
        if (devices.ExitCode != 0)
        {
            throw new InvalidOperationException("adb devices failed: " + devices.Text);
        }

        var rows = devices.Text.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Contains(" device ", StringComparison.Ordinal) || line.EndsWith(" device", StringComparison.Ordinal))
            .Where(line => !line.StartsWith("List of devices", StringComparison.Ordinal))
            .ToList();
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("No adb device is attached. Device visual tests fail closed.");
        }

        var preferred = rows.FirstOrDefault(line => line.StartsWith(PreferredSerial + " ", StringComparison.Ordinal)
            || line.StartsWith(PreferredSerial + "\t", StringComparison.Ordinal));
        var chosen = preferred ?? rows.FirstOrDefault(line => line.Contains("motorola_edge_2024", StringComparison.OrdinalIgnoreCase))
            ?? rows[0];
        var serial = chosen.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[0];
        var model = "unknown";
        var modelMatch = Regex.Match(chosen, @"model:(\S+)");
        if (modelMatch.Success)
        {
            model = modelMatch.Groups[1].Value;
        }

        return new AdbDeviceSession(adb, serial, model);
    }

    public void RestartApp()
    {
        Shell("am force-stop " + PackageName, TimeSpan.FromSeconds(15));
        var start = Shell("am start -n " + Activity, TimeSpan.FromSeconds(20));
        if (start.ExitCode != 0 || start.Text.Contains("Error", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("am start failed: " + start.Text);
        }
    }

    public string DumpUi()
    {
        var dump = Shell("uiautomator dump /dev/tty", TimeSpan.FromSeconds(25));
        var text = dump.Text;
        var start = text.IndexOf("<?xml", StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException("uiautomator did not return XML: " + text);
        }

        var end = text.LastIndexOf("</hierarchy>", StringComparison.Ordinal);
        if (end < 0)
        {
            throw new InvalidOperationException("uiautomator XML was truncated.");
        }

        return text.Substring(start, end + "</hierarchy>".Length - start);
    }

    public string? ScreenId(string xml)
    {
        var doc = XDocument.Parse(xml);
        var node = doc.Descendants()
            .FirstOrDefault(element => Attribute(element, "resource-id").Contains("ScreenId", StringComparison.Ordinal));
        return node is null ? null : Attribute(node, "text");
    }

    public bool TapResource(string resourceFragment, int maxScrolls = 6)
    {
        for (var attempt = 0; attempt <= maxScrolls; attempt++)
        {
            var xml = DumpUi();
            var doc = XDocument.Parse(xml);
            var node = doc.Descendants().FirstOrDefault(element =>
                Attribute(element, "resource-id").Contains(resourceFragment, StringComparison.Ordinal));
            if (node is not null && TryTapBounds(Attribute(node, "bounds")))
            {
                Thread.Sleep(700);
                return true;
            }

            var page = doc.Descendants().FirstOrDefault(element =>
                Attribute(element, "resource-id").Contains("PART_PageUpButton", StringComparison.Ordinal));
            if (page is null || !TryTapBounds(Attribute(page, "bounds")))
            {
                return false;
            }

            Thread.Sleep(400);
        }

        return false;
    }

    public byte[] Screencap()
    {
        var psi = new ProcessStartInfo(adb, "-s " + Serial + " exec-out screencap -p")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("adb screencap did not start.");
        using var memory = new MemoryStream();
        process.StandardOutput.BaseStream.CopyTo(memory);
        process.WaitForExit(20000);
        var bytes = memory.ToArray();
        if (process.ExitCode != 0 || bytes.Length < 8 || bytes[0] != 0x89)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException("screencap failed: " + error);
        }

        return bytes;
    }

    public void Navigate(string screenId)
    {
        RestartApp();
        Thread.Sleep(1200);
        switch (screenId)
        {
            case "WF-01":
                return;
            case "WF-04":
                TapResource("DriverButton", 0);
                return;
            case "WF-05":
                TapResource("PassengerButton", 0);
                return;
            case "WF-02":
                TapResource("DriverButton", 0);
                TapResource("DiscoverButton", 4);
                return;
            case "WF-08":
                TapResource("PassengerButton", 0);
                TapResource("StartButton", 6);
                return;
            case "WF-06":
                TapResource("DriverButton", 0);
                TapResource("StartButton", 6);
                TapResource("StopButton", 4);
                return;
            case "WF-07":
                TapResource("DriverButton", 0);
                TapResource("StopButton", 4);
                return;
            default:
                return;
        }
    }

    private bool TryTapBounds(string bounds)
    {
        var match = Bounds.Match(bounds);
        if (!match.Success)
        {
            return false;
        }

        var x1 = int.Parse(match.Groups["x1"].Value);
        var y1 = int.Parse(match.Groups["y1"].Value);
        var x2 = int.Parse(match.Groups["x2"].Value);
        var y2 = int.Parse(match.Groups["y2"].Value);
        var x = (x1 + x2) / 2;
        var y = (y1 + y2) / 2;
        Shell("input tap " + x + " " + y, TimeSpan.FromSeconds(15));
        return true;
    }

    private (int ExitCode, string Text) Shell(string command, TimeSpan timeout) =>
        Run(adb, "-s " + Serial + " shell " + command, timeout);

    private static string ResolveAdb()
    {
        var home = Environment.GetEnvironmentVariable("ANDROID_HOME")
            ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        if (!string.IsNullOrWhiteSpace(home))
        {
            var candidate = Path.Combine(home, "platform-tools", "adb.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var local = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Android",
            "Sdk",
            "platform-tools",
            "adb.exe");
        if (File.Exists(local))
        {
            return local;
        }

        throw new InvalidOperationException("adb.exe was not found. Set ANDROID_HOME.");
    }

    private static (int ExitCode, string Text) Run(string file, string args, TimeSpan timeout)
    {
        var psi = new ProcessStartInfo(file, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Process did not start: " + file);
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit((int)timeout.TotalMilliseconds))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            throw new TimeoutException(file + " timed out: " + args);
        }

        return (process.ExitCode, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
    }

    private static string Attribute(XElement element, string name) =>
        (string?)element.Attribute(name) ?? string.Empty;
}
