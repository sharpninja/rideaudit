// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.RemoteControl.Protocol.V1;

namespace RideAudit.Client.Android.AiUnit.Tests;

public static class WireframeReflection
{
    private static readonly Regex TextElement = new(
        "<text\\b([^>]*)>(.*?)</text>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex Attr = new(
        "\\b(y|fill|font-size)\\s*=\\s*\"([^\"]*)\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ScreenIdText = new(
        "^(?:WF|SB)(?:-R)?-\\d+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static IReadOnlyList<UsabilityCheck> Unavailable(string status, string detail, string evidence)
    {
        return
        [
            new UsabilityCheck("controls", status, detail, evidence),
            new UsabilityCheck("layout", status, detail, evidence),
            new UsabilityCheck("style", status, detail, evidence),
        ];
    }

    public static IReadOnlyList<UsabilityCheck> Judge(string svgPath, IReadOnlyList<TreeNode> nodes)
    {
        var expected = ReadLabels(svgPath);
        if (expected.Count == 0)
        {
            return Unavailable("fail-closed", "The wireframe SVG has no content labels to compare.", svgPath);
        }

        var byId = nodes.ToDictionary(node => node.Id, node => node);
        var live = nodes
            .Where(node => node.TypeName.Contains("TextBlock", StringComparison.Ordinal) && EffectivelyVisible(node, byId))
            .Select(node => new LiveLabel(TextOf(node).Trim(), node))
            .Where(item => item.Text.Length > 0)
            .ToList();

        var missing = expected
            .Where(label => live.All(item => !string.Equals(item.Text, label.Text, StringComparison.Ordinal)))
            .Select(label => label.Text)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var expectedTexts = expected.Select(label => label.Text).ToHashSet(StringComparer.Ordinal);
        var extras = live
            .Select(item => item.Text)
            .Where(text => !expectedTexts.Contains(text) && !ScreenIdText.IsMatch(text))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var controlDetail = "missing " + missing.Count + ": " + Join(missing)
            + ". extra " + extras.Count + ": " + Join(extras)
            + ". wireframe labels " + expected.Select(label => label.Text).Distinct(StringComparer.Ordinal).Count();
        var controls = missing.Count == 0 && extras.Count == 0
            ? new UsabilityCheck("controls", "pass", controlDetail, svgPath)
            : new UsabilityCheck("controls", "fail", controlDetail, svgPath);

        var shared = new List<(SvgLabel Label, LiveLabel Live)>();
        foreach (var label in expected)
        {
            var match = live
                .Where(item => string.Equals(item.Text, label.Text, StringComparison.Ordinal))
                .OrderBy(item => item.Node.AbsoluteBounds.Y)
                .FirstOrDefault();
            if (match is not null)
            {
                shared.Add((label, match));
            }
        }

        UsabilityCheck layout;
        if (shared.Count < 2)
        {
            layout = new UsabilityCheck(
                "layout",
                "not-detectable",
                "Fewer than two shared labels, so vertical order was not compared.",
                "shared " + shared.Count);
        }
        else
        {
            var inversions = new List<string>();
            for (var i = 1; i < shared.Count; i++)
            {
                var above = shared[i - 1].Live.Node.AbsoluteBounds.Y;
                var below = shared[i].Live.Node.AbsoluteBounds.Y;
                if (below + 8 < above)
                {
                    inversions.Add(shared[i].Label.Text + " is above " + shared[i - 1].Label.Text);
                }
            }

            layout = inversions.Count == 0
                ? new UsabilityCheck("layout", "pass", "Vertical order matches for " + shared.Count + " shared labels.", "y order")
                : new UsabilityCheck("layout", "fail", Join(inversions), "y order");
        }

        var styleMisses = new List<string>();
        var styleCompared = 0;
        foreach (var pair in shared)
        {
            var fontRaw = Prop(pair.Live.Node, "FontSize");
            if (double.TryParse(fontRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var font) && font > 0)
            {
                styleCompared++;
                if (Math.Abs(font - pair.Label.FontSize) > 2)
                {
                    styleMisses.Add(pair.Label.Text + " font " + font.ToString("0.##", CultureInfo.InvariantCulture)
                        + " vs wireframe " + pair.Label.FontSize.ToString("0.##", CultureInfo.InvariantCulture));
                }
            }

            var foreground = Hex(Prop(pair.Live.Node, "Foreground"));
            var fill = Hex(pair.Label.Fill);
            if (foreground is not null && fill is not null)
            {
                styleCompared++;
                if (!string.Equals(foreground, fill, StringComparison.Ordinal))
                {
                    styleMisses.Add(pair.Label.Text + " color #" + foreground + " vs wireframe #" + fill);
                }
            }
        }

        UsabilityCheck style;
        if (styleCompared == 0)
        {
            style = new UsabilityCheck(
                "style",
                "not-detectable",
                "Live text did not expose a font size or hex color to compare with the wireframe.",
                "FontSize, Foreground");
        }
        else if (styleMisses.Count == 0)
        {
            style = new UsabilityCheck("style", "pass", styleCompared + " font or color samples match the wireframe.", "FontSize, Foreground");
        }
        else
        {
            style = new UsabilityCheck("style", "fail", Join(styleMisses), "FontSize, Foreground");
        }

        return [controls, layout, style];
    }

    private static List<SvgLabel> ReadLabels(string svgPath)
    {
        var text = File.ReadAllText(svgPath);
        var labels = new List<SvgLabel>();
        foreach (Match match in TextElement.Matches(text))
        {
            double y = 0;
            var fill = string.Empty;
            double fontSize = 0;
            foreach (Match attr in Attr.Matches(match.Groups[1].Value))
            {
                var name = attr.Groups[1].Value;
                var value = attr.Groups[2].Value;
                if (name.Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                }
                else if (name.Equals("fill", StringComparison.OrdinalIgnoreCase))
                {
                    fill = value;
                }
                else if (name.Equals("font-size", StringComparison.OrdinalIgnoreCase))
                {
                    double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fontSize);
                }
            }

            var content = Decode(match.Groups[2].Value).Trim();
            if (content.Length == 0 || y < 80 || fontSize <= 0)
            {
                continue;
            }

            if (content == "5G" || Regex.IsMatch(content, "^\\d{1,2}:\\d{2}$"))
            {
                continue;
            }

            labels.Add(new SvgLabel(content, y, fontSize, fill));
        }

        labels.Sort((left, right) => left.Y.CompareTo(right.Y));
        return labels;
    }

    private static string Decode(string value) =>
        value.Replace("&amp;", "&", StringComparison.Ordinal)
            .Replace("&lt;", "<", StringComparison.Ordinal)
            .Replace("&gt;", ">", StringComparison.Ordinal)
            .Replace("&quot;", "\"", StringComparison.Ordinal)
            .Replace("&#39;", "'", StringComparison.Ordinal);

    private static string? Hex(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = Regex.Match(value, "#([0-9A-Fa-f]{8}|[0-9A-Fa-f]{6})");
        if (!match.Success)
        {
            return null;
        }

        var hex = match.Groups[1].Value;
        if (hex.Length == 8)
        {
            hex = hex.Substring(2);
        }

        return hex.ToUpperInvariant();
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

    private static string TextOf(TreeNode node) => Prop(node, "Text");

    private static string Prop(TreeNode node, string name) =>
        node.Properties.FirstOrDefault(property => property.Name == name)?.Value ?? string.Empty;

    private static string Join(IReadOnlyList<string> items) =>
        items.Count == 0 ? "(none)" : string.Join("; ", items.Take(6));

    private sealed record SvgLabel(string Text, double Y, double FontSize, string Fill);

    private sealed record LiveLabel(string Text, TreeNode Node);
}
