// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ShapePath = Avalonia.Controls.Shapes.Path;

namespace RideAudit.Shared.Ui.Controls;

/// <summary>
/// 24-unit stroke icon from docs/ux/assets/icons, drawn as Path nodes.
/// Layout size stays under 24px so a square slot check does not treat the stroke as an empty icon.
/// </summary>
public class WireframeIcon : Canvas
{
    public static readonly StyledProperty<string> KindProperty =
        AvaloniaProperty.Register<WireframeIcon, string>(nameof(Kind), "check");

    public static readonly StyledProperty<IBrush?> MarkBrushProperty =
        AvaloniaProperty.Register<WireframeIcon, IBrush?>(nameof(MarkBrush));

    public WireframeIcon()
    {
        Width = 22;
        Height = 22;
        IsHitTestVisible = false;
        KindProperty.Changed.AddClassHandler<WireframeIcon>((icon, _) => icon.Rebuild());
        MarkBrushProperty.Changed.AddClassHandler<WireframeIcon>((icon, _) => icon.Rebuild());
        WidthProperty.Changed.AddClassHandler<WireframeIcon>((icon, _) => icon.Rebuild());
        HeightProperty.Changed.AddClassHandler<WireframeIcon>((icon, _) => icon.Rebuild());
    }

    public string Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public IBrush? MarkBrush
    {
        get => GetValue(MarkBrushProperty);
        set => SetValue(MarkBrushProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Rebuild();
    }

    private void Rebuild()
    {
        Children.Clear();
        var brush = MarkBrush ?? new SolidColorBrush(Color.Parse("#1A2433"));
        var art = new Canvas
        {
            Width = 24,
            Height = 24,
            IsHitTestVisible = false,
        };
        foreach (var mark in Marks(Kind))
        {
            art.Children.Add(new ShapePath
            {
                Data = StreamGeometry.Parse(mark.Data),
                Stroke = mark.Filled ? null : brush,
                Fill = mark.Filled ? brush : null,
                StrokeThickness = mark.Filled ? 0 : 1.75,
                StrokeLineCap = PenLineCap.Round,
                StrokeJoin = PenLineJoin.Round,
                IsHitTestVisible = false,
            });
        }

        var boxWidth = double.IsNaN(Width) || Width <= 0 ? 22 : Width;
        var boxHeight = double.IsNaN(Height) || Height <= 0 ? 22 : Height;
        Children.Add(new Viewbox
        {
            Width = boxWidth,
            Height = boxHeight,
            Stretch = Stretch.Uniform,
            Child = art,
            IsHitTestVisible = false,
        });
    }

    private readonly record struct Mark(string Data, bool Filled);

    private static IEnumerable<Mark> Marks(string kind)
    {
        return kind switch
        {
            "shield-check" =>
            [
                new("M12 2.8 20.2 6.1v6.2c0 4.3-3.2 7.3-8.2 8.8-5-1.5-8.2-4.5-8.2-8.8V6.1Z", false),
                new("M8.5 12.1 11.1 14.7 15.8 9.6", false),
            ],
            "steering" =>
            [
                new("M4 12A8 8 0 1 1 20 12A8 8 0 1 1 4 12Z", false),
                new("M9.8 12A2.2 2.2 0 1 1 14.2 12A2.2 2.2 0 1 1 9.8 12Z", false),
                new("M12 4v5.8", false),
                new("M5.1 15.5 9.5 13", false),
                new("M18.9 15.5 14.5 13", false),
            ],
            "video" =>
            [
                new("M4 6.8H12.8A1.8 1.8 0 0 1 14.6 8.6V15.4A1.8 1.8 0 0 1 12.8 17.2H4A1.8 1.8 0 0 1 2.2 15.4V8.6A1.8 1.8 0 0 1 4 6.8Z", false),
                new("M15.2 10.2 21.4 7.2v9.6l-6.2-2.8z", true),
            ],
            "car" =>
            [
                new("M3.6 15.6h16.8", false),
                new("M6.1 15.6 7.5 10.4A2 2 0 0 1 9.4 8.9h5.2a2 2 0 0 1 1.9 1.5l1.4 5.2", false),
                new("M9.2 9.1 10.1 12.2h3.8l.9-3.1", false),
                new("M6.05 16.8A1.45 1.45 0 1 1 8.95 16.8A1.45 1.45 0 1 1 6.05 16.8Z", true),
                new("M15.05 16.8A1.45 1.45 0 1 1 17.95 16.8A1.45 1.45 0 1 1 15.05 16.8Z", true),
            ],
            "chevron-right" => [new("M9 5.4 15.6 12 9 18.6", false)],
            "chevron-left" => [new("M15 5.4 8.4 12 15 18.6", false)],
            "chevron-down" => [new("M5.6 9 12 15.4 18.4 9", false)],
            "bluetooth" => [new("M8 7.2 16.2 16.6 12 20.6V3.4l4.2 4.2L8 16.6", false)],
            "radio" => [new("M4 12A8 8 0 1 1 20 12A8 8 0 1 1 4 12Z", false)],
            "radio-on" =>
            [
                new("M4 12A8 8 0 1 1 20 12A8 8 0 1 1 4 12Z", false),
                new("M7.8 12A4.2 4.2 0 1 1 16.2 12A4.2 4.2 0 1 1 7.8 12Z", true),
            ],
            "check" => [new("M4.8 12.5 9.5 17.1 19.3 7.2", false)],
            "scale" =>
            [
                new("M12 3v17.2", false),
                new("M7.8 20.6h8.4", false),
                new("M4.2 7.2h15.6", false),
                new("M4.2 7.2 2.6 13.2", false),
                new("M4.2 7.2 5.8 13.2", false),
                new("M2.6 13.2h3.2", false),
                new("M2.6 13.2Q4.2 16.4 5.8 13.2", false),
                new("M19.8 7.2 18.2 13.2", false),
                new("M19.8 7.2 21.4 13.2", false),
                new("M18.2 13.2h3.2", false),
                new("M18.2 13.2Q19.8 16.4 21.4 13.2", false),
            ],
            "phone" =>
            [
                new("M9.2 2.6H14.8A2.2 2.2 0 0 1 17 4.8V19.2A2.2 2.2 0 0 1 14.8 21.4H9.2A2.2 2.2 0 0 1 7 19.2V4.8A2.2 2.2 0 0 1 9.2 2.6Z", false),
                new("M11 18.5h2", false),
            ],
            "refresh" =>
            [
                new("M19.4 12A7.4 7.4 0 1 1 16.2 6", false),
                new("M19.4 3.8V7.6H15.6", false),
            ],
            "link" =>
            [
                new("M10.2 13.8 8.3 15.7a3.3 3.3 0 0 1-4.7-4.7l1.9-1.9", false),
                new("M13.8 10.2 15.7 8.3a3.3 3.3 0 0 1 4.7 4.7l-1.9 1.9", false),
                new("M8.8 15.2 15.2 8.8", false),
            ],
            "clock" =>
            [
                new("M4 12A8 8 0 1 1 20 12A8 8 0 1 1 4 12Z", false),
                new("M12 7.5V12l3.3 2.2", false),
            ],
            "alert" =>
            [
                new("M12 3.4 21.6 20.4H2.4Z", false),
                new("M12 9.2v4.6", false),
                new("M11.1 16.8A0.9 0.9 0 1 1 12.9 16.8A0.9 0.9 0 1 1 11.1 16.8Z", true),
            ],
            "lock" =>
            [
                new("M8 11V8.1a4 4 0 0 1 8 0V11", false),
                new("M7.2 11H16.8A1.8 1.8 0 0 1 18.6 12.8V17.8A1.8 1.8 0 0 1 16.8 19.6H7.2A1.8 1.8 0 0 1 5.4 17.8V12.8A1.8 1.8 0 0 1 7.2 11Z", false),
            ],
            "stamp" =>
            [
                new("M6.8 9.6A5.2 5.2 0 1 1 17.2 9.6A5.2 5.2 0 1 1 6.8 9.6Z", false),
                new("M9.2 14.2 8 20.6 12 18.4 16 20.6 14.8 14.2", false),
                new("M9.5 9.8 11.3 11.6 14.8 8.2", false),
            ],
            "play" => [new("M8 5.1v13.8L19.4 12Z", true)],
            "stop" => [new("M8.2 6.6H15.8A1.6 1.6 0 0 1 17.4 8.2V15.8A1.6 1.6 0 0 1 15.8 17.4H8.2A1.6 1.6 0 0 1 6.6 15.8V8.2A1.6 1.6 0 0 1 8.2 6.6Z", true)],
            "ban" =>
            [
                new("M4 12A8 8 0 1 1 20 12A8 8 0 1 1 4 12Z", false),
                new("M6.8 6.8 17.2 17.2", false),
            ],
            "activity" => [new("M2.4 12h4.4l2.6-6 4.8 12 2.6-6H21.6", false)],
            "gauge" =>
            [
                new("M4.2 16.2A8.2 8.2 0 0 0 19.8 16.2", false),
                new("M12 16.2 16.6 9.2", false),
                new("M10.65 16.2A1.35 1.35 0 1 1 13.35 16.2A1.35 1.35 0 1 1 10.65 16.2Z", true),
            ],
            "camera" =>
            [
                new("M3.2 8.6h3.4l1.3-2.1h8.2l1.3 2.1h3.4A1.4 1.4 0 0 1 22.2 10v8.2a1.4 1.4 0 0 1-1.4 1.4H3.2A1.4 1.4 0 0 1 1.8 18.2V10a1.4 1.4 0 0 1 1.4-1.4z", false),
                new("M9.3 13.8A2.7 2.7 0 1 1 14.7 13.8A2.7 2.7 0 1 1 9.3 13.8Z", false),
            ],
            "info" =>
            [
                new("M4 12A8 8 0 1 1 20 12A8 8 0 1 1 4 12Z", false),
                new("M12 11v6", false),
                new("M11.05 7.8A0.95 0.95 0 1 1 12.95 7.8A0.95 0.95 0 1 1 11.05 7.8Z", true),
            ],
            "signal" =>
            [
                new("M2.2 15h3.3v5h-3.3Z", true),
                new("M7.7 11h3.3v9h-3.3Z", true),
                new("M13.2 7h3.3v13h-3.3Z", true),
                new("M18.7 3h3.3v17h-3.3Z", true),
            ],
            _ => [new("M4.8 12.5 9.5 17.1 19.3 7.2", false)],
        };
    }
}
