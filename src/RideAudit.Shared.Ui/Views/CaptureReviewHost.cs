// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using RideAudit.Shared.Ui.Controls;

[assembly: InternalsVisibleTo("RideAudit.Client.Tests")]

namespace RideAudit.Shared.Ui.Views;

public class CaptureReviewHost : Grid
{
    private readonly Dictionary<string, StackPanel> _pages = new(System.StringComparer.Ordinal);

    public CaptureReviewHost()
    {
        foreach (var screen in CaptureReviewCatalog.Labels.Select(label => label.Screen).Distinct())
        {
            var page = new StackPanel
            {
                Spacing = 4,
                IsVisible = false,
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
            foreach (var label in CaptureReviewCatalog.Labels)
            {
                if (!string.Equals(label.Screen, screen, System.StringComparison.Ordinal))
                {
                    continue;
                }

                page.Children.Add(new TextBlock
                {
                    Text = label.Text,
                    FontSize = label.FontSize,
                    Foreground = Brush(label.Fill),
                    Background = Brush(label.Back),
                    FontWeight = Weight(label.Weight),
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.None,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    MaxWidth = 360,
                });
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
