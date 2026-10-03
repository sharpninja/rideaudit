// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text.RegularExpressions;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed record VisualAsset(string Kind, string Id, string MarkdownPath, IReadOnlyList<string> FrameSvgPaths);

public sealed record StoryboardStep(int Beat, string Title, string SvgPath, string ScreenId, bool AlternateBranch);

public static class VisualAssetCatalog
{
    private static readonly Regex SvgLink = new(@"\((?<path>[^)\s]+\.svg)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex IdFromName = new(@"^(?<id>(?:WF|SB)(?:-R)?-\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex BeatHeading = new(@"^(?<n>\d+)\.\s+\*\*(?<title>[^*]+)\*\*", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex AlternateMark = new(@"(?i)fail-closed branch|(?m)^\s*Reject:", RegexOptions.Compiled);

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

    public static IReadOnlyList<StoryboardStep> ParseSteps(string markdownPath)
    {
        var text = File.ReadAllText(markdownPath);
        var beatsAt = text.IndexOf("## Beats", StringComparison.Ordinal);
        if (beatsAt < 0)
        {
            return Array.Empty<StoryboardStep>();
        }

        var body = text.Substring(beatsAt + "## Beats".Length);
        var nextHeading = Regex.Match(body, @"^##\s+", RegexOptions.Multiline);
        if (nextHeading.Success)
        {
            body = body.Substring(0, nextHeading.Index);
        }

        var headings = BeatHeading.Matches(body);
        var steps = new List<StoryboardStep>();
        var directory = Path.GetDirectoryName(markdownPath)!;
        for (var i = 0; i < headings.Count; i++)
        {
            var heading = headings[i];
            var start = heading.Index;
            var end = i + 1 < headings.Count ? headings[i + 1].Index : body.Length;
            var beatText = body.Substring(start, end - start);
            var beat = int.Parse(heading.Groups["n"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var title = heading.Groups["title"].Value.Trim();
            string? previous = null;
            var cursor = 0;
            foreach (Match link in SvgLink.Matches(beatText))
            {
                var relative = link.Groups["path"].Value.Replace('/', Path.DirectorySeparatorChar);
                var full = Path.GetFullPath(Path.Combine(directory, relative));
                if (previous is not null && string.Equals(previous, full, StringComparison.OrdinalIgnoreCase))
                {
                    cursor = link.Index + link.Length;
                    continue;
                }

                var prelude = beatText.Substring(cursor, link.Index - cursor);
                steps.Add(new StoryboardStep(
                    beat,
                    title,
                    full,
                    ScreenIdFromPath(full),
                    AlternateMark.IsMatch(prelude)));
                previous = full;
                cursor = link.Index + link.Length;
            }
        }

        return steps;
    }

    public static string ScreenIdFromPath(string svgPath)
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
