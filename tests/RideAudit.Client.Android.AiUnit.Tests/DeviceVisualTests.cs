// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

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
        Assert.Null(StoryboardSequence.Clicks("WF-01", reviewSteps[0], "driver"));
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
