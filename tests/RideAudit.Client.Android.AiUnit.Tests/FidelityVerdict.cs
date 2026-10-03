// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Client.Android.AiUnit.Tests;

/// <summary>
/// Primary visual verdict is controls, layout, and style, plus usability.
/// Pixel ratio is advisory and is never added to the failure list.
/// </summary>
public static class FidelityVerdict
{
    public static List<string> Failures(string frameId, IReadOnlyList<UsabilityCheck> checks)
    {
        var failures = new List<string>();
        foreach (var check in checks)
        {
            if (check.Status is not ("fail" or "fail-closed"))
            {
                continue;
            }

            if (check.Id is "controls" or "layout" or "style")
            {
                failures.Add(frameId + " " + check.Id + " " + check.Status + ": " + check.Detail + " Evidence: " + check.Evidence);
                continue;
            }

            failures.Add(frameId + " usability " + check.Id + " " + check.Status + ": " + check.Detail + " Evidence: " + check.Evidence);
        }

        return failures;
    }

    public static string AdvisoryPixel(string frameId, double ratio, string diffPath)
    {
        return frameId + " advisory pixel ratio " + ratio.ToString("0.0000")
            + " (numeric diff is not the pass or fail bar; reference "
            + VisualThreshold.MaxDifferingPixelRatio.ToString("0.00") + "). Diff: " + diffPath;
    }
}
