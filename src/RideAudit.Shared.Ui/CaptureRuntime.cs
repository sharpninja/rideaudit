// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Bt;
using RideAudit.Capture;
using RideAudit.Shared.Ui.Views;

namespace RideAudit.Shared.Ui;

/// <summary>
/// Production capture entry. The Android APK must install this before the shell
/// is constructed. A missing runtime is fail-closed, not a silent UnavailableDiscoveryBus.
/// </summary>
public sealed class CaptureRuntime
{
    public required ProductionCaptureGraph Graph { get; init; }

    public CaptureShellView CreateShell()
    {
        var view = new CaptureShellView(Graph.Discovery, productionEntry: true);
        if (!Graph.ProductionReady)
        {
            view.ShowFailClosed(
                "PRODUCTION_UNAVAILABLE: " + string.Join(" | ", Graph.UnavailableSeams));
        }

        return view;
    }
}
