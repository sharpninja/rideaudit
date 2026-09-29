// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

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
