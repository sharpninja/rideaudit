// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;

namespace RideAudit.Shared.Ui.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        LicenseNotice.Text = UiLicense.Notice;
        FrameworkNotice.Text = UiLicense.Framework;
        ApplyMode(ShellMode.Capture);
    }

    public void ApplyMode(ShellMode mode)
    {
        Body.Content = mode == ShellMode.Capture
            ? new CaptureShellView()
            : new ReviewShellView();
    }
}
