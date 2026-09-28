// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia;
using RideAudit.Shared.Ui;

namespace RideAudit.Client.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.Mode = ShellMode.Review;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
