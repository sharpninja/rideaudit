// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Shared.Ui;

public static class UiLicense
{
    public const string Notice =
        "RideAudit UI. Copyright (C) 2026 RideAudit contributors. Licensed GPL-2.0-or-later. " +
        "In-scope RideAudit UI code is not relicensed MIT or Apache-2.0.";

    public const string Framework = "Avalonia UI 12";

    /// <summary>
    /// Third-party attributions (licenses and credits) for the About view (FR-RIDE-074 / TR-RIDE-VIEW-007).
    /// Sourced from repository NOTICE; Avalonia framework label alone is not this copyright notice.
    /// </summary>
    public const string Attributions =
        "Third-party attributions (licenses and credits):\n" +
        "Avalonia UI framework — licensed by its authors under the MIT license. " +
        "In-scope RideAudit application UI code is GPL-2.0-or-later and is not relicensed as MIT or Apache-2.0.";
}
