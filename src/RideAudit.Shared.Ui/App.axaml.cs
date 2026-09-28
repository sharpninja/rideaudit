// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RideAudit.Shared.Ui.Views;

namespace RideAudit.Shared.Ui;

public partial class App : Application
{
    public static ShellMode Mode { get; set; } = ShellMode.Review;

    public static CaptureRuntime? CaptureRuntime { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var view = new MainView();
        view.ApplyMode(Mode);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(view);
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activity)
        {
            activity.MainViewFactory = () => view;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            single.MainView = view;
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public enum ShellMode
{
    Capture,
    Review,
}
