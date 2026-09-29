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
