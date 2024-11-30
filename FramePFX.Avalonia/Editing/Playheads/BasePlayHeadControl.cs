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
using Avalonia.Controls.Primitives;
using Avalonia.Reactive;
using FramePFX.Avalonia.Editing.Timelines;
using FramePFX.Editing.Timelines;

namespace FramePFX.Avalonia.Editing.Playheads;

public abstract class BasePlayHeadControl : TemplatedControl {
    private IDisposable? zoomChangeHandler;
    private IDisposable? timelineChangeHandler;
    public static readonly StyledProperty<TimelineControl?> TimelineControlProperty = AvaloniaProperty.Register<BasePlayHeadControl, TimelineControl?>(nameof(TimelineControl));

    public TimelineControl? TimelineControl {
        get => this.GetValue(TimelineControlProperty);
        set => this.SetValue(TimelineControlProperty, value);
    }

    private Timeline? lastTimeline;

    protected BasePlayHeadControl() {
    }

    static BasePlayHeadControl() {
        TimelineControlProperty.Changed.AddClassHandler<BasePlayHeadControl, TimelineControl?>((d, e) => d.OnTimelineControlChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    public abstract long GetFrame(Timeline timeline);

    protected virtual void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        Debug.Assert(this.lastTimeline == oldTimeline, "Different last timelines");
        this.lastTimeline = newTimeline;
        if (newTimeline != null) {
            this.IsVisible = this.TimelineControl != null;
            this.UpdateZoom();
        }
        else {
            this.IsVisible = false;
        }
    }

    protected virtual void OnTimelineControlChanged(TimelineControl? oldTimeline, TimelineControl? newTimeline) {
        if (oldTimeline != null) {
            this.zoomChangeHandler?.Dispose();
            this.timelineChangeHandler?.Dispose();
        }

        if (newTimeline != null) {
            this.timelineChangeHandler = TimelineControl.TimelineProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<Timeline?>>((e) => this.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault())));
            this.zoomChangeHandler = TimelineControl.ZoomProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<double>>(this.OnTimelineZoomed));
            Timeline? newTimelineModel = newTimeline.Timeline;
            if (newTimelineModel != null) {
                Debug.Assert(this.lastTimeline == oldTimeline?.Timeline, "Different last timelines");
                this.lastTimeline = newTimelineModel;
                this.OnTimelineChanged(oldTimeline?.Timeline, newTimelineModel);
            }

            this.IsVisible = newTimeline.Timeline != null;
            this.UpdateZoom();
        }
        else {
            this.IsVisible = false;
        }
    }

    private void UpdateZoom() {
        if (this.TimelineControl is TimelineControl control && control.Timeline is Timeline timeline)
            this.SetPixelFromFrameAndZoom(this.GetFrame(timeline), control.Zoom);
    }

    private void OnTimelineZoomed(AvaloniaPropertyChangedEventArgs<double> e) {
        if (this.TimelineControl?.Timeline is Timeline timeline)
            this.SetPixelFromFrameAndZoom(this.GetFrame(timeline), e.NewValue.GetValueOrDefault(1.0));
    }

    protected void SetPixelFromFrame(long frame) {
        if (this.TimelineControl is TimelineControl control)
            this.SetPixelFromFrameAndZoom(frame, control.Zoom);
    }

    protected virtual void SetPixelFromFrameAndZoom(long frame, double zoom) {
        Thickness m = this.Margin;
        this.SetPixelMargin(new Thickness(frame * zoom, m.Top, m.Right, m.Bottom));
    }

    protected virtual void SetPixelMargin(Thickness thickness) {
        this.Margin = thickness;
    }
}