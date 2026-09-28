// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using RideAudit.Shared.Ui;

namespace RideAudit.Client.Android;

[Application]
public class Application : AvaloniaAndroidApplication<App>
{
    protected Application(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
        App.Mode = ShellMode.Capture;
        AndroidProductionComposition.Install(this);
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
