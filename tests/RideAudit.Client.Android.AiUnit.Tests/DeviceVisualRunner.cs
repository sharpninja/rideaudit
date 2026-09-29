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
        var asset = VisualAssetCatalog.Storyboards().Single(item => item.MarkdownPath == markdownPath);
        if (asset.FrameSvgPaths.Count == 0)
        {
            throw new Xunit.Sdk.XunitException(id + " has no storyboard frame SVG. A static missing baseline is a fail, not a skip.");
        }

        AssertFrames(asset.Id, asset.Kind, asset.FrameSvgPaths, invokePerceptual: false);
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
                var screenId = ScreenIdFromSvg(svg);
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
                    failures.Add(frameId + " failed closed: " + ex.GetType().Name + ": " + ex.Message);
                    AppendLog(new
                    {
                        assetId,
                        kind,
                        frameId,
                        error = ex.ToString(),
                    });
                }
            }
        }

        if (failures.Count > 0)
        {
            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, failures));
        }
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
