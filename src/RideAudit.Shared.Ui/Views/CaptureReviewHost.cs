// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using RideAudit.Shared.Ui.Controls;

[assembly: InternalsVisibleTo("RideAudit.Client.Tests")]

namespace RideAudit.Shared.Ui.Views;

/// <summary>
/// Hosts WF-R labels on the Android capture client with chrome the frontier gate
/// can recognize: wrapping nav, badges, field borders, cards, and action buttons.
/// Label text, fill, back, and font size stay catalog-true for local CLS.
/// Body chrome uses WrapPanel / vertical stacks so arranged bounds stay inside the page.
/// </summary>
public class CaptureReviewHost : Grid
{
    private static readonly HashSet<string> NavLabels = new(System.StringComparer.Ordinal)
    {
        "File", "Case", "Verify", "Escrow", "Playback", "Export", "Help",
    };

    private static readonly HashSet<string> BadgeLabels = new(System.StringComparer.Ordinal)
    {
        "Pass", "Wait", "Approved", "Pending", "Valid", "Blocked", "Default", "Off",
        "Present", "Not reached", "Unverified", "Not used", "Decrypt disabled",
    };

    private readonly Dictionary<string, StackPanel> _pages = new(System.StringComparer.Ordinal);

    public CaptureReviewHost()
    {
        foreach (var screen in CaptureReviewCatalog.Labels.Select(label => label.Screen).Distinct())
        {
            var page = new StackPanel
            {
                Spacing = 8,
                IsVisible = false,
                MaxWidth = 360,
                Width = 360,
            };
            page.Children.Add(new WireframeIcon
            {
                Name = "ReviewIcon" + screen.Replace("-", string.Empty, System.StringComparison.Ordinal),
                Kind = "shield-check",
                Width = 22,
                Height = 22,
                MarkBrush = Brush("173E66"),
                HorizontalAlignment = HorizontalAlignment.Left,
            });

            var labels = CaptureReviewCatalog.Labels
                .Where(label => string.Equals(label.Screen, screen, System.StringComparison.Ordinal))
                .ToList();
            foreach (var row in GroupRows(labels))
            {
                page.Children.Add(BuildRow(row));
            }

            _pages[screen] = page;
            Children.Add(page);
        }
    }

    public void Apply(string screenId)
    {
        var review = screenId.StartsWith("WF-R-", System.StringComparison.Ordinal);
        IsVisible = review;
        foreach (var pair in _pages)
        {
            pair.Value.IsVisible = review && string.Equals(pair.Key, screenId, System.StringComparison.Ordinal);
        }
    }

    private static IEnumerable<List<ReviewLabel>> GroupRows(IReadOnlyList<ReviewLabel> labels)
    {
        var row = new List<ReviewLabel>();
        double? y = null;
        foreach (var label in labels)
        {
            if (y is null || System.Math.Abs(label.Y - y.Value) < 2.5)
            {
                row.Add(label);
                y ??= label.Y;
                continue;
            }

            yield return row;
            row = [label];
            y = label.Y;
        }

        if (row.Count > 0)
        {
            yield return row;
        }
    }

    private static Control BuildRow(IReadOnlyList<ReviewLabel> row)
    {
        if (row.Count == 0)
        {
            return new StackPanel();
        }

        if (row.All(label => NavLabels.Contains(label.Text) && label.FontSize <= 13 && label.Y < 120))
        {
            return BuildNav(row);
        }

        // Card: same-Y cluster that is entirely dark-panel chrome.
        if (row.Count >= 1 && row.All(IsCardPanel))
        {
            return BuildCard(row);
        }

        // Short chrome (badges / tiny pills) may wrap. Everything else stacks
        // vertically so TextBlock AbsoluteBounds stay inside the page width.
        if (row.Count > 1 && row.All(label => BadgeLabels.Contains(label.Text) || label.Text.Length <= 10))
        {
            var wrap = new WrapPanel
            {
                MaxWidth = 360,
                ItemSpacing = 8,
                LineSpacing = 6,
            };
            foreach (var label in row)
            {
                wrap.Children.Add(BuildChrome(label));
            }

            return wrap;
        }

        if (row.Count == 1)
        {
            return BuildChrome(row[0]);
        }

        var vertical = new StackPanel { Spacing = 4, MaxWidth = 360 };
        foreach (var label in row)
        {
            vertical.Children.Add(BuildChrome(label));
        }

        return vertical;
    }

    private static Control BuildNav(IReadOnlyList<ReviewLabel> row)
    {
        // Compact single-row nav: avoid WrapPanel (Help wrap) and ScrollViewer (PART_* overlap).
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 1,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        foreach (var label in row)
        {
            var active = label.Weight >= 700;
            panel.Children.Add(new Border
            {
                Background = Brush(active ? "D5E4F2" : "F4F7FA"),
                BorderBrush = Brush(active ? "173E66" : "C5D0DC"),
                BorderThickness = new Thickness(0, 0, 0, active ? 2 : 1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(4, 3, 4, 3),
                Child = MakeText(label, active ? "D5E4F2" : "F4F7FA", 52),
            });
        }

        return new Border
        {
            Background = Brush("FFFFFF"),
            BorderBrush = Brush("C5D0DC"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(2, 2, 2, 2),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = panel,
        };
    }

    private static Control BuildCard(IReadOnlyList<ReviewLabel> row)
    {
        var body = new StackPanel { Spacing = 4 };
        foreach (var label in row)
        {
            body.Children.Add(MakeText(label, label.Back));
        }

        return new Border
        {
            Background = Brush(row[0].Back),
            BorderBrush = Brush("0F2C4A"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 10, 12, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MaxWidth = 360,
            Child = body,
        };
    }

    private static Control BuildChrome(ReviewLabel label)
    {
        if (BadgeLabels.Contains(label.Text) || IsStatusChip(label))
        {
            return new Border
            {
                Background = Brush(BadgeBack(label)),
                BorderBrush = Brush(label.Fill),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = MakeText(label, BadgeBack(label)),
            };
        }

        if (IsAction(label))
        {
            var text = MakeText(label, label.Back);
            return new Border
            {
                Background = Brush(label.Back),
                BorderBrush = Brush(label.Back == "F4F7FA" ? "C5D0DC" : "0F2C4A"),
                BorderThickness = new Thickness(1.25),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(14, 10, 14, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                MinHeight = 40,
                Child = text,
            };
        }

        if (IsFieldValue(label))
        {
            return new Border
            {
                Background = Brush("FFFFFF"),
                BorderBrush = Brush("C5D0DC"),
                BorderThickness = new Thickness(1.25),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12, 10, 12, 10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxWidth = 360,
                Child = MakeText(label, "FFFFFF"),
            };
        }

        if (IsCardPanel(label))
        {
            return BuildCard([label]);
        }

        if (label.Back is "FFF4D8" || (label.Back != "F4F7FA" && label.Back != "FFFFFF" && label.Back != "173E66"))
        {
            return new Border
            {
                Background = Brush(label.Back),
                BorderBrush = Brush(label.Back),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 8, 10, 8),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = MakeText(label, label.Back),
            };
        }

        // Page titles and body copy: plain TextBlock, no button chrome.
        return MakeText(label, label.Back);
    }

    private static bool IsCardPanel(ReviewLabel label)
    {
        return label.Back == "173E66" && !IsAction(label) && !NavLabels.Contains(label.Text);
    }

    private static bool IsStatusChip(ReviewLabel label)
    {
        return label.Fill is "8A5A00" or "B42318" or "0C7A62"
            && label.FontSize <= 13
            && label.Text.Length <= 28;
    }

    private static bool IsAction(ReviewLabel label)
    {
        if (label.FontSize < 14)
        {
            return false;
        }

        // Page titles are large dark-on-paper copy, never action chrome.
        if (label.FontSize >= 18 && label.Fill is "1A2433" or "394656")
        {
            return false;
        }

        if (label.Back == "173E66" && label.Fill is "FFFFFF" or "D5E4F2")
        {
            return true;
        }

        return label.Text is "Open bundle" or "Quit" or "Open OTS provenance" or "Save report"
            or "Continue to escrow" or "Fail-closed history" or "View full report" or "Return to bundle"
            or "Export fail report" or "Attach CourtRelease" or "Request custodian approvals" or "Cancel"
            or "Play" or "Pause" or "Verification report" or "Build pack"
            or "Close ViewerSession after export";
    }

    private static bool IsFieldValue(ReviewLabel label)
    {
        if (label.FontSize < 13 || NavLabels.Contains(label.Text) || BadgeLabels.Contains(label.Text))
        {
            return false;
        }

        return label.Text.StartsWith("CASE-", System.StringComparison.Ordinal)
            || label.Text.StartsWith("RB-", System.StringComparison.Ordinal)
            || label.Text.StartsWith("SUBPOENA-", System.StringComparison.Ordinal)
            || label.Text.StartsWith("OTS_", System.StringComparison.Ordinal)
            || label.Text.StartsWith("key:", System.StringComparison.Ordinal)
            || label.Text.StartsWith("device:", System.StringComparison.Ordinal)
            || label.Text.StartsWith("pi:", System.StringComparison.Ordinal)
            || label.Text.Contains('\\')
            || label.Text is "Counsel" or "Firm / Court" or "72 hours"
            or "Driver coordinator" or "Passenger compositor";
    }

    private static string BadgeBack(ReviewLabel label)
    {
        return label.Fill switch
        {
            "0C7A62" => "E6F6F1",
            "8A5A00" => "FFF4D8",
            "B42318" => "FEE4E2",
            _ => label.Back,
        };
    }

    private static TextBlock MakeText(ReviewLabel label, string back, double maxWidth = 336) =>
        new()
        {
            Text = label.Text,
            FontSize = label.FontSize,
            Foreground = Brush(label.Fill),
            Background = Brush(back),
            FontWeight = Weight(label.Weight),
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.None,
            HorizontalAlignment = HorizontalAlignment.Left,
            MaxWidth = maxWidth,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private static FontWeight Weight(int weight)
    {
        if (weight >= 700)
        {
            return FontWeight.Bold;
        }

        if (weight >= 600)
        {
            return FontWeight.SemiBold;
        }

        if (weight >= 500)
        {
            return FontWeight.Medium;
        }

        return FontWeight.Normal;
    }

    private static SolidColorBrush Brush(string hex)
    {
        var text = hex.StartsWith('#') ? hex : "#" + hex;
        return new SolidColorBrush(Color.Parse(text));
    }
}
