// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RideAudit.Shared.Ui.Views;

public partial class AboutView : UserControl
{
    public event EventHandler? BackRequested;

    public AboutView()
    {
        InitializeComponent();
        CopyrightNotice.Text = UiLicense.Notice;
        AttributionsNotice.Text = UiLicense.Attributions;
        BackButton.Click += OnBack;
    }

    private void OnBack(object? sender, RoutedEventArgs e) => BackRequested?.Invoke(this, EventArgs.Empty);
}
