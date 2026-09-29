// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;

namespace RideAudit.Shared.Ui.Views;

public partial class MainView : UserControl
{
    private Control? _shell;

    public MainView()
    {
        InitializeComponent();
        AboutButton.Click += (_, _) => OpenAbout();
    }

    public void ApplyMode(ShellMode mode)
    {
        if (mode != ShellMode.Capture)
        {
            _shell = new ReviewShellView();
        }
        else
        {
            _shell = App.CaptureRuntime?.CreateShell()
                ?? CaptureShellView.CreateUncomposedRefuse();
        }

        ShowShell();
    }

    public void OpenAbout()
    {
        Body.Content = new AboutView(ShowShell);
    }

    public void ShowShell()
    {
        Body.Content = _shell;
    }
}
