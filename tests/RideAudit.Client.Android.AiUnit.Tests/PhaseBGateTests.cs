// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text.Json;
using Xunit;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed class PhaseBGateTests
{
    [Fact]
    public void Fidelity_failure_stays_a_failure_when_the_advisory_pixel_ratio_would_pass()
    {
        var checks = new[]
        {
            new UsabilityCheck("controls", "fail", "missing Stop", "wireframe.svg"),
            new UsabilityCheck("layout", "pass", "order matches", "wireframe.svg"),
            new UsabilityCheck("style", "pass", "colors match", "wireframe.svg"),
        };

        var failures = FidelityVerdict.Failures("WF-04", checks);
        var advisory = FidelityVerdict.AdvisoryPixel("WF-04", 0.01, "WF-04-diff.png");
        var report = string.Join(Environment.NewLine, failures.Concat([advisory]));

        Assert.Contains("WF-04 controls fail", report, StringComparison.Ordinal);
        Assert.Contains("advisory pixel ratio", report, StringComparison.Ordinal);
        Assert.DoesNotContain("advisory pixel ratio", string.Join(Environment.NewLine, failures), StringComparison.Ordinal);
    }

    [Fact]
    public void Advisory_pixel_miss_does_not_create_a_fidelity_failure()
    {
        var checks = new[]
        {
            new UsabilityCheck("controls", "pass", "missing 0", "wireframe.svg"),
            new UsabilityCheck("layout", "pass", "order matches", "wireframe.svg"),
            new UsabilityCheck("style", "pass", "colors match", "wireframe.svg"),
            new UsabilityCheck("low-contrast", "pass", "regions met the floor", "crop"),
        };

        var failures = FidelityVerdict.Failures("WF-05", checks);
        Assert.Empty(failures);
        var advisory = FidelityVerdict.AdvisoryPixel("WF-05", 0.35, "WF-05-diff.png");
        Assert.Contains("0.3500", advisory, StringComparison.Ordinal);
        Assert.Contains("not the pass or fail bar", advisory, StringComparison.Ordinal);
    }

    [Fact]
    public void Review_screen_storyboard_step_is_undriven_rather_than_skipped()
    {
        var step = new StoryboardStep(3, "Open review", "WF-R-01.svg", "WF-R-01", AlternateBranch: false);
        Assert.Null(StoryboardSequence.Clicks("WF-01", step, "driver"));
        var reason = StoryboardSequence.UndrivenReason("WF-01", step);
        Assert.Contains("not hosted", reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("skip", reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Seal_storyboard_path_uses_the_seal_link_instead_of_start_and_stop()
    {
        var seal = new StoryboardStep(4, "Seal", "WF-06.svg", "WF-06", AlternateBranch: false);
        var clicks = StoryboardSequence.Clicks("WF-01", seal, "driver");
        Assert.NotNull(clicks);
        Assert.Contains("SealLinkButton", clicks);
        Assert.DoesNotContain("StopButton", clicks);
        Assert.DoesNotContain("StartButton", clicks);
    }

    [Fact]
    public void Authorized_slate_meets_wcag_aa_on_capture_backgrounds()
    {
        Assert.True(SlateContrast.Ratio(SlateContrast.Muted, "FFFFFF") >= 4.5);
        Assert.True(SlateContrast.Ratio(SlateContrast.Muted, "F4F7FA") >= 4.5);
        Assert.True(SlateContrast.Ratio(SlateContrast.Disabled, "FFFFFF") >= 4.5);
        Assert.True(SlateContrast.Ratio(SlateContrast.Disabled, "E6ECF1") >= 4.5);
        Assert.True(SlateContrast.Ratio(SlateContrast.Disabled, "FFFFFF") >= 3.0);
    }

    [Fact]
    public void Wireframes_and_capture_shell_use_the_authorized_slate_and_not_the_retired_inks()
    {
        var root = VisualAssetCatalog.RepoRoot();
        var axaml = File.ReadAllText(Path.Combine(root, "src", "RideAudit.Shared.Ui", "Views", "CaptureShellView.axaml"));
        Assert.Contains("#" + SlateContrast.Muted, axaml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#" + SlateContrast.Disabled, axaml, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(SlateContrast.RetiredHits(axaml));

        var wireframes = Directory.GetFiles(Path.Combine(root, "docs", "ux", "assets", "wireframes"), "WF-*.svg");
        Assert.NotEmpty(wireframes);
        foreach (var path in wireframes)
        {
            var svg = File.ReadAllText(path);
            Assert.Empty(SlateContrast.RetiredHits(svg));
        }
    }

    [Fact]
    public void Known_failing_labels_share_one_hex_between_wireframe_and_app()
    {
        var root = VisualAssetCatalog.RepoRoot();
        var axaml = File.ReadAllText(Path.Combine(root, "src", "RideAudit.Shared.Ui", "Views", "CaptureShellView.axaml"));
        var wf04 = File.ReadAllText(Path.Combine(root, "docs", "ux", "assets", "wireframes", "WF-04-driver-dashboard.svg"));
        var wf05 = File.ReadAllText(Path.Combine(root, "docs", "ux", "assets", "wireframes", "WF-05-passenger-capture-spider.svg"));
        var wf06 = File.ReadAllText(Path.Combine(root, "docs", "ux", "assets", "wireframes", "WF-06-seal-progress.svg"));

        Assert.True(SlateContrast.SameHex(SlateContrast.FillForText(wf04, "Stop")!, SlateContrast.Disabled));
        Assert.True(SlateContrast.SameHex(SlateContrast.FillForText(wf05, "+Ax")!, SlateContrast.Muted));
        Assert.True(SlateContrast.SameHex(SlateContrast.FillForText(wf06, "4")!, SlateContrast.Disabled));
        Assert.Contains("Text=\"Stop\"", axaml, StringComparison.Ordinal);
        Assert.Contains("Foreground=\"#" + SlateContrast.Disabled + "\" Text=\"Stop\"", axaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"+Ax\"", axaml, StringComparison.Ordinal);
        Assert.Contains("Foreground=\"#" + SlateContrast.Muted + "\" Text=\"+Ax\"", axaml, StringComparison.Ordinal);
        Assert.False(SlateContrast.SameHex(SlateContrast.Muted, SlateContrast.Disabled));
    }

    [Fact]
    public void Codex_subscription_timeout_is_above_the_expired_60_second_budget()
    {
        var path = Path.Combine(
            VisualAssetCatalog.RepoRoot(),
            "tests",
            "RideAudit.Client.Android.AiUnit.Tests",
            "appsettings.aiunit.json");
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        var seconds = json.RootElement
            .GetProperty("AiUnit")
            .GetProperty("Strategies")
            .GetProperty("codex-subscription")
            .GetProperty("TimeoutSeconds")
            .GetInt32();
        Assert.True(seconds >= 180, "TimeoutSeconds is " + seconds + ". The 60 second budget expired before codex returned.");
    }
}
