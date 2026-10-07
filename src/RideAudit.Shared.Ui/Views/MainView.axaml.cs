// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RideAudit.Shared.Ui.Views;

public partial class MainView : UserControl
{
    private object? _shellContent;
    private ShellMode _mode = ShellMode.Review;

    public MainView()
    {
        InitializeComponent();
        FrameworkNotice.Text = UiLicense.Framework;
        AboutButton.Click += OnAbout;
    }

    public void ApplyMode(ShellMode mode)
    {
        _mode = mode;
        if (mode != ShellMode.Capture)
        {
            _shellContent = new ReviewShellView();
            Body.Content = _shellContent;
            return;
        }

        _shellContent = App.CaptureRuntime?.CreateShell()
            ?? CaptureShellView.CreateUncomposedRefuse();
        Body.Content = _shellContent;
    }

    public void OpenAbout() => OnAbout(null, new RoutedEventArgs());

    private void OnAbout(object? sender, RoutedEventArgs e)
    {
        if (Body.Content is AboutView)
        {
            return;
        }

        if (Body.Content is not null && Body.Content is not AboutView)
        {
            _shellContent = Body.Content;
        }

        var about = new AboutView();
        about.BackRequested += (_, _) => Body.Content = _shellContent ?? CreateShellForMode();
        Body.Content = about;
    }

    private Control CreateShellForMode()
    {
        if (_mode != ShellMode.Capture)
        {
            return new ReviewShellView();
        }

        return App.CaptureRuntime?.CreateShell()
            ?? CaptureShellView.CreateUncomposedRefuse();
    }
}
