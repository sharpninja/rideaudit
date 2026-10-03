// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Client.Android.AiUnit.Tests;

/// <summary>
/// SharpNinja.aiUnit 3.0.0 does not expose a pixel or perceptual numeric threshold.
/// The ratio below is advisory. A high value does not fail a frame, and a low value
/// does not pass one. Channel delta is the per-channel absolute difference that
/// counts toward that advisory ratio.
/// </summary>
public static class VisualThreshold
{
    public const double MaxDifferingPixelRatio = 0.08;

    public const int ChannelDelta = 24;
}
