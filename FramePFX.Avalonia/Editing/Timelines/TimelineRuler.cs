// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.UI;

namespace FramePFX.Avalonia.Editing.Timelines;

public class TimelineRuler : Control {
    private const double MinRender = 0.01D;
    private const double MajorLineThickness = 1.0;
    private const double MinorStepRatio = 0.5;
    public static readonly StyledProperty<TimelineControl?> TimelineControlProperty = AvaloniaProperty.Register<TimelineRuler, TimelineControl?>(nameof(TimelineControl));
    public static readonly StyledProperty<IBrush?> BackgroundProperty = Panel.BackgroundProperty.AddOwner<TimelineControl>();
    public static readonly AttachedProperty<FontFamily> FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner<TimelineControl>();
    public static readonly AttachedProperty<IBrush?> ForegroundProperty = TextElement.ForegroundProperty.AddOwner<TimelineControl>();
    public static readonly StyledProperty<IBrush?> MajorStepColourProperty = AvaloniaProperty.Register<TimelineRuler, IBrush?>(nameof(MajorStepColour), Brushes.Gray);
    public static readonly StyledProperty<IBrush?> MinorStepColourProperty = AvaloniaProperty.Register<TimelineRuler, IBrush?>(nameof(MinorStepColour), Brushes.DimGray);
    public static readonly StyledProperty<ScrollViewer?> ScrollViewerReferenceProperty = AvaloniaProperty.Register<TimelineRuler, ScrollViewer?>(nameof(ScrollViewerReference));

    // public static readonly DependencyProperty TimelineProperty = DependencyProperty.Register("Timeline", typeof(Timeline), typeof(TimelineRuler), new PropertyMetadata(null, (d, e) => ((TimelineRuler) d).OnTimelineChanged((Timeline) e.OldValue, (Timeline) e.NewValue)));
    // public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(TimelineRuler), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.None));
    // public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(TimelineRuler), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, (d, e) => ((TimelineRuler) d).CachedTypeFace = null));
    // public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(TimelineRuler), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush));
    // public static readonly DependencyProperty StepColorProperty = DependencyProperty.Register(nameof(StepColor), typeof(Brush), typeof(TimelineRuler), new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((TimelineRuler) d).majorLineStepColourPen = null));

    public TimelineControl? TimelineControl {
        get => this.GetValue(TimelineControlProperty);
        set => this.SetValue(TimelineControlProperty, value);
    }

    public IBrush? Background {
        get => this.GetValue(BackgroundProperty);
        set => this.SetValue(BackgroundProperty, value);
    }

    public FontFamily FontFamily {
        get => this.GetValue(FontFamilyProperty);
        set => this.SetValue(FontFamilyProperty, value);
    }

    public IBrush? Foreground {
        get => this.GetValue(ForegroundProperty);
        set => this.SetValue(ForegroundProperty, value);
    }

    public IBrush? MajorStepColour {
        get => this.GetValue(MajorStepColourProperty);
        set => this.SetValue(MajorStepColourProperty, value);
    }

    public IBrush? MinorStepColour {
        get => this.GetValue(MinorStepColourProperty);
        set => this.SetValue(MinorStepColourProperty, value);
    }

    public ScrollViewer? ScrollViewerReference {
        get => this.GetValue(ScrollViewerReferenceProperty);
        set => this.SetValue(ScrollViewerReferenceProperty, value);
    }

    private Pen MajorStepColourPen => this.majorLineStepColourPen ??= new Pen(this.MajorStepColour, 1.0);
    private Pen MinorStepColourPen => this.minorLineStepColourPen ??= new Pen(this.MinorStepColour, 1.0);

    private Typeface? CachedTypeFace;
    private Pen? majorLineStepColourPen;
    private Pen? minorLineStepColourPen;
    private Timeline? targetTimelineModel;
    private double timelineZoom;

    public TimelineRuler() {
        this.ClipToBounds = true;
    }

    static TimelineRuler() {
        TimelineControlProperty.Changed.AddClassHandler<TimelineRuler, TimelineControl?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        MajorStepColourProperty.Changed.AddClassHandler<TimelineRuler, IBrush?>((d, e) => d.majorLineStepColourPen = null);
        MinorStepColourProperty.Changed.AddClassHandler<TimelineRuler, IBrush?>((d, e) => d.minorLineStepColourPen = null);
        FontFamilyProperty.Changed.AddClassHandler<TimelineRuler, FontFamily>((d, e) => d.CachedTypeFace = null);
        AffectsRender<TimelineRuler>(MajorStepColourProperty, MinorStepColourProperty);
        ScrollViewerReferenceProperty.Changed.AddClassHandler<TimelineRuler, ScrollViewer?>((d, e) => d.OnScrollViewerReferenceChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnScrollViewerReferenceChanged(ScrollViewer? oldValue, ScrollViewer? newValue) {
        if (oldValue != null) {
            oldValue.SizeChanged -= this.OnScrollerOnSizeChanged;
            oldValue.ScrollChanged -= this.OnScrollerOnScrollChanged;
            oldValue.EffectiveViewportChanged -= this.OnEffectiveViewportChanged;
        }

        if (newValue != null) {
            newValue.SizeChanged += this.OnScrollerOnSizeChanged;
            newValue.ScrollChanged += this.OnScrollerOnScrollChanged;
            newValue.EffectiveViewportChanged += this.OnEffectiveViewportChanged;
        }
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e) {
        this.InvalidateVisual();
    }

    private void OnTimelineChanged(TimelineControl? oldTimeline, TimelineControl? newTimeline) {
        this.InvalidateVisual();

        if (oldTimeline != null) {
            oldTimeline.TimelineModelChanged -= this.OnTimelineModelChanged;
        }

        if (newTimeline != null) {
            newTimeline.TimelineModelChanged += this.OnTimelineModelChanged;
            if (newTimeline.Timeline is Timeline timeline) {
                this.OnTimelineModelChanged(newTimeline, null, timeline);
            }

            this.timelineZoom = newTimeline.Zoom;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Dispatcher.UIThread.InvokeAsync(this.InvalidateVisual, DispatcherPriority.Background);
    }

    private void OnTimelineModelChanged(ITimelineElement element, Timeline? oldtimeline, Timeline? newtimeline) {
        this.targetTimelineModel = newtimeline;
        this.InvalidateVisual();
    }

    private void OnScrollerOnSizeChanged(object? o, SizeChangedEventArgs e) => this.InvalidateVisual();

    private void OnScrollerOnScrollChanged(object? o, ScrollChangedEventArgs e) {
        if (e.OffsetDelta.X != 0 || e.OffsetDelta.Y != 0) {
            this.InvalidateVisual();
        }
    }

    public override void Render(DrawingContext dc) {
        base.Render(dc);
        if (this.targetTimelineModel == null || !(this.TimelineControl is TimelineControl timelineControl) || !(this.ScrollViewerReference is ScrollViewer scrollViewer)) {
            return;
        }

        Rect myBounds = this.Bounds;
        if (myBounds.Width < MinRender || myBounds.Height < MinRender) {
            return;
        }

        if (this.Background is Brush bg) {
            dc.DrawRectangle(bg, null, myBounds);
        }

        // Not using anymore but this is some witchcraft math
        // double start = zoom - (scrollH - (long) (scrollH / zoom) * zoom);
        // double firstMajor = scrollH % zoom == 0D ? scrollH : scrollH + (zoom - scrollH % zoom);
        // double firstMajorRelative = zoom - (scrollH - firstMajor + zoom);

        int[] Steps = [1, 2, 5, 10];

        double rulerWidth = myBounds.Width;
        double zoom = this.timelineZoom;
        double scrollH = Math.Round(scrollViewer.Offset.X);
        double timelineWidth = zoom * this.targetTimelineModel.MaxDuration;

        const int SubStepNumber = 10;
        const int MinPixelSize = 5;
        double minPixel = MinPixelSize * SubStepNumber / timelineWidth;
        double minStep = minPixel * this.targetTimelineModel.MaxDuration;
        double minStepMagPow = Math.Pow(10, Math.Floor(Math.Log10(minStep)));
        double normMinStep = minStep / minStepMagPow;
        int finalStep = Steps.FirstOrDefault(step => step > normMinStep);
        if (finalStep < 1) {
            return;
        }

        double valueStep = finalStep * minStepMagPow;
        double pixelSize = timelineWidth * valueStep / this.targetTimelineModel.MaxDuration;

        int minorSteps = Math.Min((int) Math.Floor(valueStep), SubStepNumber);
        double minorSubPixelSize = pixelSize / minorSteps;

        // These are slightly outside the rendering area both left and right, but that's fine since it's not by much
        int i = (int) Math.Floor(scrollH / pixelSize);
        int j = (int) Math.Ceiling((scrollH + rulerWidth + pixelSize) / pixelSize);
        using (dc.PushRenderOptions(new RenderOptions() { EdgeMode = EdgeMode.Aliased })) {
            do {
                double pixel = i * pixelSize - scrollH;
                if (i > j)
                    break;

                // TODO: optimise smaller/minor lines, maybe using skia?
                for (int y = 1; y < minorSteps; ++y) {
                    double subpixel = pixel + y * minorSubPixelSize;
                    this.DrawMinorLine(dc, subpixel, this.Bounds.Height);
                }

                double text_value = i * valueStep;
                if (Math.Abs(text_value - (int) text_value) < 0.00001d) {
                    this.DrawMajorLine(dc, pixel, this.Bounds.Height);
                    this.DrawText(dc, text_value, pixel);
                }

                i++;
            } while (true);
        }
    }


    public void DrawMajorLine(DrawingContext dc, double offset, double height) {
        double size = Math.Min(height / 2d, height);
        dc.DrawLine(this.MajorStepColourPen, new Point(offset, height - size), new Point(offset, height));
    }

    public void DrawMinorLine(DrawingContext dc, double offset, double height) {
        double majorSize = height / 2d;
        double size = majorSize * (1 - MinorStepRatio);
        dc.DrawLine(this.MinorStepColourPen, new Point(offset, height - size), new Point(offset, height));
    }

    public void DrawText(DrawingContext dc, double value, double offset) {
        double height = this.Bounds.Height;
        double majorSize = this.Bounds.Height / 2d;

        Point point;
        FormattedText format = this.GetFormattedText(value);
        double gap = height - majorSize;
        if (gap >= format.Height / 2d) {
            point = new Point(offset + MajorLineThickness - format.Width / 2d, gap - format.Height);
        }
        else {
            // Draw above major if possible
            point = new Point(offset + MajorLineThickness + 2d, height / 2d - format.Height / 2d);
        }

        dc.DrawText(format, point);
    }

    protected FormattedText GetFormattedText(double value) {
        string text = value.ToString();
        return new FormattedText(text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            this.CachedTypeFace ??= new Typeface(this.FontFamily),
            12,
            this.Foreground);
    }

    public void OnZoomChanged(double newZoom) {
        this.timelineZoom = newZoom;
        this.InvalidateVisual();
    }
}