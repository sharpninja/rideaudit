// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(RideAudit.Client.Tests.Support.HeadlessAppBuilder))]

namespace RideAudit.Client.Tests.Support;

public static class HeadlessAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<RideAudit.Shared.Ui.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true,
            });
}
