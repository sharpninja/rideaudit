// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.RemoteControl.Protocol.V1;
using SkiaSharp;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed record UsabilityCheck(string Id, string Status, string Detail, string Evidence);

public static class UsabilityInspector
{
    public static readonly string[] CheckIds =
    [
        "clipped-text",
        "truncated-text",
        "text-overflow",
        "overlapping-controls",
        "empty-icon",
        "missing-icons",
        "low-contrast",
        "broken-layout",
        "about-cutoff",
    ];

    public static int CountBaselineIconGroups(string svgPath)
    {
        var text = File.ReadAllText(svgPath);
        return Regex.Matches(text, "<g transform=", RegexOptions.IgnoreCase).Count;
    }

    public static IReadOnlyList<UsabilityCheck> RemoteUnavailable()
    {
        return CheckIds.Select(id => new UsabilityCheck(
            id,
            "fail-closed",
            "AvaloniaRemote did not attach, so this check has no visual tree.",
            "no tree"))
            .Concat(WireframeReflection.Unavailable(
                "fail-closed",
                "AvaloniaRemote did not attach, so controls, layout, and style were not compared.",
                "no tree"))
            .ToList();
    }

    public static IReadOnlyList<UsabilityCheck> NotDriven()
    {
        return CheckIds.Select(id => new UsabilityCheck(
            id,
            "not-run",
            "The storyboard step was not driven, so no frame was captured.",
            "no navigation"))
            .Concat(WireframeReflection.Unavailable(
                "not-run",
                "The storyboard step was not driven, so controls, layout, and style were not compared.",
                "no navigation"))
            .ToList();
    }

    public static IReadOnlyList<UsabilityCheck> InspectTree(
        IReadOnlyList<TreeNode> nodes,
        string expectedScreenId,
        string? actualScreenId,
        int baselineIconGroups)
    {
        var byId = nodes.ToDictionary(node => node.Id, node => node);
        var visible = nodes.Where(node => EffectivelyVisible(node, byId)).ToList();
        return
        [
            ClippedText(visible),
            TruncatedText(visible),
            TextOverflow(visible, byId),
            OverlappingControls(visible, byId),
            EmptyIcon(visible, byId),
            MissingIcons(visible, baselineIconGroups),
            new UsabilityCheck("low-contrast", "not-detectable", "Contrast uses the screenshot crop after the tree is known.", "pending bitmap"),
            BrokenLayout(visible, expectedScreenId, actualScreenId),
            AboutCutoff(visible),
        ];
    }

    public static UsabilityCheck InspectContrastPng(
        byte[] png,
        IReadOnlyList<TreeNode> nodes,
        string artifactDirectory,
        string frameId)
    {
        using var bitmap = SKBitmap.Decode(png);
        if (bitmap is null)
        {
            return new UsabilityCheck("low-contrast", "fail-closed", "Screenshot is not a PNG.", frameId);
        }

        return InspectContrast(bitmap, nodes, artifactDirectory, frameId);
    }

    public static UsabilityCheck InspectContrast(
        SKBitmap bitmap,
        IReadOnlyList<TreeNode> nodes,
        string artifactDirectory,
        string frameId)
    {
        var byId = nodes.ToDictionary(node => node.Id, node => node);
        var texts = nodes.Where(node =>
            EffectivelyVisible(node, byId)
            && node.TypeName.Contains("TextBlock", StringComparison.Ordinal)
            && TextOf(node).Length > 0
            && node.AbsoluteBounds.Width > 4
            && node.AbsoluteBounds.Height > 4).ToList();
        if (texts.Count == 0 || bitmap.Width < 2 || bitmap.Height < 2)
        {
            return new UsabilityCheck("low-contrast", "not-detectable", "No visible text bounds to sample.", "no text node");
        }

        // Invisible / off-tree pages inflate extents and mis-map crops onto chrome.
        var visibleNodes = nodes.Where(node => EffectivelyVisible(node, byId)).ToList();
        var rootWidth = visibleNodes.Max(node => node.AbsoluteBounds.X + node.AbsoluteBounds.Width);
        var rootHeight = visibleNodes.Max(node => node.AbsoluteBounds.Y + node.AbsoluteBounds.Height);
        if (rootWidth < 8 || rootHeight < 8)
        {
            return new UsabilityCheck("low-contrast", "not-detectable", "Tree bounds are too small to map onto the screenshot.", "root " + rootWidth.ToString("0") + "x" + rootHeight.ToString("0"));
        }

        var scaleX = bitmap.Width / rootWidth;
        var scaleY = bitmap.Height / rootHeight;
        // A scrolled page is taller than the screenshot. Compressing that height
        // maps on-screen text onto the wrong pixels and clamps off-screen text
        // into the bitmap. Width still matches the window, so use it for both axes.
        if (scaleY > 0 && (scaleX / scaleY > 1.15 || scaleY / scaleX > 1.15))
        {
            scaleY = scaleX;
        }

        var measured = 0;
        string? failDetail = null;
        string? evidence = null;
        foreach (var node in texts)
        {
            var bounds = node.AbsoluteBounds;
            var mappedY = bounds.Y * scaleY;
            var mappedX = bounds.X * scaleX;
            if (mappedY >= bitmap.Height || mappedX >= bitmap.Width)
            {
                continue;
            }

            var x = (int)Math.Clamp(mappedX, 0, bitmap.Width - 1);
            var y = (int)Math.Clamp(mappedY, 0, bitmap.Height - 1);
            var width = (int)Math.Clamp(bounds.Width * scaleX, 1, bitmap.Width - x);
            var height = (int)Math.Clamp(bounds.Height * scaleY, 1, bitmap.Height - y);
            if (width < 4 || height < 4)
            {
                continue;
            }

            var samples = new List<double>(width * height);
            for (var py = y; py < y + height; py++)
            {
                for (var px = x; px < x + width; px++)
                {
                    samples.Add(Luminance(bitmap.GetPixel(px, py)));
                }
            }

            samples.Sort();
            var lightPeak = samples[(int)(samples.Count * 0.95)];
            var darkPeak = samples[(int)(samples.Count * 0.05)];
            if (lightPeak - darkPeak < 0.02)
            {
                continue;
            }

            // Ink cluster: true text is substantially darker than paper. Mid chrome
            // grays (nav/field borders ~C5D0DC) must not become the "dark" sample.
            var inkFloor = lightPeak - 0.35;
            var inkSamples = samples.Where(sample => sample <= inkFloor).ToList();
            var bgSamples = samples.Where(sample => sample >= lightPeak - 0.08).ToList();
            if (inkSamples.Count < 24 || inkSamples.Count < samples.Count * 0.08)
            {
                // Sparse/misfit crops (short-label dilution or mis-mapped chrome).
                continue;
            }

            if (bgSamples.Count < 24)
            {
                continue;
            }

            inkSamples.Sort();
            bgSamples.Sort();
            var dark = inkSamples[inkSamples.Count / 2];
            var light = bgSamples[bgSamples.Count / 2];
            if (light - dark < 0.15)
            {
                continue;
            }

            measured++;
            var ratio = (light + 0.05) / (dark + 0.05);
            var font = FontSize(node);
            var minimum = font >= 18 ? 3.0 : 4.5;
            if (ratio + 0.01 < minimum)
            {
                var crop = SaveCrop(bitmap, x, y, width, height, artifactDirectory, frameId, node.Name);
                failDetail = "contrast " + ratio.ToString("0.00", CultureInfo.InvariantCulture)
                    + " is below " + minimum.ToString("0.0", CultureInfo.InvariantCulture)
                    + " on " + Label(node);
                evidence = crop;
                break;
            }
        }

        if (failDetail is not null)
        {
            return new UsabilityCheck("low-contrast", "fail", failDetail, evidence ?? "crop");
        }

        if (measured == 0)
        {
            return new UsabilityCheck("low-contrast", "not-detectable", "Text regions had no separable light and dark pixels.", "sampled " + texts.Count);
        }

        return new UsabilityCheck("low-contrast", "pass", measured + " text regions met the contrast floor.", "sampled " + measured);
    }

    private static UsabilityCheck AboutCutoff(IReadOnlyList<TreeNode> visible)
    {
        var nodes = visible.Where(node =>
            node.Name is "CopyrightText" or "AttributionText" or "AttributionScope"
            || (node.Name?.StartsWith("Attribution", StringComparison.Ordinal) ?? false)).ToList();
        if (nodes.Count == 0)
        {
            return new UsabilityCheck(
                "about-cutoff",
                "not-run",
                "This frame has no About copyright or attribution text.",
                "CopyrightText absent");
        }

        var hits = new List<string>();
        foreach (var node in nodes)
        {
            var text = TextOf(node);
            var font = FontSize(node);
            var bounds = node.AbsoluteBounds;
            var wrapping = Prop(node, "TextWrapping");
            var trimming = Prop(node, "TextTrimming");
            if (text.Length == 0)
            {
                hits.Add(Label(node) + " is empty");
                continue;
            }

            if (trimming.Length > 0 && !trimming.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                hits.Add(Label(node) + " TextTrimming " + trimming);
            }

            if (!wrapping.Contains("Wrap", StringComparison.OrdinalIgnoreCase))
            {
                hits.Add(Label(node) + " TextWrapping is not Wrap");
            }

            if (font > bounds.Height + 1)
            {
                hits.Add(Label(node) + " font " + font.ToString("0") + " exceeds height " + bounds.Height.ToString("0"));
                continue;
            }

            if (bounds.Width < 8)
            {
                hits.Add(Label(node) + " width " + bounds.Width.ToString("0"));
                continue;
            }

            var lines = Math.Max(1, (int)Math.Ceiling(text.Length * font * 0.45 / bounds.Width));
            var required = lines * font;
            if (bounds.Height + 2 < required)
            {
                hits.Add(Label(node) + " height " + bounds.Height.ToString("0")
                    + " is below " + required.ToString("0") + " for " + lines + " lines");
            }
        }

        return hits.Count == 0
            ? new UsabilityCheck("about-cutoff", "pass", "About copyright and attributions wrap inside their boxes.", "CopyrightText AttributionText")
            : new UsabilityCheck("about-cutoff", "fail", string.Join("; ", hits.Take(4)), Snippet(visible, hits[0]));
    }

    private static UsabilityCheck ClippedText(IReadOnlyList<TreeNode> visible)
    {
        var hits = new List<string>();
        foreach (var node in visible.Where(item => item.TypeName.Contains("TextBlock", StringComparison.Ordinal)))
        {
            var text = TextOf(node);
            if (text.Length == 0)
            {
                continue;
            }

            var font = FontSize(node);
            var bounds = node.AbsoluteBounds;
            if (font > bounds.Height + 1)
            {
                hits.Add(Label(node) + " font " + font.ToString("0") + " height " + bounds.Height.ToString("0"));
                continue;
            }

            var wrap = Prop(node, "TextWrapping");
            if (!wrap.Contains("Wrap", StringComparison.OrdinalIgnoreCase)
                && text.Length * font * 0.45 > bounds.Width + 8)
            {
                hits.Add(Label(node) + " text does not fit the arranged width");
            }
        }

        return hits.Count == 0
            ? new UsabilityCheck("clipped-text", "pass", "Visible text fits its arranged box.", "text nodes checked")
            : new UsabilityCheck("clipped-text", "fail", string.Join("; ", hits.Take(4)), Snippet(visible, hits[0]));
    }

    private static UsabilityCheck TruncatedText(IReadOnlyList<TreeNode> visible)
    {
        var hits = visible.Where(node =>
        {
            var trimming = Prop(node, "TextTrimming");
            return trimming.Length > 0 && !trimming.Equals("None", StringComparison.OrdinalIgnoreCase);
        }).Select(Label).Take(4).ToList();
        return hits.Count == 0
            ? new UsabilityCheck("truncated-text", "pass", "No visible text uses trimming.", "TextTrimming")
            : new UsabilityCheck("truncated-text", "fail", string.Join("; ", hits), "TextTrimming is not None");
    }

    private static UsabilityCheck TextOverflow(IReadOnlyList<TreeNode> visible, IReadOnlyDictionary<string, TreeNode> byId)
    {
        var hits = new List<string>();
        foreach (var node in visible.Where(item => item.TypeName.Contains("TextBlock", StringComparison.Ordinal) && TextOf(item).Length > 0))
        {
            if (string.IsNullOrEmpty(node.ParentId) || !byId.TryGetValue(node.ParentId, out var parent))
            {
                continue;
            }

            var child = node.AbsoluteBounds;
            var host = parent.AbsoluteBounds;
            if (child.X < host.X - 4
                || child.Y < host.Y - 4
                || child.X + child.Width > host.X + host.Width + 4
                || child.Y + child.Height > host.Y + host.Height + 4)
            {
                hits.Add(Label(node) + " outside " + Label(parent));
            }
        }

        return hits.Count == 0
            ? new UsabilityCheck("text-overflow", "pass", "Visible text stays inside its parent.", "parent bounds")
            : new UsabilityCheck("text-overflow", "fail", string.Join("; ", hits.Take(4)), hits[0]);
    }

    private static UsabilityCheck OverlappingControls(IReadOnlyList<TreeNode> visible, IReadOnlyDictionary<string, TreeNode> byId)
    {
        var controls = visible.Where(node =>
            node.TypeName.Contains("Button", StringComparison.Ordinal)
            || node.TypeName.Contains("CheckBox", StringComparison.Ordinal)
            || (node.TypeName.Contains("TextBlock", StringComparison.Ordinal) && TextOf(node).Length > 0)).ToList();
        var hits = new List<string>();
        for (var i = 0; i < controls.Count; i++)
        {
            for (var j = i + 1; j < controls.Count; j++)
            {
                if (IsAncestor(controls[i], controls[j], byId) || IsAncestor(controls[j], controls[i], byId))
                {
                    continue;
                }

                if (Overlaps(controls[i].AbsoluteBounds, controls[j].AbsoluteBounds))
                {
                    hits.Add(Label(controls[i]) + " overlaps " + Label(controls[j]));
                }
            }
        }

        return hits.Count == 0
            ? new UsabilityCheck("overlapping-controls", "pass", "No visible controls overlap.", "buttons, checks, text")
            : new UsabilityCheck("overlapping-controls", "fail", string.Join("; ", hits.Take(4)), hits[0]);
    }

    private static UsabilityCheck EmptyIcon(IReadOnlyList<TreeNode> visible, IReadOnlyDictionary<string, TreeNode> byId)
    {
        var hits = new List<string>();
        foreach (var node in visible)
        {
            var slot = node.TypeName.Contains("Border", StringComparison.Ordinal)
                || node.TypeName.Contains("Image", StringComparison.Ordinal)
                || node.TypeName.Contains("Path", StringComparison.Ordinal)
                || node.TypeName.Contains("Viewbox", StringComparison.Ordinal);
            if (!slot)
            {
                continue;
            }

            var bounds = node.AbsoluteBounds;
            if (bounds.Width < 24 || bounds.Width > 80 || bounds.Height < 24 || bounds.Height > 80)
            {
                continue;
            }

            if (Math.Abs(bounds.Width - bounds.Height) > 12)
            {
                continue;
            }

            var hasMark = visible.Any(child =>
                child.Id != node.Id
                && IsAncestor(node, child, byId)
                && (child.TypeName.Contains("Path", StringComparison.Ordinal)
                    || child.TypeName.Contains("Image", StringComparison.Ordinal)
                    || TextOf(child).Length > 0));
            if (!hasMark)
            {
                hits.Add(Label(node) + " " + bounds.Width.ToString("0") + "x" + bounds.Height.ToString("0"));
            }
        }

        return hits.Count == 0
            ? new UsabilityCheck("empty-icon", "pass", "No empty square icon slot.", "border and image nodes")
            : new UsabilityCheck("empty-icon", "fail", string.Join("; ", hits.Take(4)), hits[0]);
    }

    private static UsabilityCheck MissingIcons(IReadOnlyList<TreeNode> visible, int baselineIconGroups)
    {
        var live = visible.Count(node =>
            node.TypeName.Contains("Path", StringComparison.Ordinal)
            || node.TypeName.Contains("Image", StringComparison.Ordinal)
            || node.Name.Contains("Icon", StringComparison.OrdinalIgnoreCase));
        if (baselineIconGroups < 4)
        {
            return new UsabilityCheck("missing-icons", "not-detectable", "Baseline SVG has fewer than 4 icon groups.", "groups " + baselineIconGroups);
        }

        if (live == 0)
        {
            return new UsabilityCheck(
                "missing-icons",
                "fail",
                "Baseline has " + baselineIconGroups + " icon groups and the live tree has no Path, Image, or Icon node.",
                "baseline groups " + baselineIconGroups);
        }

        return new UsabilityCheck("missing-icons", "pass", "Live tree has " + live + " icon nodes.", "baseline groups " + baselineIconGroups);
    }

    private static UsabilityCheck BrokenLayout(IReadOnlyList<TreeNode> visible, string expectedScreenId, string? actualScreenId)
    {
        if (!string.Equals(actualScreenId, expectedScreenId, StringComparison.Ordinal))
        {
            return new UsabilityCheck(
                "broken-layout",
                "fail",
                "Screen was '" + (actualScreenId ?? "(missing)") + "', expected " + expectedScreenId + ".",
                "ScreenId");
        }

        if (visible.Count == 0)
        {
            return new UsabilityCheck("broken-layout", "fail", "The visual tree has no visible nodes.", "empty tree");
        }

        var rootWidth = visible.Max(node => node.AbsoluteBounds.X + node.AbsoluteBounds.Width);
        var rootHeight = visible.Max(node => node.AbsoluteBounds.Y + node.AbsoluteBounds.Height);
        var outside = visible.Where(node =>
            node.TypeName.Contains("Button", StringComparison.Ordinal)
            && (node.AbsoluteBounds.X < -8
                || node.AbsoluteBounds.Y < -8
                || node.AbsoluteBounds.X + node.AbsoluteBounds.Width > rootWidth + 8
                || node.AbsoluteBounds.Y + node.AbsoluteBounds.Height > rootHeight + 8)).Select(Label).Take(3).ToList();
        if (outside.Count > 0)
        {
            return new UsabilityCheck("broken-layout", "fail", string.Join("; ", outside), "button outside root");
        }

        return new UsabilityCheck("broken-layout", "pass", "Screen id matches and buttons stay in the tree bounds.", expectedScreenId);
    }

    private static string SaveCrop(SKBitmap bitmap, int x, int y, int width, int height, string artifactDirectory, string frameId, string name)
    {
        Directory.CreateDirectory(artifactDirectory);
        var safe = string.IsNullOrWhiteSpace(name) ? "text" : Regex.Replace(name, "[^A-Za-z0-9_-]", "");
        var path = Path.Combine(artifactDirectory, frameId + "-contrast-" + safe + ".png");
        using var subset = new SKBitmap(width, height);
        bitmap.ExtractSubset(subset, new SKRectI(x, y, x + width, y + height));
        using var image = SKImage.FromBitmap(subset);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        using var stream = File.Create(path);
        data.SaveTo(stream);
        return path;
    }

    private static double Luminance(SKColor color)
    {
        double Channel(byte value)
        {
            var s = value / 255.0;
            return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(color.Red)) + (0.7152 * Channel(color.Green)) + (0.0722 * Channel(color.Blue));
    }

    private static bool Overlaps(Rect left, Rect right)
    {
        var x = Math.Min(left.X + left.Width, right.X + right.Width) - Math.Max(left.X, right.X);
        var y = Math.Min(left.Y + left.Height, right.Y + right.Height) - Math.Max(left.Y, right.Y);
        return x > 8 && y > 8;
    }

    private static bool IsAncestor(TreeNode ancestor, TreeNode node, IReadOnlyDictionary<string, TreeNode> byId)
    {
        var currentId = node.ParentId;
        var guard = 0;
        while (!string.IsNullOrEmpty(currentId) && guard++ < 32 && byId.TryGetValue(currentId, out var current))
        {
            if (current.Id == ancestor.Id)
            {
                return true;
            }

            currentId = current.ParentId;
        }

        return false;
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

    private static double FontSize(TreeNode node)
    {
        var raw = Prop(node, "FontSize");
        return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var size) && size > 0
            ? size
            : 16;
    }

    private static string Prop(TreeNode node, string name) =>
        node.Properties.FirstOrDefault(property => property.Name == name)?.Value ?? string.Empty;

    private static string Label(TreeNode node) =>
        (string.IsNullOrEmpty(node.Name) ? node.TypeName : node.Name) + ":" + TextOf(node);

    private static string Snippet(IReadOnlyList<TreeNode> visible, string hint) =>
        hint + " visible=" + visible.Count;
}
