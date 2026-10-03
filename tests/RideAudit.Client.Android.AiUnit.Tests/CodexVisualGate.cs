// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text.Json;
using SharpNinja.AiUnit.Frontier;
using SharpNinja.AiUnit.Strategy;
using SharpNinja.AiUnit.Xunit;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed record PerceptualResult(string Status, string Detail);

public static class CodexVisualGate
{
    public static void RequireCodexSubscriptionProfile()
    {
        const string name = CodexSubscriptionProfile.Name;
        RejectOverride("AIUNIT_STRATEGY", name);
        RejectOverride("AIUNIT_KIND", "cli");
        RejectOverride("AIUNIT_COMMAND", "codex");
        RejectOverride("AIUNIT_MODEL", "(cli-managed)");

        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.aiunit.json");
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                "appsettings.aiunit.json is missing from the test output. "
                + "Keep tests/RideAudit.Client.Android.AiUnit.Tests/appsettings.aiunit.json and copy it to the output directory. "
                + "Set AiUnit.ActiveStrategy to codex-subscription and add Strategies.codex-subscription with Kind cli, Command codex, and Model (cli-managed).");
        }

        var config = AiUnitStrategyLoader.TryLoad(path);
        if (config is null)
        {
            throw new InvalidOperationException(
                "appsettings.aiunit.json did not parse an AiUnit section. Set ActiveStrategy to codex-subscription.");
        }

        if (!string.Equals(config.ActiveStrategy, name, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "AiUnit.ActiveStrategy is '" + config.ActiveStrategy + "'. Set it to codex-subscription. "
                + "When ActiveStrategy is empty and AIUNIT_STRATEGY is unset, the package selects claude.");
        }

        if (config.Strategies is null || !config.Strategies.ContainsKey(name))
        {
            throw new InvalidOperationException(
                "Strategies is missing the exact key codex-subscription. Add that entry. The name codex is a different strategy.");
        }

        var settings = config.Strategies[name];
        if (!string.Equals(settings.Kind, "cli", StringComparison.Ordinal)
            || !string.Equals(settings.Command, "codex", StringComparison.Ordinal)
            || !string.Equals(settings.Model, "(cli-managed)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "codex-subscription settings are Kind '" + settings.Kind + "', Command '" + settings.Command
                + "', Model '" + settings.Model + "'. The package profile is Kind cli, Command codex, Model (cli-managed).");
        }

        var (resolvedName, resolvedSettings) = AiUnitStrategyLoader.ResolveActive(config);
        if (!string.Equals(resolvedName, name, StringComparison.Ordinal) || resolvedSettings is null)
        {
            var env = Environment.GetEnvironmentVariable("AIUNIT_STRATEGY");
            throw new InvalidOperationException(
                "SharpNinja.aiUnit selected '" + resolvedName + "' (AIUNIT_STRATEGY='" + (env ?? "") + "'). "
                + "Set AIUNIT_STRATEGY to codex-subscription or unset it, and keep ActiveStrategy at codex-subscription. "
                + "Run codex login when the CLI is not authenticated. A missing client stays fail-closed.");
        }
    }

    private static void RejectOverride(string variable, string expected)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, expected, StringComparison.Ordinal))
        {
            return;
        }

        throw new InvalidOperationException(
            variable + " is '" + value + "'. Unset it, or set it to '" + expected
            + "', so the codex-subscription profile is not replaced.");
    }

    public static PerceptualResult Compare(string screenId, string actualPath, string baselinePath)
    {
        RequireCodexSubscriptionProfile();
        var fixture = AiStrategyFixture.Default;
        if (fixture.Client is null)
        {
            return new PerceptualResult(
                "fail-closed",
                "codex-subscription did not resolve a client. " + fixture.SkipReason);
        }

        var request = new FrontierRequest(
            "You judge how accurately a RideAudit device screenshot reflects the wireframe. Return only JSON with keys screenId, controls, layout, style, usabilityDefects, summary. controls, layout, and style are each the string agree or the string disagree. controls covers which controls are present, their roles, and their placement. layout covers structure, hierarchy, and spacing intent. style covers theme, typography, colors, and visual language. usabilityDefects lists cut-off or clipped text, truncated text, missing or empty icons, overlapping controls, text outside its bounds, and low-contrast labels. A numeric pixel difference is not a reason to agree or disagree.",
            "screenId: " + screenId + ". First image attachment is the wireframe baseline. Second image attachment is the device screenshot.",
            Attachments:
            [
                new FrontierAttachment("image/png", "baseline.png", File.ReadAllBytes(baselinePath)),
                new FrontierAttachment("image/png", "actual.png", File.ReadAllBytes(actualPath)),
            ],
            RequireJsonOutput: true,
            Temperature: 0);

        FrontierResponse response;
        try
        {
            CodexCliStdin.PrepareForExec();
            response = fixture.Client.SendAsync(request, CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return new PerceptualResult("fail-closed", ex.GetType().Name + ": " + ex.Message);
        }

        if (response.Error is not null)
        {
            return new PerceptualResult("fail-closed", response.Error.ErrorCode + ": " + response.Error.Message);
        }

        if (string.IsNullOrWhiteSpace(response.Text))
        {
            return new PerceptualResult("fail-closed", "codex-subscription returned an empty response.");
        }

        try
        {
            using var json = JsonDocument.Parse(ExtractJson(response.Text));
            var controls = Axis(json.RootElement, "controls");
            var layout = Axis(json.RootElement, "layout");
            var style = Axis(json.RootElement, "style");
            var defects = new List<string>();
            if (json.RootElement.TryGetProperty("usabilityDefects", out var defectNode) && defectNode.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in defectNode.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                    {
                        defects.Add(item.GetString()!);
                    }
                }
            }

            var summary = "";
            if (json.RootElement.TryGetProperty("summary", out var summaryNode) && summaryNode.ValueKind == JsonValueKind.String)
            {
                summary = summaryNode.GetString() ?? "";
                if (summary.Length > 400)
                {
                    summary = summary.Substring(0, 400);
                }
            }

            var axes = "controls=" + (controls ?? "(missing)")
                + " layout=" + (layout ?? "(missing)")
                + " style=" + (style ?? "(missing)")
                + " defects=" + string.Join("; ", defects)
                + " summary=" + summary;
            if (controls is null || layout is null || style is null)
            {
                return new PerceptualResult("fail-closed", "Response omitted controls, layout, or style. " + axes);
            }

            if (controls != "agree" || layout != "agree" || style != "agree" || defects.Count > 0)
            {
                return new PerceptualResult("fail-closed", axes);
            }

            return new PerceptualResult("pass", "codex-subscription agreed on controls, layout, and style with no usability defects.");
        }
        catch (JsonException ex)
        {
            return new PerceptualResult("fail-closed", "Response was not the required JSON: " + ex.Message);
        }
    }

    private static string? Axis(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var node) || node.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = node.GetString();
        return value is "agree" or "disagree" ? value : null;
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return text;
        }

        return text.Substring(start, end - start + 1);
    }
}
