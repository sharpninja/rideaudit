// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Shared.Ui;

public static class UiLicense
{
    public const string Notice =
        "RideAudit UI. Copyright (C) 2026 RideAudit contributors. Licensed GPL-2.0-or-later. " +
        "In-scope RideAudit UI code is not relicensed MIT or Apache-2.0.";

    public const string AttributionScope =
        "Licenses and credits for runtime components resolved into the Android capture client. " +
        "Each license is that package nuspec expression. " +
        "Other AndroidX bindings in the Avalonia.Android closure use MIT AND Apache-2.0 and are not repeated. " +
        "SharpNinja.aiUnit is a compile reference and is not in the APK. Grpc.Tools is a build tool and is not in the APK. " +
        "In-scope RideAudit application UI code stays GPL-2.0-or-later.";

    public static readonly IReadOnlyList<ThirdPartyCredit> Credits =
    [
        new("Avalonia", "12.1.3", "MIT", "Avalonia Team. Copyright 2013-2026 The AvaloniaUI Project."),
        new("Avalonia.Android", "12.1.3", "MIT", "Avalonia Team. Copyright 2013-2026 The AvaloniaUI Project."),
        new("Avalonia.Themes.Fluent", "12.1.3", "MIT", "Avalonia Team. Copyright 2013-2026 The AvaloniaUI Project."),
        new("Avalonia.Fonts.Inter", "12.1.3", "MIT", "Avalonia Team. Package license expression MIT."),
        new("Avalonia.Skia", "12.1.3", "MIT", "Avalonia Team. Copyright 2013-2026 The AvaloniaUI Project."),
        new("Avalonia.HarfBuzz", "12.1.3", "MIT", "Avalonia Team. Copyright 2013-2026 The AvaloniaUI Project."),
        new("SkiaSharp", "3.119.4", "MIT", "Microsoft. Copyright Microsoft Corporation."),
        new("HarfBuzzSharp", "8.3.1.3", "MIT", "Microsoft. Copyright Microsoft Corporation."),
        new("Grpc.Net.Client", "2.84.0", "Apache-2.0", "The gRPC Authors. Copyright 2019 The gRPC Authors."),
        new("Grpc.Core.Api", "2.84.0", "Apache-2.0", "The gRPC Authors. Copyright 2019 The gRPC Authors."),
        new("Google.Protobuf", "3.36.1", "BSD-3-Clause", "Google Inc. Copyright 2015, Google Inc."),
        new("Xamarin.AndroidX.Core.SplashScreen", "1.0.1.15", "MIT AND Apache-2.0", "Microsoft. Bindings in the package are MIT."),
        new("Xamarin.AndroidX.AppCompat", "1.7.1.3", "MIT AND Apache-2.0", "Microsoft. Pulled in by Avalonia.Android."),
        new("SharpNinja.Avalonia.RemoteControl.Runtime", "0.7.4", "MIT", "Sharp Ninja. Debug builds only. Release omits this package."),
        new("Microsoft.Extensions.DependencyInjection", "10.0.8", "MIT", "Microsoft. Debug builds only. Release omits this package."),
    ];

    public static string Attributions =>
        AttributionScope + " " + string.Join(" ", Credits.Select(credit => credit.Line));

    public const string Framework = "Avalonia UI 12";
}

public sealed record ThirdPartyCredit(string Name, string Version, string License, string Credit)
{
    public string Line => Name + " " + Version + ". License: " + License + ". " + Credit;
}
