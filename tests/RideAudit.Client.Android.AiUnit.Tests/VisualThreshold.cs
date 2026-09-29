// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Client.Android.AiUnit.Tests;

/// <summary>
/// SharpNinja.aiUnit 3.0.0 does not expose a pixel or perceptual numeric threshold.
/// These constants are the RideAudit harness gate. A frame fails closed when the
/// scaled baseline and the device screenshot differ by more than the ratio below.
/// Channel delta is the per-channel absolute difference that counts as a mismatch.
/// </summary>
public static class VisualThreshold
{
    public const double MaxDifferingPixelRatio = 0.08;

    public const int ChannelDelta = 24;
}
