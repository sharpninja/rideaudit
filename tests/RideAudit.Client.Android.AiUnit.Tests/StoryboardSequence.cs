// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text.RegularExpressions;

namespace RideAudit.Client.Android.AiUnit.Tests;

public static class StoryboardSequence
{
    public static string Role(IReadOnlyList<StoryboardStep> steps)
    {
        foreach (var step in steps)
        {
            if (step.ScreenId.StartsWith("WF-R-", StringComparison.Ordinal))
            {
                continue;
            }

            return step.ScreenId == "WF-05" ? "passenger" : "driver";
        }

        return "driver";
    }

    public static IReadOnlyList<string>? Clicks(string currentScreen, StoryboardStep step, string role)
    {
        if (step.AlternateBranch || step.ScreenId.StartsWith("WF-R-", StringComparison.Ordinal))
        {
            return null;
        }

        if (string.Equals(currentScreen, step.ScreenId, StringComparison.Ordinal))
        {
            return SameScreenClicks(step, role);
        }

        return PathClicks(currentScreen, step.ScreenId, role);
    }

    private static IReadOnlyList<string> SameScreenClicks(StoryboardStep step, string role)
    {
        if (step.ScreenId == "WF-01" && step.Title.Contains("role", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { role == "passenger" ? "PassengerButton" : "DriverButton" };
        }

        if (Regex.IsMatch(step.Title, @"^Start\b", RegexOptions.IgnoreCase))
        {
            return new[] { "StartButton" };
        }

        if (Regex.IsMatch(step.Title, @"^Submit\b", RegexOptions.IgnoreCase))
        {
            return new[] { "SubmitButton" };
        }

        return Array.Empty<string>();
    }

    private static IReadOnlyList<string>? PathClicks(string current, string target, string role)
    {
        var roleButton = role == "passenger" ? "PassengerButton" : "DriverButton";
        if (current == "WF-01" && target == "WF-02")
        {
            return new[] { roleButton, "ContinueButton" };
        }

        if (current == "WF-01" && target == "WF-03")
        {
            return new[] { roleButton, "ContinueButton", "PeerButton" };
        }

        if (current == "WF-01" && target == "WF-04" && role == "driver")
        {
            return new[] { "DriverButton", "ContinueButton", "PeerButton", "ConfirmCheck", "ConfirmPairButton" };
        }

        if (current == "WF-01" && target == "WF-05" && role == "passenger")
        {
            return new[] { "PassengerButton", "ContinueButton", "PeerButton", "ConfirmCheck", "ConfirmPairButton" };
        }

        if (current == "WF-01" && target == "WF-06" && role == "driver")
        {
            return new[] { "DriverButton", "ContinueButton", "PeerButton", "ConfirmCheck", "ConfirmPairButton", "StartButton", "StopButton" };
        }

        if (current == "WF-01" && target == "WF-07" && role == "driver")
        {
            return new[] { "DriverButton", "ContinueButton", "PeerButton", "ConfirmCheck", "ConfirmPairButton", "StartButton", "StopButton", "SubmitButton" };
        }

        if (current == "WF-02" && target == "WF-03")
        {
            return new[] { "PeerButton" };
        }

        if (current == "WF-03" && (target == "WF-04" || target == "WF-05"))
        {
            return new[] { "ConfirmCheck", "ConfirmPairButton" };
        }

        if (current == "WF-04" && target == "WF-06")
        {
            return new[] { "StartButton", "StopButton" };
        }

        if (current == "WF-04" && target == "WF-07")
        {
            return new[] { "StartButton", "StopButton", "SubmitButton" };
        }

        if (current == "WF-06" && target == "WF-07")
        {
            return new[] { "SubmitButton" };
        }

        return null;
    }
}
