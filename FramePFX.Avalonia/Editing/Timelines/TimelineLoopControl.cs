// 
// Copyright (c) 2023-2024 REghZy
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
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Reactive;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.UI;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Editing.Timelines;

/// <summary>
/// A control with two grips to drag around a frame span based region
/// </summary>
public class TimelineLoopControl : TemplatedControl {
    public static readonly StyledProperty<TimelineControl?> TimelineControlProperty = AvaloniaProperty.Register<TimelineLoopControl, TimelineControl?>(nameof(TimelineControl));
    public static readonly StyledProperty<FrameSpan> LoopSpanProperty = AvaloniaProperty.Register<TimelineLoopControl, FrameSpan>(nameof(LoopSpan));
    public static readonly StyledProperty<ScrollViewer?> ScrollViewerReferenceProperty = AvaloniaProperty.Register<TimelineLoopControl, ScrollViewer?>(nameof(ScrollViewerReference));

    public TimelineControl? TimelineControl {
        get => this.GetValue(TimelineControlProperty);
        set => this.SetValue(TimelineControlProperty, value);
    }

    public FrameSpan LoopSpan {
        get => this.GetValue(LoopSpanProperty);
        set => this.SetValue(LoopSpanProperty, value);
    }

    public ScrollViewer? ScrollViewerReference {
        get => this.GetValue(ScrollViewerReferenceProperty);
        set => this.SetValue(ScrollViewerReferenceProperty, value);
    }

    private const int StateNone = 0;
    private const int StateInit = 1;
    private const int StateActive = 2;
    private const double MinDragInitPx = 5d;

    private Timeline? myTimeline;
    private bool isUpdatingControl;
    private bool isLoopRegionEnabled;
    private IDisposable? zoomChangeHandler;
    private double currZoom;

    public TimelineLoopControl() {
    }

    static TimelineLoopControl() {
        TimelineControlProperty.Changed.AddClassHandler<TimelineLoopControl, TimelineControl?>((d, e) => d.OnTimelineControlChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        LoopSpanProperty.Changed.AddClassHandler<TimelineLoopControl, FrameSpan>((d, e) => d.OnControlLoopSpanChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        ScrollViewerReferenceProperty.Changed.AddClassHandler<TimelineLoopControl, ScrollViewer?>((d, e) => d.OnScrollViewerReferenceChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnScrollViewerReferenceChanged(ScrollViewer? oldValue, ScrollViewer? newValue) {
        if (oldValue != null) {
            oldValue.ScrollChanged += this.OnScrollViewerScrollChanged;
            oldValue.EffectiveViewportChanged += this.OnScrollViewerEffectiveViewPortChanged;
        }

        if (newValue != null) {
            newValue.ScrollChanged += this.OnScrollViewerScrollChanged;
            newValue.EffectiveViewportChanged += this.OnScrollViewerEffectiveViewPortChanged;
        }
    }

    private void OnScrollViewerEffectiveViewPortChanged(object? sender, EffectiveViewportChangedEventArgs e) => this.UpdatePosition();

    private void OnScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e) => this.UpdatePosition();


    private void OnTimelineControlChanged(TimelineControl? oldTimeline, TimelineControl? newTimeline) {
        this.zoomChangeHandler?.Dispose();
        if (oldTimeline != null) {
            oldTimeline.TimelineModelChanged -= this.OnControlTimelineChanged;
        }

        if (this.myTimeline != null) {
            this.OnTimelineChanged(this.myTimeline, null);
        }

        if (newTimeline != null) {
            this.zoomChangeHandler = TimelineControl.ZoomProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<double>>(this.OnTimelineZoomed));
            this.currZoom = newTimeline.Zoom;
            newTimeline.TimelineModelChanged += this.OnControlTimelineChanged;
            if (newTimeline.Timeline is Timeline timeline) {
                this.OnTimelineChanged(null, timeline);
            }

            this.UpdatePosition();
        }
    }

    private void OnControlTimelineChanged(ITimelineElement element, Timeline? oldTimeline, Timeline? newTimeline) {
        Debug.Assert(oldTimeline == this.myTimeline);
        this.myTimeline = newTimeline;
        this.OnTimelineChanged(oldTimeline, newTimeline);
        this.UpdatePosition();
    }

    private void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.LoopRegionChanged -= this.OnTimelineLoopRegionChanged;
            oldTimeline.IsLoopRegionEnabledChanged -= this.OnIsLoopRegionEnabledChanged;
        }

        if (newTimeline != null) {
            newTimeline.LoopRegionChanged += this.OnTimelineLoopRegionChanged;
            newTimeline.IsLoopRegionEnabledChanged += this.OnIsLoopRegionEnabledChanged;
            this.isLoopRegionEnabled = newTimeline.IsLoopRegionEnabled;
            this.LoopSpan = newTimeline.LoopRegion ?? FrameSpan.Empty;
            this.InvalidateVisual();
        }
    }

    private void OnIsLoopRegionEnabledChanged(Timeline timeline) {
        this.isLoopRegionEnabled = timeline.IsLoopRegionEnabled;
        this.InvalidateVisual();
    }

    private void OnControlLoopSpanChanged(FrameSpan oldValue, FrameSpan newValue) {
        if (!this.isUpdatingControl && this.myTimeline != null) {
            this.myTimeline.LoopRegion = newValue.Clamp(new FrameSpan(0, this.myTimeline.MaxDuration));
        }

        this.UpdatePosition();
    }

    private void OnTimelineLoopRegionChanged(Timeline timeline) {
        this.isUpdatingControl = true;
        this.LoopSpan = timeline.LoopRegion ?? FrameSpan.Empty;
        this.isUpdatingControl = false;
        this.InvalidateVisual();
    }

    private void OnTimelineZoomed(AvaloniaPropertyChangedEventArgs<double> e) {
        this.currZoom = e.NewValue.GetValueOrDefault(1.0);
        this.UpdatePosition();
    }

    private void UpdatePosition() {
        long beginFrame = this.LoopSpan.Begin;
        Thickness m = this.Margin;
        double left;
        if (this.ScrollViewerReference is ScrollViewer scroller) {
            // Point translated = scroller.TranslatePoint(new Point(frame * zoom, 0.0), this) ?? new Point(scroller.Offset.X, 0.0);
            double offset = scroller.Offset.X;
            // double start = zoom - (offset - (long) (offset / zoom) * zoom);
            // double firstMajor = offset % zoom == 0D ? offset : offset + (zoom - offset % zoom);
            // double firstMajorRelative = zoom - (offset - firstMajor + zoom);
            // // left = (frame * zoom) - offset;
            // // left = (frame * zoom) - ((long) (offset / zoom) * zoom) - (this is GrippedPlayHeadControl ? 7 : 0);
            // Point? translated2 = this.TranslatePoint(new Point(this.Bounds.Width / 2.0, 0.0), (Visual) scroller.Content!);
            // double offset3 = (translated2?.X - m.Left) ?? 0.0; 

            left = (beginFrame * this.currZoom) - Math.Round(offset);
        }
        else {
            left = (beginFrame * this.currZoom);
        }

        this.Margin = new Thickness(Math.Floor(left), m.Top, m.Right, m.Bottom);
    }

    public override void Render(DrawingContext ctx) {
        base.Render(ctx);
        if (!(this.TimelineControl is TimelineControl control)) {
            return;
        }

        FrameSpan span = this.LoopSpan;
        if (span.Begin < 0 || span.IsEmpty) {
            return;
        }

        ImmutablePen gripPen = new ImmutablePen(Brushes.DarkBlue, 1);
        double x1 = 0.5; //TimelineUtils.FrameToPixel(span.Begin, control.Zoom);
        double x2 = TimelineUtils.FrameToPixel(span.Duration, this.currZoom);
        double height = this.Bounds.Height;

        using (ctx.PushRenderOptions(new RenderOptions() { EdgeMode = EdgeMode.Aliased })) {
            if (this.isLoopRegionEnabled) {
                ImmutableSolidColorBrush brush = new ImmutableSolidColorBrush(Colors.DodgerBlue, 0.1);
                ctx.DrawRectangle(brush, null, new Rect(x1, 0, x2 - x1, height));
            }

            ctx.DrawLine(gripPen, new Point(x1, 0), new Point(x1, height));
            ctx.DrawLine(gripPen, new Point(x2, 0), new Point(x2, height));
        }
    }
}