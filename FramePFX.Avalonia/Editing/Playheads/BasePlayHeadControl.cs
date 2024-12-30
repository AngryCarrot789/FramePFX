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
using FramePFX.Avalonia.Editing.Timelines;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.UI;

namespace FramePFX.Avalonia.Editing.Playheads;

public abstract class BasePlayHeadControl : TemplatedControl {
    public static readonly StyledProperty<TimelineControl?> TimelineControlProperty = AvaloniaProperty.Register<BasePlayHeadControl, TimelineControl?>(nameof(TimelineControl));
    public static readonly StyledProperty<PlayHeadType> PlayHeadTypeProperty = AvaloniaProperty.Register<BasePlayHeadControl, PlayHeadType>(nameof(PlayHeadType), PlayHeadType.PlayHead);
    public static readonly StyledProperty<ScrollViewer?> ScrollViewerReferenceProperty = AvaloniaProperty.Register<BasePlayHeadControl, ScrollViewer?>(nameof(ScrollViewerReference));
    public static readonly StyledProperty<Thickness> AdditionalOffsetProperty = AvaloniaProperty.Register<BasePlayHeadControl, Thickness>(nameof(AdditionalOffset));
    public static readonly StyledProperty<BasePlayHeadControl?> SnapToDecimalSourceProperty = AvaloniaProperty.Register<BasePlayHeadControl, BasePlayHeadControl?>(nameof(SnapToDecimalSource));

    public BasePlayHeadControl? SnapToDecimalSource {
        get => this.GetValue(SnapToDecimalSourceProperty);
        set => this.SetValue(SnapToDecimalSourceProperty, value);
    }

    public TimelineControl? TimelineControl {
        get => this.GetValue(TimelineControlProperty);
        set => this.SetValue(TimelineControlProperty, value);
    }

    public PlayHeadType PlayHeadType {
        get => this.GetValue(PlayHeadTypeProperty);
        set => this.SetValue(PlayHeadTypeProperty, value);
    }

    public ScrollViewer? ScrollViewerReference {
        get => this.GetValue(ScrollViewerReferenceProperty);
        set => this.SetValue(ScrollViewerReferenceProperty, value);
    }

    public Thickness AdditionalOffset {
        get => this.GetValue(AdditionalOffsetProperty);
        set => this.SetValue(AdditionalOffsetProperty, value);
    }

    private Timeline? myTimeline;
    private long myFrame;
    private double myZoom;

    public long Frame {
        get => this.myFrame;
        set {
            if (this.myTimeline == null)
                throw new InvalidOperationException("No timeline attached");

            if (value == this.myFrame)
                return;

            switch (this.PlayHeadType) {
                case PlayHeadType.PlayHead: this.myTimeline.PlayHeadPosition = value; break;
                case PlayHeadType.StopHead: this.myTimeline.StopHeadPosition = value; break;
            }
        }
    }

    protected BasePlayHeadControl() {
    }

    static BasePlayHeadControl() {
        TimelineControlProperty.Changed.AddClassHandler<BasePlayHeadControl, TimelineControl?>((d, e) => d.OnTimelineControlChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        PlayHeadTypeProperty.Changed.AddClassHandler<BasePlayHeadControl, PlayHeadType>((d, e) => d.OnPlayHeadTypeChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        ScrollViewerReferenceProperty.Changed.AddClassHandler<BasePlayHeadControl, ScrollViewer?>((d, e) => d.OnScrollViewerReferenceChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        AdditionalOffsetProperty.Changed.AddClassHandler<BasePlayHeadControl, Thickness>((d, e) => d.OnAdditionalOffsetChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        SnapToDecimalSourceProperty.Changed.AddClassHandler<BasePlayHeadControl, BasePlayHeadControl?>((d, e) => d.OnSnapToDecimalSourceChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnAdditionalOffsetChanged(Thickness oldValue, Thickness newValue) {
        this.UpdatePosition();
    }

    private void OnSnapToDecimalSourceChanged(BasePlayHeadControl? oldValue, BasePlayHeadControl? newValue) {
        this.UpdatePosition();
    }

    protected virtual void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        Debug.Assert(this.myTimeline == oldTimeline, "Different last timelines");
        this.myTimeline = newTimeline;
        if (oldTimeline != null) {
            this.UnregisterPlayHeadEvents(oldTimeline, this.PlayHeadType);
        }

        if (newTimeline != null) {
            // this.IsVisible = this.TimelineControl != null;
            this.RegisterPlayHeadEvents(newTimeline, this.PlayHeadType);
            this.UpdatePosition();
        }
        else {
            // this.IsVisible = false;
        }
    }

    protected virtual void OnTimelineControlChanged(TimelineControl? oldControl, TimelineControl? newControl) {
        if (oldControl != null) {
            oldControl.TimelineModelChanged -= this.OnTimelineModelChangedHandler;
            oldControl.ZoomChanged -= this.OnTimelineZoomChangedHandler;
        }

        if (newControl != null) {
            newControl.TimelineModelChanged += this.OnTimelineModelChangedHandler;
            newControl.ZoomChanged += this.OnTimelineZoomChangedHandler;
            this.myZoom = newControl.Zoom;
            Timeline? newTimelineModel = newControl.Timeline;
            if (newTimelineModel != null) {
                Debug.Assert(this.myTimeline == oldControl?.Timeline, "Different last timelines");
                this.OnTimelineChanged(oldControl?.Timeline, newTimelineModel);
            }

            // this.IsVisible = newTimeline.Timeline != null;
            this.UpdatePosition();
        }
        else {
            if (this.myTimeline != null) {
                this.OnTimelineChanged(this.myTimeline, null);
            }

            // this.IsVisible = false;
        }
    }

    private void OnTimelineZoomChangedHandler(object sender, AvaloniaPropertyChangedEventArgs<double> e) => this.OnTimelineZoomed(e);
    private void OnTimelineModelChangedHandler(ITimelineElement element, Timeline? oldTimeline, Timeline? newTimeline) => this.OnTimelineChanged(oldTimeline, newTimeline);

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

    private void OnPlayHeadTypeChanged(PlayHeadType oldValue, PlayHeadType newValue) {
        if (this.myTimeline != null) {
            this.UnregisterPlayHeadEvents(this.myTimeline, oldValue);
            this.RegisterPlayHeadEvents(this.myTimeline, newValue);
        }
    }

    private void UnregisterPlayHeadEvents(Timeline timeline, PlayHeadType playHeadType) {
        switch (playHeadType) {
            case PlayHeadType.None:     break;
            case PlayHeadType.PlayHead: timeline.PlayHeadChanged -= this.OnPlayHeadValueChanged; break;
            case PlayHeadType.StopHead: timeline.StopHeadChanged -= this.OnPlayHeadValueChanged; break;
        }
    }

    private void RegisterPlayHeadEvents(Timeline timeline, PlayHeadType playHeadType) {
        switch (playHeadType) {
            case PlayHeadType.None: break;
            case PlayHeadType.PlayHead:
                timeline.PlayHeadChanged += this.OnPlayHeadValueChanged;
                this.myFrame = timeline.PlayHeadPosition;
                return;
            case PlayHeadType.StopHead:
                timeline.StopHeadChanged += this.OnPlayHeadValueChanged;
                this.myFrame = timeline.StopHeadPosition;
                return;
        }
    }

    private void OnPlayHeadValueChanged(Timeline timeline, long oldValue, long newValue) {
        this.myFrame = newValue;
        this.UpdatePosition();
    }

    private void OnTimelineZoomed(AvaloniaPropertyChangedEventArgs<double> e) {
        this.myZoom = e.NewValue.GetValueOrDefault(1.0);
        this.UpdatePosition();
    }

    private void UpdatePosition() {
        this.SetPixelFromFrameAndZoom(this.myFrame, this.TimelineControl?.Zoom ?? 1.0);
    }

    protected virtual void SetPixelFromFrameAndZoom(long frame, double zoom) {
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

            left = (frame * zoom) - Math.Round(offset);
        }
        else {
            left = (frame * zoom);
        }

        Thickness a = this.AdditionalOffset;
        this.SetPixelMargin(new Thickness(Math.Floor(left + a.Left), m.Top + a.Top, m.Right + a.Right, m.Bottom + a.Bottom));
    }

    protected virtual void SetPixelMargin(Thickness thickness) {
        this.Margin = thickness;
    }
}