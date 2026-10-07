// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using RideAudit.Client.Tests.Support;
using RideAudit.Shared.Ui;
using RideAudit.Shared.Ui.Views;
using Xunit;

namespace RideAudit.Client.Tests;

[Trait("Partition", "TEST-RIDE-055")]
[Trait("FR", "FR-RIDE-074")]
[Trait("TR", "TR-RIDE-VIEW-007")]
public class TestRide055AboutTests
{
    [AvaloniaFact]
    [Trait("AC", "AC-TEST-055-001")]
    [Trait("AC", "AC-RIDE-074-002")]
    [Trait("AC", "AC-RIDE-VIEW-007-002")]
    public void Bottom_panel_About_control_opens_About_view()
    {
        var main = new MainView();
        main.ApplyMode(ShellMode.Review);
        var window = new Window { Width = 800, Height = 600, Content = main };
        window.Show();

        Assert.NotNull(main.FindControl<Border>("BottomPanel"));
        var aboutButton = main.FindControl<Button>("AboutButton");
        Assert.NotNull(aboutButton);
        Assert.Equal("About", aboutButton!.Content?.ToString());

        main.OpenAbout();

        var about = Assert.IsType<AboutView>(main.FindControl<ContentControl>("Body")!.Content);
        Assert.Equal("WF-ABOUT", about.FindControl<TextBlock>("ScreenId")!.Text);
    }

    [AvaloniaFact]
    [Trait("AC", "AC-TEST-055-002")]
    [Trait("AC", "AC-RIDE-074-001")]
    [Trait("AC", "AC-RIDE-VIEW-007-001")]
    public void About_view_shows_RideAudit_UI_copyright()
    {
        var about = new AboutView();
        var window = new Window { Width = 600, Height = 400, Content = about };
        window.Show();

        var copyright = about.FindControl<TextBlock>("CopyrightNotice")!.Text;
        Assert.Contains("Copyright (C) 2026 RideAudit contributors", copyright);
        Assert.Contains("GPL-2.0-or-later", copyright);
        Assert.Equal(UiLicense.Notice, copyright);
    }

    [AvaloniaFact]
    [Trait("AC", "AC-TEST-055-003")]
    [Trait("AC", "AC-RIDE-074-003")]
    [Trait("AC", "AC-RIDE-VIEW-007-003")]
    public void Top_title_bar_does_not_show_copyright_notice()
    {
        var main = new MainView();
        main.ApplyMode(ShellMode.Review);
        var window = new Window { Width = 800, Height = 600, Content = main };
        window.Show();

        Assert.Null(main.FindControl<TextBlock>("LicenseNotice"));

        var titleBar = main.FindControl<Border>("TitleBar")!;
        var titleTexts = titleBar.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text ?? string.Empty).ToList();
        Assert.DoesNotContain(titleTexts, t => t.Contains("Copyright", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(titleTexts, t => t.Contains(UiLicense.Notice, StringComparison.Ordinal));

        var framework = main.FindControl<TextBlock>("FrameworkNotice")!.Text;
        Assert.Equal(UiLicense.Framework, framework);
        Assert.DoesNotContain("Copyright", framework, StringComparison.OrdinalIgnoreCase);
    }

    [AvaloniaFact]
    [Trait("AC", "AC-TEST-055-004")]
    [Trait("AC", "AC-RIDE-074-005")]
    [Trait("AC", "AC-RIDE-VIEW-007-004")]
    public void About_view_includes_third_party_attributions_not_copyright_alone()
    {
        var about = new AboutView();
        var window = new Window { Width = 600, Height = 400, Content = about };
        window.Show();

        var attributions = about.FindControl<TextBlock>("AttributionsNotice")!.Text;
        Assert.False(string.IsNullOrWhiteSpace(attributions));
        Assert.Contains("licenses and credits", attributions, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MIT", attributions, StringComparison.Ordinal);
        Assert.Contains("Avalonia", attributions, StringComparison.Ordinal);
        Assert.Equal(UiLicense.Attributions, attributions);

        var copyright = about.FindControl<TextBlock>("CopyrightNotice")!.Text;
        Assert.NotEqual(copyright, attributions);
    }

    [Fact]
    [Trait("AC", "AC-RIDE-074-004")]
    public void Source_headers_and_artifact_GPL_notices_remain()
    {
        var root = Repo.Root();
        var mainViewCs = File.ReadAllText(Path.Combine(root, "src/RideAudit.Shared.Ui/Views/MainView.axaml.cs"));
        Assert.Contains("Copyright (C) 2026 RideAudit contributors", mainViewCs);
        Assert.Contains("SPDX-License-Identifier: GPL-2.0-or-later", mainViewCs);

        var aboutCs = File.ReadAllText(Path.Combine(root, "src/RideAudit.Shared.Ui/Views/AboutView.axaml.cs"));
        Assert.Contains("Copyright (C) 2026 RideAudit contributors", aboutCs);

        var notice = File.ReadAllText(Path.Combine(root, "NOTICE"));
        Assert.Contains("GPL-2.0", notice);
        Assert.Contains("Avalonia", notice);
    }
}
