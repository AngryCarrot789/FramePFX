// 
// Copyright (c) 2026-2026 REghZy
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
using Avalonia.Media;
using Avalonia.Threading;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Avalonia.Editor;

public class TimelineRuler : Control {
    private const double MinRender = 0.01D, MajorLineThickness = 1.0, MinorStepRatio = 0.5;
    public static readonly StyledProperty<TimelineViewState?> TimelineProperty = AvaloniaProperty.Register<TimelineRuler, TimelineViewState?>(nameof(Timeline));
    public static readonly StyledProperty<IBrush?> BackgroundProperty = Panel.BackgroundProperty.AddOwner<TimelineControl>();
    public static readonly AttachedProperty<FontFamily> FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner<TimelineControl>();
    public static readonly AttachedProperty<IBrush?> ForegroundProperty = TextElement.ForegroundProperty.AddOwner<TimelineControl>();
    public static readonly StyledProperty<IBrush?> MajorStepColourProperty = AvaloniaProperty.Register<TimelineRuler, IBrush?>(nameof(MajorStepColour), Brushes.Gray);
    public static readonly StyledProperty<IBrush?> MinorStepColourProperty = AvaloniaProperty.Register<TimelineRuler, IBrush?>(nameof(MinorStepColour), Brushes.DimGray);
    public static readonly StyledProperty<TimelineRulerMode> ModeProperty = AvaloniaProperty.Register<TimelineRuler, TimelineRulerMode>(nameof(Mode), TimelineRulerMode.FPS);
    public static readonly StyledProperty<double> ProjectFPSProperty = AvaloniaProperty.Register<TimelineRuler, double>(nameof(ProjectFPS), 60.0);

    public TimelineViewState? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
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

    public TimelineRulerMode Mode {
        get => this.GetValue(ModeProperty);
        set => this.SetValue(ModeProperty, value);
    }

    public double ProjectFPS {
        get => this.GetValue(ProjectFPSProperty);
        set => this.SetValue(ProjectFPSProperty, value);
    }

    private Pen MajorStepColourPen => this.majorLineStepColourPen ??= new Pen(this.MajorStepColour, 1.0);
    private Pen MinorStepColourPen => this.minorLineStepColourPen ??= new Pen(this.MinorStepColour, 1.0);

    private Typeface? CachedTypeFace;
    private Pen? majorLineStepColourPen, minorLineStepColourPen;

    public TimelineRuler() {
        this.ClipToBounds = true;
    }

    static TimelineRuler() {
        TimelineProperty.Changed.AddClassHandler<TimelineRuler, TimelineViewState?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        MajorStepColourProperty.Changed.AddClassHandler<TimelineRuler, IBrush?>((d, e) => d.majorLineStepColourPen = null);
        MinorStepColourProperty.Changed.AddClassHandler<TimelineRuler, IBrush?>((d, e) => d.minorLineStepColourPen = null);
        FontFamilyProperty.Changed.AddClassHandler<TimelineRuler, FontFamily>((d, e) => d.CachedTypeFace = null);
        AffectsRender<TimelineRuler>(MajorStepColourProperty, MinorStepColourProperty, ModeProperty, ProjectFPSProperty);
    }

    private void OnTimelineChanged(TimelineViewState? oldTimeline, TimelineViewState? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.ZoomChanged -= this.TimelineOnZoomChanged;
            oldTimeline.HorizontalScrollChanged -= this.TimelineOnHorizontalScrollChanged;
            oldTimeline.Timeline.MaximumDurationChanged -= this.TimelineOnMaximumDurationChanged;
        }

        if (newTimeline != null) {
            newTimeline.Timeline.MaximumDurationChanged += this.TimelineOnMaximumDurationChanged;
            newTimeline.ZoomChanged += this.TimelineOnZoomChanged;
            newTimeline.HorizontalScrollChanged += this.TimelineOnHorizontalScrollChanged;
        }

        this.InvalidateVisual();
    }

    private void TimelineOnMaximumDurationChanged(object? sender, ValueChangedEventArgs<TimeSpan> e) => this.InvalidateVisual();
    private void TimelineOnZoomChanged(object? sender, ValueChangedEventArgs<double> e) => this.InvalidateVisual();
    private void TimelineOnHorizontalScrollChanged(object? sender, ValueChangedEventArgs<TimeSpan> e) => this.InvalidateVisual();

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Dispatcher.UIThread.Post(this.InvalidateVisual, DispatcherPriority.Background);
    }

    public long CalculateMajorStepTicks(double ticksPerPixel, out int minorDivisions) {
        minorDivisions = 10;
        switch (this.Mode) {
            case TimelineRulerMode.FPS: {
                double fps = Math.Max(1.0, this.ProjectFPS);
                long frameTicks = (long) (TimeSpan.TicksPerSecond / fps);
                long minTicks = (long) (TimelineUnits.PixelsPerSecond * ticksPerPixel);
                long framesPerMajor = Math.Max(1, minTicks / frameTicks);
                minorDivisions = (int) Math.Min(framesPerMajor, 5L); // no sub-frames
                return framesPerMajor * frameTicks;
            }

            case TimelineRulerMode.AutoTimeScale:
            default: {
                int[] steps = { 1, 2, 5, 10 };
                double minStepSeconds = (TimelineUnits.PixelsPerSecond * ticksPerPixel) / TimeSpan.TicksPerSecond;
                double unitSeconds;
                if (minStepSeconds >= 3600)
                    unitSeconds = 3600;
                else if (minStepSeconds >= 60)
                    unitSeconds = 60;
                else
                    unitSeconds = 1;

                double magnitude = Math.Pow(10, Math.Floor(Math.Log10(minStepSeconds / unitSeconds)));
                double normalized = minStepSeconds / (unitSeconds * magnitude);

                int stepBase = steps.FirstOrDefault(s => s >= normalized);
                if (stepBase == 0)
                    stepBase = 10;

                double majorSeconds = stepBase * magnitude * unitSeconds;

                minorDivisions = 10;
                return (long) (majorSeconds * TimeSpan.TicksPerSecond);
            }
        }
    }


    public override void Render(DrawingContext dc) {
        base.Render(dc);
        TimelineViewState? timeline = this.Timeline;
        if (timeline == null) {
            return;
        }

        Rect bounds = this.Bounds;
        if (bounds.Width < MinRender || bounds.Height < MinRender)
            return;

        if (this.Background is Brush bg)
            dc.DrawRectangle(bg, null, bounds);

        double ticksPerPixel = TimelineUnits.GetTicksPerPixelRatio(timeline.Zoom);
        double pixelsPerTick = TimelineUnits.GetPixelsPerTickRatio(timeline.Zoom);
        long visibleStartTicks = timeline.HorizontalScroll.Ticks;
        long visibleEndTicks = visibleStartTicks + (long) (bounds.Width * ticksPerPixel);

        const int MinorDivisions = 10;
        long majorStepTicks = this.CalculateMajorStepTicks(ticksPerPixel, out int minorDivisions);
        long minorStepTicks = majorStepTicks / minorDivisions;
        long firstMajorTick = ((visibleStartTicks / majorStepTicks) * majorStepTicks) - majorStepTicks;
        using (dc.PushRenderOptions(new RenderOptions { EdgeMode = EdgeMode.Aliased })) {
            for (long t = firstMajorTick; t <= visibleEndTicks + majorStepTicks; t += majorStepTicks) {
                for (int i = 1; i < MinorDivisions; i++) {
                    long minorTick = t + i * minorStepTicks;
                    double subX = (minorTick - visibleStartTicks) * pixelsPerTick;
                    this.DrawMinorLine(dc, subX, bounds.Height);
                }

                double x = (t - visibleStartTicks) * pixelsPerTick;
                this.DrawMajorLine(dc, x, bounds.Height);
                // this.DrawText(dc, t / (double) TimeSpan.TicksPerSecond, x);
                this.DrawText(dc, this.FormatLabel(t), x);
            }
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

    public void DrawText(DrawingContext dc, string text, double offset) {
        double height = this.Bounds.Height;
        double majorSize = height / 2d;

        Point point;
        FormattedText format = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, this.CachedTypeFace ??= new Typeface(this.FontFamily), 12, this.Foreground);
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

    private string FormatLabel(long ticks) {
        if (ticks < 0)
            return string.Empty;

        switch (this.Mode) {
            case TimelineRulerMode.FPS: {
                double fps = Math.Max(1.0, this.ProjectFPS);
                long frame = (long) Math.Round(ticks / (TimeSpan.TicksPerSecond / fps));
                return frame.ToString();
            }
            case TimelineRulerMode.AutoTimeScale:
            default: {
                TimeSpan ts = new TimeSpan(ticks);
                if (ts.TotalHours >= 1.0)
                    return $"{(int) ts.TotalHours}h {ts.Minutes:D2}m {ts.Seconds:D2}s";
                if (ts.TotalMinutes >= 1.0)
                    return $"{(int) ts.TotalMinutes}m {ts.Seconds:D2}s";
                return Math.Round((double) ts.Ticks / TimeSpan.TicksPerSecond % TimeSpan.SecondsPerMinute, 2) + "s";
            }
        }
    }
}

public enum TimelineRulerMode {
    AutoTimeScale,
    FPS
}