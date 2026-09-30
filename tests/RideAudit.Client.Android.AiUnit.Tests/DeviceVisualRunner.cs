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

        AssertFrames(asset.Id, asset.Kind, asset.FrameSvgPaths.Take(1).ToList());
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
        var advisories = new List<string>();
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
                        var reason = StoryboardSequence.UndrivenReason(current, step);
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
                            usability = UsabilityInspector.NotDriven(),
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
                        var usability = TreeChecks(remote, png, directory, frameId, step.ScreenId, actualScreen, step.SvgPath);
                        var frontier = FrontierCheck(step.ScreenId, pixel.ActualPath, pixel.BaselinePath);
                        usability.Add(frontier);

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
                            perceptual = frontier.Status,
                            perceptualDetail = frontier.Detail,
                            usability,
                            threshold = VisualThreshold.MaxDifferingPixelRatio,
                            channelDelta = VisualThreshold.ChannelDelta,
                        });

                        if (!screenOk)
                        {
                            failures.Add(frameId + " screen was '" + (actualScreen ?? "(missing)") + "', expected " + step.ScreenId + ".");
                        }

                        advisories.Add(FidelityVerdict.AdvisoryPixel(frameId, pixel.DifferingPixelRatio, pixel.DiffPath));
                        failures.AddRange(FidelityVerdict.Failures(frameId, usability));
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
            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, failures.Concat(advisories)));
        }
    }

    private static void AssertFrames(string assetId, string kind, IReadOnlyList<string> frames)
    {
        CodexVisualGate.RequireCodexSubscriptionProfile();
        ResetLogOnce();
        List<string> failures;
        var advisories = new List<string>();
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
                    session.RestartApp();
                    Thread.Sleep(1500);
                    RemoteBridgeSession? remote = null;
                    try
                    {
                        try
                        {
                            remote = RemoteBridgeSession.Connect(session);
                        }
                        catch (Exception attachEx)
                        {
                            failures.Add(frameId + " RemoteControl did not attach for usability: " + RedactException(attachEx));
                        }

                        if (remote is not null)
                        {
                            session.Drive(remote, screenId);
                        }

                        Thread.Sleep(1500);

                        var xml = session.DumpUi();
                        var actualScreen = session.ScreenId(xml);
                        var png = session.Screencap();
                        var directory = Path.Combine(VisualAssetCatalog.RepoRoot(), "artifacts", "aiunit-device", assetId);
                        var pixel = BaselineComparer.Compare(png, svg, directory, frameId);
                        var usability = remote is null
                            ? UsabilityInspector.RemoteUnavailable().ToList()
                            : TreeChecks(remote, png, directory, frameId, screenId, actualScreen, svg);
                        var frontier = FrontierCheck(screenId, pixel.ActualPath, pixel.BaselinePath);
                        usability.Add(frontier);

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
                            perceptual = frontier.Status,
                            perceptualDetail = frontier.Detail,
                            usability,
                            threshold = VisualThreshold.MaxDifferingPixelRatio,
                            channelDelta = VisualThreshold.ChannelDelta,
                        });

                        if (!screenOk)
                        {
                            failures.Add(frameId + " screen was '" + (actualScreen ?? "(missing)") + "', expected " + screenId + ".");
                        }

                        advisories.Add(FidelityVerdict.AdvisoryPixel(frameId, pixel.DifferingPixelRatio, pixel.DiffPath));
                        failures.AddRange(FidelityVerdict.Failures(frameId, usability));
                    }
                    finally
                    {
                        remote?.Dispose();
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
            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, failures.Concat(advisories)));
        }
    }

    private static List<UsabilityCheck> TreeChecks(
        RemoteBridgeSession remote,
        byte[] png,
        string directory,
        string frameId,
        string expectedScreen,
        string? actualScreen,
        string svgPath)
    {
        var tree = remote.CaptureTree().Nodes.ToList();
        var checks = UsabilityInspector.InspectTree(
                tree,
                expectedScreen,
                actualScreen,
                UsabilityInspector.CountBaselineIconGroups(svgPath))
            .Where(check => check.Id != "low-contrast")
            .ToList();
        checks.Add(UsabilityInspector.InspectContrastPng(png, tree, directory, frameId));
        checks.AddRange(WireframeReflection.Judge(svgPath, tree));
        return checks;
    }

    private static UsabilityCheck FrontierCheck(string screenId, string actualPath, string baselinePath)
    {
        var perceptual = CodexVisualGate.Compare(screenId, actualPath, baselinePath);
        var status = perceptual.Status == "pass" ? "pass" : "fail-closed";
        return new UsabilityCheck("aiunit-frontier", status, perceptual.Detail, "codex-subscription image attachments");
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
