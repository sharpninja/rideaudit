// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Globalization;
using System.Text.RegularExpressions;

namespace RideAudit.Client.Android.AiUnit.Tests;

public static class SlateContrast
{
    public const string Muted = "394656";
    public const string Disabled = "3D4A5A";

    private static readonly string[] Retired = ["5C6B7C", "8B98A8", "93A0AE"];

    private static readonly Regex TextElement = new(
        "<text\\b([^>]*)>(.*?)</text>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    public static bool SameHex(string left, string right)
    {
        return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
    }

    public static double Ratio(string foreground, string background)
    {
        var hi = Math.Max(Luminance(foreground), Luminance(background));
        var lo = Math.Min(Luminance(foreground), Luminance(background));
        return (hi + 0.05) / (lo + 0.05);
    }

    public static IReadOnlyList<string> RetiredHits(string source)
    {
        return Retired.Where(hex => source.Contains("#" + hex, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public static string? FillForText(string svg, string text)
    {
        foreach (Match match in TextElement.Matches(svg))
        {
            if (!string.Equals(match.Groups[2].Value.Trim(), text, StringComparison.Ordinal))
            {
                continue;
            }

            var fill = Regex.Match(match.Groups[1].Value, "fill\\s*=\\s*\"#([0-9A-Fa-f]{6})\"", RegexOptions.IgnoreCase);
            if (fill.Success)
            {
                return fill.Groups[1].Value.ToUpperInvariant();
            }
        }

        return null;
    }

    private static string Normalize(string hex) => hex.Trim().TrimStart('#').ToUpperInvariant();

    private static double Luminance(string hex)
    {
        var raw = Normalize(hex);
        var r = Linear(int.Parse(raw.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0);
        var g = Linear(int.Parse(raw.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0);
        var b = Linear(int.Parse(raw.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0);
        return (0.2126 * r) + (0.7152 * g) + (0.0722 * b);
    }

    private static double Linear(double channel)
    {
        return channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }
}
