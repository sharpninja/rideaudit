// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text.Json;

namespace RideAudit.Client.Android.AiUnit.Tests;

public static class DeviceVisualRunner
{
    private static readonly object Gate = new();
    private static readonly object LogGate = new();

    public static void AssertWireframe(string id, string markdownPath)
    {
        var asset = VisualAssetCatalog.Wireframes().Single(item => item.MarkdownPath == markdownPath);
        if (asset.FrameSvgPaths.Count == 0)
        {
            throw new Xunit.Sdk.XunitException(id + " has no SVG baseline link in " + markdownPath);
        }

        AssertFrames(asset.Id, asset.Kind, asset.FrameSvgPaths.Take(1).ToList(), invokePerceptual: id == "WF-01", storyboardSequence: false);
    }

    public static void AssertStoryboard(string id, string markdownPath)
    {
        var asset = VisualAssetCatalog.Storyboards().Single(item => item.MarkdownPath == markdownPath);
        if (asset.FrameSvgPaths.Count == 0)
        {
            throw new Xunit.Sdk.XunitException(id + " has no storyboard frame SVG. A static missing baseline is a fail, not a skip.");
        }

        AssertFrames(asset.Id, asset.Kind, asset.FrameSvgPaths, invokePerceptual: false, storyboardSequence: true);
    }

    private static void AssertFrames(string assetId, string kind, IReadOnlyList<string> frames, bool invokePerceptual, bool storyboardSequence)
    {
        CodexVisualGate.RequireCodexSubscriptionProfile();
        ResetLogOnce();
        List<string> failures;
        lock (Gate)
        {
            AdbDeviceSession session;
            try
            {
                session = AdbDeviceSession.Open();
            }
            catch (Exception ex)
            {
                throw new Xunit.Sdk.XunitException("Device visual test failed closed: " + ex.Message);
            }

            failures = new List<string>();
            var index = 0;
            RemoteBridgeSession? remote = null;
            var storyboardScreen = "WF-01";
            var perceptualBudget = storyboardSequence;
            if (storyboardSequence)
            {
                session.RestartApp();
                Thread.Sleep(1500);
                remote = RemoteBridgeSession.Connect(session);
            }

            try
            {
            foreach (var svg in frames)
            {
                index++;
                var frameId = frames.Count == 1 ? assetId : assetId + "-f" + index.ToString("00");
                var screenId = ScreenIdFromSvg(svg);
                var reviewMiss = screenId.StartsWith("WF-R-", StringComparison.Ordinal);
                try
                {
                    if (storyboardSequence)
                    {
                        if (!reviewMiss)
                        {
                            AdvanceStoryboard(remote!, ref storyboardScreen, screenId);
                        }
                    }
                    else
                    {
                        session.Navigate(screenId);
                    }

                    var xml = session.DumpUi();
                    var actualScreen = session.ScreenId(xml);
                    var png = session.Screencap();
                    var directory = Path.Combine(VisualAssetCatalog.RepoRoot(), "artifacts", "aiunit-device", assetId);
                    var pixel = BaselineComparer.Compare(png, svg, directory, frameId);
                    IReadOnlyList<string> usability = storyboardSequence
                        ? remote!.UsabilityDefects()
                        : Array.Empty<string>();
                    PerceptualResult? perceptual = null;
                    var callPerceptual = !reviewMiss && (invokePerceptual || pixel.WithinThreshold || (perceptualBudget && index == 1));
                    if (callPerceptual)
                    {
                        perceptual = CodexVisualGate.Compare(screenId, pixel.ActualPath, pixel.BaselinePath);
                    }

                    var screenOk = string.Equals(actualScreen, screenId, StringComparison.Ordinal);
                    AppendLog(new
                    {
                        assetId,
                        kind,
                        frameId,
                        screenId,
                        actualScreen,
                        session.Serial,
                        session.Model,
                        pixel.DifferingPixelRatio,
                        pixel.Width,
                        pixel.Height,
                        pixel.ActualPath,
                        pixel.BaselinePath,
                        pixel.DiffPath,
                        pixel.WithinThreshold,
                        perceptual = perceptual?.Status,
                        perceptualDetail = perceptual?.Detail,
                        usability,
                        threshold = VisualThreshold.MaxDifferingPixelRatio,
                        channelDelta = VisualThreshold.ChannelDelta,
                    });

                    if (reviewMiss)
                    {
                        failures.Add(frameId + " review screen " + screenId + " is not hosted on the Android capture client. RemoteControl left the capture shell in place.");
                    }

                    if (!screenOk)
                    {
                        failures.Add(frameId + " screen was '" + (actualScreen ?? "(missing)") + "', expected " + screenId + ".");
                    }

                    if (!pixel.WithinThreshold)
                    {
                        failures.Add(frameId + " differing pixel ratio " + pixel.DifferingPixelRatio.ToString("0.0000")
                            + " exceeds " + VisualThreshold.MaxDifferingPixelRatio.ToString("0.00")
                            + ". Diff: " + pixel.DiffPath);
                    }

                    if (usability.Count > 0)
                    {
                        failures.Add(frameId + " usability fail-closed: " + string.Join("; ", usability));
                    }

                    if (perceptual is not null && perceptual.Status != "pass")
                    {
                        failures.Add(frameId + " perceptual gate " + perceptual.Status + ": " + perceptual.Detail);
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(frameId + " failed closed: " + ex.GetType().Name + ": " + RedactException(ex));
                    AppendLog(new
                    {
                        assetId,
                        kind,
                        frameId,
                        error = RedactException(ex),
                    });
                }
            }
            }
            finally
            {
                remote?.Dispose();
            }
        }

        if (failures.Count > 0)
        {
            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, failures));
        }
    }

    private static void AdvanceStoryboard(RemoteBridgeSession remote, ref string current, string target)
    {
        if (string.Equals(current, target, StringComparison.Ordinal))
        {
            return;
        }

        if (target == "WF-01")
        {
            remote.Click("ReturnButton");
            current = "WF-01";
            return;
        }

        if (current != "WF-01" && !CanStep(current, target))
        {
            remote.Click("ReturnButton");
            Thread.Sleep(400);
            current = "WF-01";
        }

        foreach (var name in ClicksFrom(current, target))
        {
            remote.Click(name);
            Thread.Sleep(500);
        }

        current = target;
    }

    private static bool CanStep(string current, string target)
    {
        var order = new[] { "WF-01", "WF-02", "WF-03", "WF-04", "WF-06", "WF-07" };
        var from = Array.IndexOf(order, current);
        var to = Array.IndexOf(order, target);
        if (current == "WF-04" && target == "WF-08")
        {
            return true;
        }

        return from >= 0 && to > from;
    }

    private static IEnumerable<string> ClicksFrom(string current, string target)
    {
        if (target == "WF-05")
        {
            yield return "PassengerButton";
            yield return "ContinueButton";
            yield return "PeerButton";
            yield return "ConfirmCheck";
            yield return "ConfirmPairButton";
            yield break;
        }

        if (target == "WF-08")
        {
            foreach (var name in ClicksFrom(current, "WF-04"))
            {
                yield return name;
            }

            yield return "StartButton";
            yield break;
        }

        var steps = new[]
        {
            ("WF-01", "WF-02", new[] { "DriverButton", "ContinueButton" }),
            ("WF-02", "WF-03", new[] { "PeerButton" }),
            ("WF-03", "WF-04", new[] { "ConfirmCheck", "ConfirmPairButton" }),
            ("WF-04", "WF-06", new[] { "StartButton", "StopButton" }),
            ("WF-06", "WF-07", new[] { "SubmitButton" }),
        };

        var seen = current == "WF-01";
        foreach (var step in steps)
        {
            if (step.Item1 == current)
            {
                seen = true;
            }

            if (!seen)
            {
                continue;
            }

            foreach (var name in step.Item3)
            {
                yield return name;
            }

            if (step.Item2 == target)
            {
                yield break;
            }
        }
    }

    private static string RedactException(Exception ex)
    {
        var text = ex.GetType().Name + ": " + ex.Message;
        return text.Length > 500 ? text.Substring(0, 500) : text;
    }

    private static string ScreenIdFromSvg(string svgPath)
    {
        var name = Path.GetFileNameWithoutExtension(svgPath);
        var split = name.Split('-', 3);
        if (name.StartsWith("WF-R-", StringComparison.OrdinalIgnoreCase))
        {
            return "WF-R-" + split[2].Split('-')[0];
        }

        if (name.StartsWith("WF-", StringComparison.OrdinalIgnoreCase) && split.Length >= 2)
        {
            return "WF-" + split[1];
        }

        return name;
    }

    private static int logReset;

    private static void ResetLogOnce()
    {
        if (Interlocked.Exchange(ref logReset, 1) != 0)
        {
            return;
        }

        var path = Path.Combine(VisualAssetCatalog.RepoRoot(), "artifacts", "aiunit-device", "results.jsonl");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void AppendLog(object row)
    {
        var path = Path.Combine(VisualAssetCatalog.RepoRoot(), "artifacts", "aiunit-device", "results.jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var line = JsonSerializer.Serialize(row);
        lock (LogGate)
        {
            File.AppendAllText(path, line + Environment.NewLine);
        }
    }
}
