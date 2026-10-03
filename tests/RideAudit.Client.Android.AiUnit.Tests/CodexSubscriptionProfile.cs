// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Runtime.CompilerServices;
using SharpNinja.AiUnit.Strategy;

namespace RideAudit.Client.Android.AiUnit.Tests;

internal static class CodexSubscriptionProfile
{
    internal const string Name = "codex-subscription";

    [ModuleInitializer]
    internal static void Select()
    {
        var current = Environment.GetEnvironmentVariable("AIUNIT_STRATEGY");
        if (string.IsNullOrWhiteSpace(current))
        {
            Environment.SetEnvironmentVariable("AIUNIT_STRATEGY", Name);
            return;
        }

        if (!string.Equals(current, Name, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "AIUNIT_STRATEGY is '" + current + "'. Set it to codex-subscription or unset it. "
                + "The package uses that variable before ActiveStrategy, and an empty selection falls back to claude.");
        }
    }
}
