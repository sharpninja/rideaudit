// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using SkiaSharp;
using Svg.Skia;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed record PixelCompareResult(
    double DifferingPixelRatio,
    int Width,
    int Height,
    string ActualPath,
    string BaselinePath,
    string DiffPath,
    bool WithinThreshold);

public static class BaselineComparer
{
    public static PixelCompareResult Compare(byte[] actualPng, string baselineSvg, string artifactDirectory, string frameId)
    {
        Directory.CreateDirectory(artifactDirectory);
        var actualPath = Path.Combine(artifactDirectory, frameId + "-actual.png");
        var baselinePath = Path.Combine(artifactDirectory, frameId + "-baseline.png");
        var diffPath = Path.Combine(artifactDirectory, frameId + "-diff.png");
        File.WriteAllBytes(actualPath, actualPng);

        using var actual = SKBitmap.Decode(actualPng) ?? throw new InvalidOperationException("Device screenshot is not a PNG: " + actualPath);
        using var baselineNative = Rasterize(baselineSvg);
        using var baseline = Scale(baselineNative, actual.Width, actual.Height);
        using var image = SKImage.FromBitmap(baseline);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        using (var stream = File.Create(baselinePath))
        {
            data.SaveTo(stream);
        }

        var different = 0;
        var total = actual.Width * actual.Height;
        using var diff = new SKBitmap(actual.Width, actual.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var left = actual.GetPixelSpan();
        var right = baseline.GetPixelSpan();
        var output = diff.GetPixelSpan();
        for (var i = 0; i + 3 < left.Length && i + 3 < right.Length && i + 3 < output.Length; i += 4)
        {
            var mismatch = Math.Abs(left[i] - right[i]) > VisualThreshold.ChannelDelta
                || Math.Abs(left[i + 1] - right[i + 1]) > VisualThreshold.ChannelDelta
                || Math.Abs(left[i + 2] - right[i + 2]) > VisualThreshold.ChannelDelta;
            if (mismatch)
            {
                different++;
                output[i] = 220;
                output[i + 1] = 40;
                output[i + 2] = 40;
                output[i + 3] = 255;
            }
            else
            {
                var gray = (byte)((left[i] + left[i + 1] + left[i + 2]) / 3);
                output[i] = gray;
                output[i + 1] = gray;
                output[i + 2] = gray;
                output[i + 3] = 255;
            }
        }

        using var diffImage = SKImage.FromBitmap(diff);
        using var diffData = diffImage.Encode(SKEncodedImageFormat.Png, 80);
        using (var stream = File.Create(diffPath))
        {
            diffData.SaveTo(stream);
        }

        var ratio = total == 0 ? 1 : (double)different / total;
        return new PixelCompareResult(
            ratio,
            actual.Width,
            actual.Height,
            actualPath,
            baselinePath,
            diffPath,
            ratio <= VisualThreshold.MaxDifferingPixelRatio);
    }

    private static SKBitmap Rasterize(string svgPath)
    {
        if (!File.Exists(svgPath))
        {
            throw new FileNotFoundException("Wireframe baseline SVG is missing.", svgPath);
        }

        using var svg = new SKSvg();
        var picture = svg.Load(svgPath) ?? throw new InvalidOperationException("SVG did not rasterize: " + svgPath);
        var rect = picture.CullRect;
        var width = Math.Max(1, (int)Math.Ceiling(rect.Width));
        var height = Math.Max(1, (int)Math.Ceiling(rect.Height));
        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        canvas.DrawPicture(picture);
        return bitmap;
    }

    private static SKBitmap Scale(SKBitmap source, int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(source, new SKRect(0, 0, width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        return bitmap;
    }
}
