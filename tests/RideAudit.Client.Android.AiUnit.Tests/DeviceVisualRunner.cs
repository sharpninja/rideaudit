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

        AssertFrames(asset.Id, asset.Kind, asset.FrameSvgPaths.Take(1).ToList(), invokePerceptual: id == "WF-01");
    }

    public static void AssertStoryboard(string id, string markdownPath)
    {
        var steps = VisualAssetCatalog.ParseSteps(markdownPath);
        if (steps.Count == 0)
        {
            throw new Xunit.Sdk.XunitException(id + " has no storyboard beats. A missing step sequence is a fail, not a skip.");
        }

        AssertStoryboardSequence(id, steps);
    }

    private static void AssertStoryboardSequence(string assetId, IReadOnlyList<StoryboardStep> steps)
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

            session.RestartApp();
            Thread.Sleep(1500);
            RemoteBridgeSession remote;
            try
            {
                remote = RemoteBridgeSession.Connect(session);
            }
            catch (Exception ex)
            {
                throw new Xunit.Sdk.XunitException(
                    assetId + " RemoteControl did not attach. Storyboard coverage fails closed: " + RedactException(ex));
            }

            failures = new List<string>();
            var current = "WF-01";
            var role = StoryboardSequence.Role(steps);
            var index = 0;
            try
            {
                foreach (var step in steps)
                {
                    index++;
                    var frameId = assetId + "-b" + step.Beat.ToString("00") + "-f" + index.ToString("00");
                    var clicks = StoryboardSequence.Clicks(current, step, role);
                    if (clicks is null)
                    {
                        var reason = step.ScreenId.StartsWith("WF-R-", StringComparison.Ordinal)
                            ? "review screen is not hosted on the Android capture client"
                            : step.AlternateBranch
                                ? "alternate branch from " + current + " to " + step.ScreenId + " is not taken on this session"
                                : "cannot be driven from " + current + " to " + step.ScreenId + " without leaving the session";
                        failures.Add(frameId + " " + reason + ".");
                        AppendLog(new
                        {
                            assetId,
                            kind = "storyboard",
                            frameId,
                            beat = step.Beat,
                            title = step.Title,
                            screenId = step.ScreenId,
                            current,
                            driven = false,
                            clicks = Array.Empty<string>(),
                            alternate = step.AlternateBranch,
                            reason,
                            session.Serial,
                            session.Model,
                        });
                        continue;
                    }

                    try
                    {
                        foreach (var name in clicks)
                        {
                            remote.Click(name);
                            Thread.Sleep(500);
                        }

                        var xml = session.DumpUi();
                        var actualScreen = session.ScreenId(xml);
                        if (!string.IsNullOrEmpty(actualScreen))
                        {
                            current = actualScreen;
                        }

                        var png = session.Screencap();
                        var directory = Path.Combine(VisualAssetCatalog.RepoRoot(), "artifacts", "aiunit-device", assetId);
                        var pixel = BaselineComparer.Compare(png, step.SvgPath, directory, frameId);
                        var usability = remote.UsabilityDefects();
                        PerceptualResult? perceptual = null;
                        if (pixel.WithinThreshold)
                        {
                            perceptual = CodexVisualGate.Compare(step.ScreenId, pixel.ActualPath, pixel.BaselinePath);
                        }

                        var screenOk = string.Equals(actualScreen, step.ScreenId, StringComparison.Ordinal);
                        AppendLog(new
                        {
                            assetId,
                            kind = "storyboard",
                            frameId,
                            beat = step.Beat,
                            title = step.Title,
                            screenId = step.ScreenId,
                            actualScreen,
                            driven = true,
                            clicks,
                            alternate = step.AlternateBranch,
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

                        if (!screenOk)
                        {
                            failures.Add(frameId + " screen was '" + (actualScreen ?? "(missing)") + "', expected " + step.ScreenId + ".");
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
                        failures.Add(frameId + " failed closed: " + RedactException(ex));
                        AppendLog(new
                        {
                            assetId,
                            kind = "storyboard",
                            frameId,
                            beat = step.Beat,
                            title = step.Title,
                            screenId = step.ScreenId,
                            current,
                            driven = false,
                            clicks,
                            error = RedactException(ex),
                        });
                        try
                        {
                            var recovered = session.ScreenId(session.DumpUi());
                            if (!string.IsNullOrEmpty(recovered))
                            {
                                current = recovered;
                            }
                        }
                        catch (Exception recoverEx)
                        {
                            failures.Add(frameId + " screen recovery failed closed: " + RedactException(recoverEx));
                        }
                    }
                }
            }
            finally
            {
                remote.Dispose();
            }
        }

        if (failures.Count > 0)
        {
            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, failures));
        }
    }

    private static void AssertFrames(string assetId, string kind, IReadOnlyList<string> frames, bool invokePerceptual)
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
            foreach (var svg in frames)
            {
                index++;
                var frameId = frames.Count == 1 ? assetId : assetId + "-f" + index.ToString("00");
                var screenId = VisualAssetCatalog.ScreenIdFromPath(svg);
                try
                {
                    session.Navigate(screenId);

                    var xml = session.DumpUi();
                    var actualScreen = session.ScreenId(xml);
                    var png = session.Screencap();
                    var directory = Path.Combine(VisualAssetCatalog.RepoRoot(), "artifacts", "aiunit-device", assetId);
                    var pixel = BaselineComparer.Compare(png, svg, directory, frameId);
                    PerceptualResult? perceptual = null;
                    if (invokePerceptual || pixel.WithinThreshold)
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
                        threshold = VisualThreshold.MaxDifferingPixelRatio,
                        channelDelta = VisualThreshold.ChannelDelta,
                    });

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

                    if (perceptual is not null && perceptual.Status != "pass")
                    {
                        failures.Add(frameId + " perceptual gate " + perceptual.Status + ": " + perceptual.Detail);
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(frameId + " failed closed: " + RedactException(ex));
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

        if (failures.Count > 0)
        {
            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, failures));
        }
    }

    private static string RedactException(Exception ex)
    {
        var text = ex.GetType().Name + ": " + ex.Message;
        return text.Length > 500 ? text.Substring(0, 500) : text;
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
