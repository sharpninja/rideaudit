// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using RideAudit.Shared.Ui.Controls;

[assembly: InternalsVisibleTo("RideAudit.Client.Tests")]

namespace RideAudit.Shared.Ui.Views;

/// <summary>
/// Hosts WF-R labels with phone chrome that mirrors the approved desktop SVG:
/// single-row nav (no wrap), left sidebar rail for dark-panel copy/icons,
/// right form column for fields/actions. Catalog text/fill/font stay CLS-true.
/// Row Y order follows the wireframe so local layout reflection stays green.
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

    private static readonly HashSet<string> PrimaryActions = new(System.StringComparer.Ordinal)
    {
        "Open bundle", "Quit", "Run verification", "Open OTS provenance", "Save report",
        "Continue to escrow", "Fail-closed history", "View full report", "Return to bundle",
        "Export fail report", "Attach CourtRelease", "Request custodian approvals", "Cancel",
        "Play", "Pause", "Verification report", "Build pack",
        "Close ViewerSession after export",
    };

    private readonly Dictionary<string, Control> _pages = new(System.StringComparer.Ordinal);

    public CaptureReviewHost()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        foreach (var screen in CaptureReviewCatalog.Labels.Select(label => label.Screen).Distinct())
        {
            var page = BuildPage(screen);
            page.IsVisible = false;
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

    private static Control BuildPage(string screen)
    {
        var root = new StackPanel
        {
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 360,
        };

        root.Children.Add(new WireframeIcon
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

        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("148,*"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 6,
        };

        var rowIndex = 0;
        var hasSidebar = false;
        foreach (var row in GroupRows(labels))
        {
            // Only the top chrome strip is nav. Later "Verify"/"Escrow"/… actions share
            // names but are form controls (larger type, body Y).
            if (row.All(label => NavLabels.Contains(label.Text) && label.FontSize <= 13 && label.Y < 120))
            {
                root.Children.Add(BuildNav(row));
                continue;
            }

            body.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var left = row.Where(IsSidebarLabel).ToList();
            var right = row.Where(label => !IsSidebarLabel(label)).ToList();

            if (left.Count > 0)
            {
                hasSidebar = true;
                var leftStack = new StackPanel { Spacing = 4 };
                foreach (var label in left)
                {
                    leftStack.Children.Add(BuildSidebarChrome(label));
                }

                var leftRail = new Border
                {
                    Background = Brush("173E66"),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 6, 8, 6),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Child = leftStack,
                };
                Grid.SetColumn(leftRail, 0);
                Grid.SetRow(leftRail, rowIndex);
                body.Children.Add(leftRail);
            }

            if (right.Count > 0)
            {
                var rightControl = right.Count == 1 ? BuildChrome(right[0]) : BuildFormCluster(right);
                Grid.SetColumn(rightControl, 1);
                Grid.SetRow(rightControl, rowIndex);
                body.Children.Add(rightControl);
            }

            rowIndex++;
        }

        if (!hasSidebar)
        {
            body.ColumnDefinitions = new ColumnDefinitions("*");
            foreach (var child in body.Children.OfType<Control>())
            {
                Grid.SetColumn(child, 0);
            }
        }

        root.Children.Add(body);
        return root;
    }

    private static Control BuildFormCluster(IReadOnlyList<ReviewLabel> row)
    {
        if (row.Count > 1 && row.All(label => BadgeLabels.Contains(label.Text) || label.Text.Length <= 10))
        {
            var wrap = new WrapPanel { ItemSpacing = 8, LineSpacing = 6 };
            foreach (var label in row)
            {
                wrap.Children.Add(BuildChrome(label));
            }

            return wrap;
        }

        var vertical = row.Any(label => label.Text.Length > 18 || IsFieldValue(label) || IsAction(label));
        var panel = new StackPanel
        {
            Spacing = 4,
            Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal,
        };
        foreach (var label in row)
        {
            panel.Children.Add(BuildChrome(label));
        }

        return panel;
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

    private static Control BuildNav(IReadOnlyList<ReviewLabel> row)
    {
        // Compact single-row StackPanel — no ScrollViewer (PART_* overlaps File/Help).
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
            Padding = new Thickness(1, 2, 1, 2),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = panel,
        };
    }

    private static Control BuildSidebarChrome(ReviewLabel label)
    {
        var iconKind = SidebarIconKind(label.Text);
        var text = MakeText(label, label.Back, 128);
        if (iconKind is null)
        {
            return text;
        }

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        row.Children.Add(new WireframeIcon
        {
            Kind = iconKind,
            Width = 18,
            Height = 18,
            MarkBrush = Brush(label.Fill is "FFFFFF" or "D5E4F2" or "9FC0E0" ? label.Fill : "D5E4F2"),
            VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(text);
        return row;
    }

    private static string? SidebarIconKind(string text)
    {
        if (text is "Independent checks" or "COURT REVIEW" or "RideAudit")
        {
            return "shield-check";
        }

        if (text.Contains("OpenTimestamps", System.StringComparison.Ordinal)
            || text.Contains("OTS", System.StringComparison.Ordinal))
        {
            return "check";
        }

        if (text.Contains("gRPC", System.StringComparison.Ordinal))
        {
            return "video";
        }

        if (text.StartsWith("Open does not", System.StringComparison.Ordinal))
        {
            return "shield-check";
        }

        return null;
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
            return new Border
            {
                Background = Brush(label.Back),
                BorderBrush = Brush(label.Back == "F4F7FA" ? "C5D0DC" : "0F2C4A"),
                BorderThickness = new Thickness(1.25),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(14, 10, 14, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                MinHeight = 40,
                Child = MakeText(label, label.Back),
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
                Child = MakeText(label, "FFFFFF"),
            };
        }

        if (label.Back is "FFF4D8"
            || (label.Back != "F4F7FA" && label.Back != "FFFFFF" && label.Back != "173E66"))
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

        return MakeText(label, label.Back);
    }

    private static bool IsSidebarLabel(ReviewLabel label)
    {
        if (NavLabels.Contains(label.Text) || IsAction(label) || BadgeLabels.Contains(label.Text))
        {
            return false;
        }

        return label.Back == "173E66";
    }

    private static bool IsStatusChip(ReviewLabel label) =>
        label.Fill is "8A5A00" or "B42318" or "0C7A62"
        && label.FontSize <= 13
        && label.Text.Length <= 28;

    private static bool IsAction(ReviewLabel label)
    {
        if (label.FontSize < 14)
        {
            return false;
        }

        if (label.FontSize >= 18 && label.Fill is "1A2433" or "394656")
        {
            return false;
        }

        return PrimaryActions.Contains(label.Text);
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

    private static string BadgeBack(ReviewLabel label) =>
        label.Fill switch
        {
            "0C7A62" => "E6F6F1",
            "8A5A00" => "FFF4D8",
            "B42318" => "FEE4E2",
            _ => label.Back,
        };

    private static TextBlock MakeText(ReviewLabel label, string back, double maxWidth = 220) =>
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