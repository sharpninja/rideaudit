// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text.Json;
using SharpNinja.AiUnit.Frontier;
using SharpNinja.AiUnit.Xunit;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed record PerceptualResult(string Status, string Detail);

public static class CodexVisualGate
{
    public static void RequireCodexSubscriptionProfile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.aiunit.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var active = document.RootElement.GetProperty("AiUnit").GetProperty("ActiveStrategy").GetString();
        if (!string.Equals(active, "codex-subscription", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("aiUnit ActiveStrategy is '" + active + "'. codex-subscription is required.");
        }
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
            "You compare a RideAudit wireframe baseline with a device screenshot. Return only JSON with keys screenId, match, usabilityDefects, summary. match is false when text is clipped, controls overlap, or the screen is not the named wireframe. A pixel-similar image with a usability defect is not a match.",
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
            var match = json.RootElement.TryGetProperty("match", out var matchNode) && matchNode.ValueKind == JsonValueKind.True;
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

            if (!match || defects.Count > 0)
            {
                return new PerceptualResult("fail-closed", "match=" + match + " defects=" + string.Join("; ", defects));
            }

            return new PerceptualResult("pass", "codex-subscription reported match with no usability defects.");
        }
        catch (JsonException ex)
        {
            return new PerceptualResult("fail-closed", "Response was not the required JSON: " + ex.Message);
        }
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
