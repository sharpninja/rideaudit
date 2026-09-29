// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System;
using Avalonia.Controls;

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
        AttributionText.Text = UiLicense.Attributions;
        AboutBackButton.Click += (_, _) => close?.Invoke();
    }
}
