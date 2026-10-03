// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using RideAudit.Shared.Ui;
using RideAudit.Shared.Ui.Views;
using Xunit;

namespace RideAudit.Client.Tests;

public class ReviewCaptureHostTests
{
    [AvaloniaFact]
    public void Capture_client_hosts_each_review_wireframe_label()
    {
        var shell = new CaptureShellView();
        var window = new Window { Width = 420, Height = 3200, Content = shell };
        window.Show();
        window.UpdateLayout();

        Assert.Equal("WF-01", shell.FindControl<TextBlock>("ScreenId")!.Text);
        Assert.DoesNotContain(VisibleText(shell), block => block.Text == "Open admitted submission");
        var jump = shell.FindControl<Button>("ShowWFR01")!;
        Assert.True(jump.Bounds.Width <= 8, "jump width " + jump.Bounds.Width);

        foreach (var screen in new[] { "WF-R-01", "WF-R-02", "WF-R-03", "WF-R-04", "WF-R-05", "WF-R-06", "WF-R-07", "WF-R-08" })
        {
            var button = shell.FindControl<Button>("Show" + screen.Replace("-", string.Empty, System.StringComparison.Ordinal))!;
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            window.UpdateLayout();
            Assert.Equal(screen, shell.FindControl<TextBlock>("ScreenId")!.Text);

            var expected = CaptureReviewCatalog.Labels.Where(label => label.Screen == screen).ToList();
            var host = shell.FindControl<CaptureReviewHost>("ReviewHost")!;
            var live = host.GetVisualDescendants()
                .OfType<TextBlock>()
                .Where(block => !string.IsNullOrEmpty(block.Text) && Shown(block))
                .ToList();

            // Order-independent fidelity: every catalog label present with font/fill/wrap.
            Assert.Equal(expected.Count, live.Count);
            foreach (var label in expected)
            {
                var matches = live.Where(block => block.Text == label.Text).ToList();
                Assert.True(matches.Count >= 1, screen + " missing live text: " + label.Text);
                var hit = matches.FirstOrDefault(block =>
                {
                    if (System.Math.Abs(block.FontSize - label.FontSize) > 2) return false;
                    if (block.TextWrapping != TextWrapping.Wrap) return false;
                    if (block.Foreground is not ISolidColorBrush brush) return false;
                    var hex = brush.Color.R.ToString("X2") + brush.Color.G.ToString("X2") + brush.Color.B.ToString("X2");
                    return hex == label.Fill;
                });
                Assert.True(hit is not null, screen + " no font/fill/wrap match for: " + label.Text);
            }

            Assert.Contains(host.GetVisualDescendants().OfType<Border>(), border => border.Child is TextBlock text && text.Text == "File");
            Assert.Contains(host.GetVisualDescendants().OfType<Border>(), border =>
                border.CornerRadius.TopLeft >= 10
                && border.Child is TextBlock action
                && action.FontSize >= 14);
        }
    }

    [AvaloniaFact]
    public void Review_screen_hides_the_capture_about_dock()
    {
        var main = new MainView();
        var window = new Window { Width = 420, Height = 800, Content = main };
        window.Show();
        main.ApplyMode(ShellMode.Capture);
        window.UpdateLayout();
        var shell = main.GetVisualDescendants().OfType<CaptureShellView>().Single();
        Assert.True(main.FindControl<Border>("AboutDock")!.IsVisible);
        shell.FindControl<Button>("ShowWFR06")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        window.UpdateLayout();
        Assert.Equal("WF-R-06", shell.FindControl<TextBlock>("ScreenId")!.Text);
        Assert.False(main.FindControl<Border>("AboutDock")!.IsVisible);
        Assert.Contains(VisibleText(shell), block => block.Text == "File");
    }

    private static List<TextBlock> VisibleText(Control root)
    {
        return root.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(block => !string.IsNullOrEmpty(block.Text) && Shown(block))
            .OrderBy(block => block.Bounds.Y)
            .ThenBy(block => block.Bounds.X)
            .ToList();
    }

    private static bool Shown(Visual visual)
    {
        var current = visual;
        while (current is not null)
        {
            if (!current.IsVisible)
            {
                return false;
            }

            current = current.GetVisualParent();
        }

        return true;
    }
}