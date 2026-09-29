// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text.RegularExpressions;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed record VisualAsset(string Kind, string Id, string MarkdownPath, IReadOnlyList<string> FrameSvgPaths);

public static class VisualAssetCatalog
{
    private static readonly Regex SvgLink = new(@"\((?<path>[^)\s]+\.svg)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex IdFromName = new(@"^(?<id>(?:WF|SB)(?:-R)?-\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "RideAudit.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("RideAudit.sln was not found above the test output directory.");
    }

    public static IReadOnlyList<VisualAsset> Wireframes() => Load("wireframe", "wireframes", "WF-");

    public static IReadOnlyList<VisualAsset> Storyboards() => Load("storyboard", "storyboards", "SB-");

    public static IEnumerable<object[]> WireframeCases()
    {
        foreach (var asset in Wireframes())
        {
            yield return new object[] { asset.Id, asset.MarkdownPath };
        }
    }

    public static IEnumerable<object[]> StoryboardCases()
    {
        foreach (var asset in Storyboards())
        {
            yield return new object[] { asset.Id, asset.MarkdownPath };
        }
    }

    private static IReadOnlyList<VisualAsset> Load(string kind, string folderName, string prefix)
    {
        var root = Path.Combine(RepoRoot(), "docs", "ux");
        var files = Directory.EnumerateFiles(root, prefix + "*.md", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || path.Contains($"/{folderName}/", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var assets = new List<VisualAsset>();
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var match = IdFromName.Match(name);
            if (!match.Success)
            {
                continue;
            }

            var text = File.ReadAllText(file);
            var frames = new List<string>();
            foreach (Match link in SvgLink.Matches(text))
            {
                var relative = link.Groups["path"].Value.Replace('/', Path.DirectorySeparatorChar);
                var full = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, relative));
                if (!frames.Contains(full, StringComparer.OrdinalIgnoreCase))
                {
                    frames.Add(full);
                }
            }

            assets.Add(new VisualAsset(kind, match.Groups["id"].Value, file, frames));
        }

        return assets;
    }
}
