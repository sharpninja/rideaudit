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
    }

    public void ApplyMode(ShellMode mode)
    {
        if (mode != ShellMode.Capture)
        {
            Body.Content = new ReviewShellView();
            return;
        }

        Body.Content = App.CaptureRuntime?.CreateShell()
            ?? CaptureShellView.CreateUncomposedRefuse();
    }
}
