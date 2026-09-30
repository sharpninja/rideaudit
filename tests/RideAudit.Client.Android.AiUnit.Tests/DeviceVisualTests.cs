// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.RemoteControl.Protocol.V1;
using SharpNinja.AiUnit.Strategy;
using SharpNinja.AiUnit.Xunit;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed class VisualCatalogTests
{
    [Fact]
    public void Catalog_lists_every_wireframe_and_storyboard_markdown()
    {
        var wireframes = VisualAssetCatalog.Wireframes();
        var storyboards = VisualAssetCatalog.Storyboards();
        Assert.Equal(16, wireframes.Count);
        Assert.Equal(12, storyboards.Count);
        Assert.Contains(wireframes, asset => asset.Id == "WF-01");
        Assert.Contains(wireframes, asset => asset.Id == "WF-R-08");
        Assert.Contains(storyboards, asset => asset.Id == "SB-01");
        Assert.Contains(storyboards, asset => asset.Id == "SB-R-06");
        Assert.All(wireframes, asset => Assert.NotEmpty(asset.FrameSvgPaths));
        Assert.All(storyboards, asset => Assert.NotEmpty(asset.FrameSvgPaths));

        var pairing = storyboards.Single(asset => asset.Id == "SB-01");
        var pairingSteps = VisualAssetCatalog.ParseSteps(pairing.MarkdownPath);
        Assert.Equal(new[] { "WF-01", "WF-02", "WF-08", "WF-03", "WF-08", "WF-04" }, pairingSteps.Select(step => step.ScreenId).ToArray());
        Assert.Equal(new[] { 1, 2, 2, 3, 3, 4 }, pairingSteps.Select(step => step.Beat).ToArray());
        Assert.Equal(2, pairingSteps.Count(step => step.AlternateBranch));
        Assert.True(pairingSteps.Count > pairing.FrameSvgPaths.Count);
        Assert.Equal(new[] { "DriverButton" }, StoryboardSequence.Clicks("WF-01", pairingSteps[0], "driver"));
        Assert.Null(StoryboardSequence.Clicks("WF-02", pairingSteps[2], "driver"));
        Assert.Equal(new[] { "PeerButton" }, StoryboardSequence.Clicks("WF-02", pairingSteps[3], "driver"));

        var driver = storyboards.Single(asset => asset.Id == "SB-02");
        var driverSteps = VisualAssetCatalog.ParseSteps(driver.MarkdownPath);
        Assert.Equal(4, driverSteps.Count(step => step.ScreenId == "WF-04"));
        Assert.Equal(new[] { "StartButton" }, StoryboardSequence.Clicks("WF-04", driverSteps[3], "driver"));

        var review = storyboards.Single(asset => asset.Id == "SB-R-01");
        var reviewSteps = VisualAssetCatalog.ParseSteps(review.MarkdownPath);
        Assert.NotEmpty(reviewSteps);
        Assert.All(reviewSteps, step => Assert.StartsWith("WF-R-", step.ScreenId, StringComparison.Ordinal));
        Assert.Equal(
            new[] { "Show" + reviewSteps[0].ScreenId.Replace("-", string.Empty, StringComparison.Ordinal) },
            StoryboardSequence.Clicks("WF-01", reviewSteps[0], "driver"));
    }
}

public sealed class UsabilityInspectorTests
{
    [Fact]
    public void Empty_icon_slot_and_missing_baseline_icons_fail()
    {
        var nodes = new List<TreeNode>
        {
            new()
            {
                Id = "root",
                TypeName = "Panel",
                IsVisible = true,
                AbsoluteBounds = new Rect { Width = 400, Height = 800 },
            },
            new()
            {
                Id = "mark",
                ParentId = "root",
                TypeName = "Border",
                IsVisible = true,
                AbsoluteBounds = new Rect { X = 160, Y = 40, Width = 64, Height = 64 },
            },
            new()
            {
                Id = "title",
                ParentId = "root",
                TypeName = "TextBlock",
                Name = "TitleText",
                IsVisible = true,
                AbsoluteBounds = new Rect { X = 20, Y = 120, Width = 200, Height = 28 },
                Properties = { new PropertyValue { Name = "Text", Value = "RideAudit" }, new PropertyValue { Name = "FontSize", Value = "28" } },
            },
        };

        var checks = UsabilityInspector.InspectTree(nodes, "WF-01", "WF-01", baselineIconGroups: 8);
        Assert.Equal("fail", checks.Single(check => check.Id == "empty-icon").Status);
        Assert.Equal("fail", checks.Single(check => check.Id == "missing-icons").Status);
        Assert.Equal("pass", checks.Single(check => check.Id == "broken-layout").Status);
    }

    [Fact]
    public void Clipped_text_and_overlap_fail_even_when_screen_id_matches()
    {
        var nodes = new List<TreeNode>
        {
            new()
            {
                Id = "a",
                TypeName = "Button",
                Name = "DriverButton",
                IsVisible = true,
                AbsoluteBounds = new Rect { X = 10, Y = 10, Width = 100, Height = 40 },
            },
            new()
            {
                Id = "b",
                TypeName = "Button",
                Name = "PassengerButton",
                IsVisible = true,
                AbsoluteBounds = new Rect { X = 40, Y = 20, Width = 100, Height = 40 },
            },
            new()
            {
                Id = "t",
                TypeName = "TextBlock",
                Name = "ClockText",
                IsVisible = true,
                AbsoluteBounds = new Rect { X = 10, Y = 80, Width = 40, Height = 8 },
                Properties =
                {
                    new PropertyValue { Name = "Text", Value = "Driver session clock master" },
                    new PropertyValue { Name = "FontSize", Value = "16" },
                },
            },
        };

        var checks = UsabilityInspector.InspectTree(nodes, "WF-04", "WF-04", baselineIconGroups: 0);
        Assert.Equal("fail", checks.Single(check => check.Id == "clipped-text").Status);
        Assert.Equal("fail", checks.Single(check => check.Id == "overlapping-controls").Status);
        Assert.Equal("not-detectable", checks.Single(check => check.Id == "missing-icons").Status);
        Assert.Equal("not-run", checks.Single(check => check.Id == "about-cutoff").Status);
    }

    [Fact]
    public void About_copyright_cutoff_fails_when_the_box_is_too_short()
    {
        var clipped = new List<TreeNode>
        {
            new()
            {
                Id = "copy",
                TypeName = "TextBlock",
                Name = "CopyrightText",
                IsVisible = true,
                AbsoluteBounds = new Rect { X = 20, Y = 80, Width = 320, Height = 16 },
                Properties =
                {
                    new PropertyValue { Name = "Text", Value = "RideAudit UI. Copyright (C) 2026 RideAudit contributors. Licensed GPL-2.0-or-later. In-scope RideAudit UI code is not relicensed MIT or Apache-2.0." },
                    new PropertyValue { Name = "FontSize", Value = "16" },
                    new PropertyValue { Name = "TextWrapping", Value = "Wrap" },
                    new PropertyValue { Name = "TextTrimming", Value = "None" },
                },
            },
        };

        var failed = UsabilityInspector.InspectTree(clipped, "ABOUT", "ABOUT", baselineIconGroups: 0);
        Assert.Equal("fail", failed.Single(check => check.Id == "about-cutoff").Status);

        var fitted = new List<TreeNode>
        {
            new()
            {
                Id = "copy",
                TypeName = "TextBlock",
                Name = "CopyrightText",
                IsVisible = true,
                AbsoluteBounds = new Rect { X = 20, Y = 80, Width = 320, Height = 96 },
                Properties =
                {
                    new PropertyValue { Name = "Text", Value = "RideAudit UI. Copyright (C) 2026 RideAudit contributors. Licensed GPL-2.0-or-later." },
                    new PropertyValue { Name = "FontSize", Value = "16" },
                    new PropertyValue { Name = "TextWrapping", Value = "Wrap" },
                    new PropertyValue { Name = "TextTrimming", Value = "None" },
                },
            },
            new()
            {
                Id = "credit",
                TypeName = "TextBlock",
                Name = "AttributionText",
                IsVisible = true,
                AbsoluteBounds = new Rect { X = 20, Y = 180, Width = 320, Height = 96 },
                Properties =
                {
                    new PropertyValue { Name = "Text", Value = "Avalonia UI 12. License: MIT. Credit: AvaloniaUI authors." },
                    new PropertyValue { Name = "FontSize", Value = "16" },
                    new PropertyValue { Name = "TextWrapping", Value = "Wrap" },
                    new PropertyValue { Name = "TextTrimming", Value = "None" },
                },
            },
        };

        var passed = UsabilityInspector.InspectTree(fitted, "ABOUT", "ABOUT", baselineIconGroups: 0);
        Assert.Equal("pass", passed.Single(check => check.Id == "about-cutoff").Status);
    }

    [Fact]
    public void Low_contrast_sample_fails_and_a_dark_on_white_sample_passes()
    {
        var node = new TreeNode
        {
            Id = "t",
            TypeName = "TextBlock",
            Name = "TitleText",
            IsVisible = true,
            AbsoluteBounds = new Rect { Width = 20, Height = 10 },
            Properties = { new PropertyValue { Name = "Text", Value = "RideAudit" }, new PropertyValue { Name = "FontSize", Value = "16" } },
        };
        using var weak = new SkiaSharp.SKBitmap(20, 10);
        weak.Erase(new SkiaSharp.SKColor(180, 180, 180));
        for (var y = 0; y < 5; y++)
        {
            for (var x = 0; x < 20; x++)
            {
                weak.SetPixel(x, y, new SkiaSharp.SKColor(170, 170, 170));
            }
        }

        var weakCheck = UsabilityInspector.InspectContrast(weak, [node], Path.GetTempPath(), "weak");
        Assert.Equal("fail", weakCheck.Status);

        using var strong = new SkiaSharp.SKBitmap(20, 10);
        strong.Erase(SkiaSharp.SKColors.White);
        for (var y = 0; y < 5; y++)
        {
            for (var x = 0; x < 20; x++)
            {
                strong.SetPixel(x, y, SkiaSharp.SKColors.Black);
            }
        }

        var strongCheck = UsabilityInspector.InspectContrast(strong, [node], Path.GetTempPath(), "strong");
        Assert.Equal("pass", strongCheck.Status);
    }
}

public sealed class WireframeReflectionTests
{
    [Fact]
    public void Missing_label_and_inverted_order_fail_controls_and_layout()
    {
        var svg = Path.Combine(Path.GetTempPath(), "rideaudit-reflection-" + Guid.NewGuid().ToString("N") + ".svg");
        File.WriteAllText(
            svg,
            "<svg xmlns=\"http://www.w3.org/2000/svg\">"
            + "<text x=\"54\" y=\"57\" fill=\"#1A2433\" font-size=\"13\">12:56</text>"
            + "<text x=\"230\" y=\"184\" fill=\"#1A2433\" font-size=\"28\">RideAudit</text>"
            + "<text x=\"130\" y=\"318\" fill=\"#1A2433\" font-size=\"16\">Driver</text>"
            + "<text x=\"130\" y=\"426\" fill=\"#1A2433\" font-size=\"16\">Passenger</text>"
            + "</svg>");
        try
        {
            var nodes = new List<TreeNode>
            {
                Text("title", "RideAudit", 200, "28", "#1A2433"),
                Text("passenger", "Passenger", 40, "16", "#1A2433"),
                Text("extra", "Live check pending", 120, "13", "#5C6B7C"),
            };
            var checks = WireframeReflection.Judge(svg, nodes);
            Assert.Equal("fail", checks.Single(check => check.Id == "controls").Status);
            Assert.Contains("Driver", checks.Single(check => check.Id == "controls").Detail, StringComparison.Ordinal);
            Assert.Contains("Live check pending", checks.Single(check => check.Id == "controls").Detail, StringComparison.Ordinal);
            Assert.Equal("fail", checks.Single(check => check.Id == "layout").Status);
            Assert.Equal("pass", checks.Single(check => check.Id == "style").Status);
        }
        finally
        {
            File.Delete(svg);
        }
    }

    [Fact]
    public void Shared_labels_in_wireframe_order_pass_when_type_matches()
    {
        var svg = Path.Combine(Path.GetTempPath(), "rideaudit-reflection-" + Guid.NewGuid().ToString("N") + ".svg");
        File.WriteAllText(
            svg,
            "<svg xmlns=\"http://www.w3.org/2000/svg\">"
            + "<text x=\"230\" y=\"184\" fill=\"#1A2433\" font-size=\"28\">RideAudit</text>"
            + "<text x=\"130\" y=\"318\" fill=\"#1A2433\" font-size=\"16\">Driver</text>"
            + "</svg>");
        try
        {
            var nodes = new List<TreeNode>
            {
                Text("title", "RideAudit", 40, "28", "#1A2433"),
                Text("driver", "Driver", 200, "16", "#1A2433"),
                Text("screen", "WF-01", 0, "16", "#F4F7FA"),
            };
            var checks = WireframeReflection.Judge(svg, nodes);
            Assert.Equal("pass", checks.Single(check => check.Id == "controls").Status);
            Assert.Equal("pass", checks.Single(check => check.Id == "layout").Status);
            Assert.Equal("pass", checks.Single(check => check.Id == "style").Status);
        }
        finally
        {
            File.Delete(svg);
        }
    }

    [Fact]
    public void Text_inside_a_hidden_page_is_not_an_extra_control()
    {
        var svg = Path.Combine(Path.GetTempPath(), "rideaudit-reflection-" + Guid.NewGuid().ToString("N") + ".svg");
        File.WriteAllText(
            svg,
            "<svg xmlns=\"http://www.w3.org/2000/svg\">"
            + "<text x=\"230\" y=\"184\" fill=\"#1A2433\" font-size=\"28\">RideAudit</text>"
            + "<text x=\"130\" y=\"318\" fill=\"#1A2433\" font-size=\"16\">Driver</text>"
            + "</svg>");
        try
        {
            var nodes = new List<TreeNode>
            {
                new()
                {
                    Id = "role",
                    TypeName = "StackPanel",
                    IsVisible = false,
                    AbsoluteBounds = new Rect { Width = 400, Height = 800 },
                },
                new()
                {
                    Id = "hidden-label",
                    ParentId = "role",
                    TypeName = "TextBlock",
                    IsVisible = true,
                    AbsoluteBounds = new Rect { X = 16, Y = 300, Width = 200, Height = 24 },
                    Properties = { new PropertyValue { Name = "Text", Value = "Passenger" } },
                },
                Text("title", "RideAudit", 40, "28", "#1A2433"),
                Text("driver", "Driver", 200, "16", "#1A2433"),
            };
            var checks = WireframeReflection.Judge(svg, nodes);
            Assert.Equal("pass", checks.Single(check => check.Id == "controls").Status);
        }
        finally
        {
            File.Delete(svg);
        }
    }

    [Fact]
    public void Repeated_labels_pair_in_vertical_order()
    {
        var svg = Path.Combine(Path.GetTempPath(), "rideaudit-reflection-" + Guid.NewGuid().ToString("N") + ".svg");
        File.WriteAllText(
            svg,
            "<svg xmlns=\"http://www.w3.org/2000/svg\">"
            + "<text x=\"54\" y=\"275\" fill=\"#173E66\" font-size=\"11\">OTS stamped</text>"
            + "<text x=\"54\" y=\"334\" fill=\"#1A2433\" font-size=\"14\">passenger-composite</text>"
            + "<text x=\"54\" y=\"361\" fill=\"#173E66\" font-size=\"11\">OTS stamped</text>"
            + "</svg>");
        try
        {
            var nodes = new List<TreeNode>
            {
                Text("ots1", "OTS stamped", 100, "11", "#173E66"),
                Text("name", "passenger-composite", 180, "14", "#1A2433"),
                Text("ots2", "OTS stamped", 220, "11", "#173E66"),
            };
            var checks = WireframeReflection.Judge(svg, nodes);
            Assert.Equal("pass", checks.Single(check => check.Id == "layout").Status);
            Assert.Equal("pass", checks.Single(check => check.Id == "controls").Status);
        }
        finally
        {
            File.Delete(svg);
        }
    }

    [Fact]
    public void Review_catalog_matches_wireframe_reflection_order()
    {
        var root = VisualAssetCatalog.RepoRoot();
        var catalogPath = Path.Combine(root, "src", "RideAudit.Shared.Ui", "Views", "CaptureReviewCatalog.cs");
        var catalog = File.ReadAllText(catalogPath);
        var rows = Regex.Matches(
            catalog,
            "new\\(\"(?<screen>WF-R-\\d+)\", \"(?<text>(?:\\\\.|[^\"\\\\])*)\", (?<y>[0-9.]+), (?<font>[0-9.]+), \"(?<fill>[0-9A-Fa-f]*)\"");
        Assert.True(rows.Count > 0, "CaptureReviewCatalog has no review labels.");
        var byScreen = new Dictionary<string, List<(string Text, double Font, string Fill)>>(StringComparer.Ordinal);
        foreach (Match row in rows)
        {
            var screen = row.Groups["screen"].Value;
            if (!byScreen.TryGetValue(screen, out var list))
            {
                list = new List<(string, double, string)>();
                byScreen[screen] = list;
            }

            var text = Regex.Unescape(row.Groups["text"].Value);
            var font = double.Parse(row.Groups["font"].Value, CultureInfo.InvariantCulture);
            list.Add((text, font, row.Groups["fill"].Value.ToUpperInvariant()));
        }

        var wireframes = Directory.GetFiles(Path.Combine(root, "docs", "ux", "assets", "wireframes"), "WF-R-*.svg");
        Assert.Equal(8, wireframes.Length);
        foreach (var path in wireframes)
        {
            var screen = VisualAssetCatalog.ScreenIdFromPath(path);
            var expected = WireframeReflection.LabelsFor(path);
            Assert.True(byScreen.TryGetValue(screen, out var actual), screen + " is missing from the review catalog.");
            Assert.Equal(expected.Count, actual!.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                var fill = expected[i].Fill.Trim().TrimStart('#').ToUpperInvariant();
                Assert.Equal(expected[i].Text, actual[i].Text);
                Assert.Equal(expected[i].FontSize, actual[i].Font, 2);
                Assert.Equal(fill, actual[i].Fill);
            }
        }
    }

    private static TreeNode Text(string id, string text, double y, string fontSize, string foreground) =>
        new()
        {
            Id = id,
            TypeName = "TextBlock",
            IsVisible = true,
            AbsoluteBounds = new Rect { X = 16, Y = y, Width = 200, Height = 24 },
            Properties =
            {
                new PropertyValue { Name = "Text", Value = text },
                new PropertyValue { Name = "FontSize", Value = fontSize },
                new PropertyValue { Name = "Foreground", Value = foreground },
            },
        };
}

public sealed class CodexSubscriptionProfileTests
{
    [Fact]
    public void Codex_subscription_profile_is_the_active_strategy()
    {
        CodexVisualGate.RequireCodexSubscriptionProfile();
        var fixture = AiStrategyFixture.Default;
        var resolved = Assert.IsType<ResolvedStrategy>(fixture.Resolved);
        Assert.Equal(CodexSubscriptionProfile.Name, resolved.Name);
        Assert.Equal("cli", resolved.Kind);
        Assert.Equal("(cli-managed)", resolved.Model);
        Assert.NotNull(fixture.Client);
        Assert.Equal("codex-subscription:codex", fixture.Client.Provider);
        Assert.Equal(string.Empty, fixture.SkipReason);
    }
}

public sealed class WireframeDeviceTests
{
    public static IEnumerable<object[]> Cases() => VisualAssetCatalog.WireframeCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void Device_screenshot_matches_wireframe_baseline(string id, string markdownPath)
    {
        DeviceVisualRunner.AssertWireframe(id, markdownPath);
    }
}

public sealed class StoryboardDeviceTests
{
    public static IEnumerable<object[]> Cases() => VisualAssetCatalog.StoryboardCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void Device_screenshots_match_each_storyboard_frame(string id, string markdownPath)
    {
        DeviceVisualRunner.AssertStoryboard(id, markdownPath);
    }
}
