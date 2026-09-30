// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace RideAudit.Shared.Ui.Views;

public partial class AboutView : UserControl
{
    public AboutView()
        : this(null)
    {
    }

    public AboutView(Action? close)
    {
        InitializeComponent();
        CopyrightText.Text = UiLicense.Notice;
        AttributionScope.Text = UiLicense.AttributionScope;
        var index = 0;
        foreach (var credit in UiLicense.Credits)
        {
            var body = new StackPanel { Spacing = 2 };
            body.Children.Add(Wrapped("AttributionName" + index, credit.Name + " " + credit.Version, FontWeight.SemiBold));
            body.Children.Add(Wrapped("AttributionLicense" + index, "License: " + credit.License, FontWeight.Normal));
            body.Children.Add(Wrapped("AttributionCredit" + index, credit.Credit, FontWeight.Normal));
            AttributionList.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
                BorderBrush = new SolidColorBrush(Color.Parse("#D5DEE8")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Child = body,
            });
            index++;
        }

        AboutBackButton.Click += (_, _) => close?.Invoke();
    }

    public void SetEdgeProbe(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            EdgeProbeText.IsVisible = false;
            return;
        }

        EdgeProbeText.Text = line;
        EdgeProbeText.IsVisible = true;
    }

    private static TextBlock Wrapped(string name, string text, FontWeight weight) =>
        new()
        {
            Name = name,
            Text = text,
            FontWeight = weight,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.None,
        };
}
