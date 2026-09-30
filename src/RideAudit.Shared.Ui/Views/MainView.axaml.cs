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

        if (_shell is CaptureShellView capture)
        {
            capture.AboutRequested += (_, _) => OpenAbout();
        }

        ShowShell();
    }

    public void OpenAbout()
    {
        var about = new AboutView(ShowShell);
        about.SetEdgeProbe(App.CaptureRuntime?.EdgeTlsLine);
        Body.Content = about;
    }

    public void ShowShell()
    {
        Body.Content = _shell;
    }
}
