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
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using FramePFX.Editing;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Interactivity.SelectingEx;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.Reactive;

namespace FramePFX.Avalonia.Editor;

public partial class TimelineControl : UserControl {
    private static readonly IEventObservable<TimelineViewState> s_TVS_MaximumDurationChanged = 
        Observable.ForEvent<TimelineViewState, ValueChangedEventArgs<TimeSpan>>(
            (tvs, handler) => tvs.Timeline.MaximumDurationChanged += handler, 
            (tvs, handler) => tvs.Timeline.MaximumDurationChanged -= handler);
    
    public static readonly StyledProperty<TimelineViewState?> TimelineProperty = AvaloniaProperty.Register<TimelineControl, TimelineViewState?>(nameof(Timeline));
    public static readonly DirectProperty<TimelineControl, double> ZoomProperty = AvaloniaProperty.RegisterDirect<TimelineControl, double>(nameof(Zoom), o => o.Zoom);
    public static readonly DirectProperty<TimelineControl, TimeSpan> HorizontalScrollProperty = AvaloniaProperty.RegisterDirect<TimelineControl, TimeSpan>(nameof(HorizontalScroll), o => o.HorizontalScroll);

    public TimelineViewState? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    public double Zoom {
        get => field;
        private set => this.SetAndRaise(ZoomProperty, ref field, value);
    }

    public TimeSpan HorizontalScroll {
        get => field;
        private set => this.SetAndRaise(HorizontalScrollProperty, ref field, value);
    }

    private readonly IBinder<Timeline> playHeadLocationBinder = new EventUpdateBinder<Timeline>(nameof(Editing.Timeline.PlayHeadLocationChanged), b => ((TextBlock) b.Control).Text = b.Model.PlayHeadLocation.ToString("c"));
    private readonly IBinder<TimelineViewState> scrollBarMaximumBinder = new ObservableUpdateBinder<TimelineViewState>(s_TVS_MaximumDurationChanged, b => ((ScrollBar) b.Control).Maximum = TimelineUnits.TicksToPixels(b.Model.Timeline.MaximumDuration.Ticks, b.Model.Zoom));

    private readonly IBinder<TimelineViewState> scrollBarValueBinder =
        new AvaloniaPropertyToEventPropertyBinder<TimelineViewState>(
            ScrollBar.ValueProperty,
            nameof(TimelineViewState.HorizontalScrollChanged),
            b => ((ScrollBar) b.Control).Value = TimelineUnits.TicksToPixels(b.Model.HorizontalScroll.Ticks, b.Model.Zoom),
            b => b.Model.HorizontalScroll = new TimeSpan(TimelineUnits.PixelsToTicks(((ScrollBar) b.Control).Value, b.Model.Zoom)));

    private bool isUpdatingSyncedScroll;
    private ScrollViewer? trackSettingsScrollViewer;

    public TimelineControl() {
        this.InitializeComponent();
        this.PART_TrackPanel.TimelineControl = this;
        this.PART_ScrollViewer.AddHandler(PointerWheelChangedEvent, this.OnPointerWheelChanged, RoutingStrategies.Tunnel);

        this.scrollBarMaximumBinder.AttachControl(this.PART_HorizontalScrollBar);
        this.scrollBarValueBinder.AttachControl(this.PART_HorizontalScrollBar);
        this.playHeadLocationBinder.AttachControl(this.PART_PlayHeadLocationTextBlock);

        this.PART_TrackSettingsListBox.PropertyChanged += this.PART_TrackSettingsListBoxOnPropertyChanged;
        this.PART_ScrollViewer.ScrollChanged += this.ScrollChanged;
    }

    private void PART_TrackSettingsListBoxOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == ListBox.ScrollProperty) {
            if (this.trackSettingsScrollViewer != null) {
                this.trackSettingsScrollViewer.ScrollChanged -= this.ScrollChanged;
            }

            if (this.PART_TrackSettingsListBox.Scroll is ScrollViewer scrollViewer) {
                this.trackSettingsScrollViewer = scrollViewer;
                scrollViewer.ScrollChanged += this.ScrollChanged;
            }
        }
    }

    static TimelineControl() {
        TimelineProperty.Changed.AddClassHandler<TimelineControl, TimelineViewState?>((s, e) => s.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void ScrollChanged(object? sender, ScrollChangedEventArgs e) {
        if (this.isUpdatingSyncedScroll) {
            return;
        }

        try {
            this.isUpdatingSyncedScroll = true;

            if (ReferenceEquals(sender, this.PART_ScrollViewer)) {
                Vector o = this.trackSettingsScrollViewer!.Offset;
                this.trackSettingsScrollViewer!.Offset = new Vector(o.X, o.Y + e.OffsetDelta.Y);
                this.trackSettingsScrollViewer!.UpdateLayout();
            }
            else {
                Vector o = this.PART_ScrollViewer!.Offset;
                this.PART_ScrollViewer!.Offset = new Vector(o.X, o.Y + e.OffsetDelta.Y);
                this.PART_ScrollViewer!.UpdateLayout();
            }
        }
        finally {
            this.isUpdatingSyncedScroll = false;
        }
    }

    public bool IsZoomingAndScrollingDisabled() {
        return this.PART_TrackPanel.Children.Any(x => ((TimelineTrackControl) x).DragHandler.IsDraggingOrWaitingForRegion);
    }

    private void OnTimelineChanged(TimelineViewState? oldTimeline, TimelineViewState? newTimeline) {
        this.PART_TrackPanel.Timeline = newTimeline;
        this.PART_TrackSettingsListBox.Timeline = newTimeline;
        this.PART_Ruler.Timeline = newTimeline;
        this.PART_PlayHeadRenderSurface.Timeline = newTimeline;
        if (oldTimeline != null) {
            oldTimeline.ZoomChanged -= this.OnZoomChanged;
            oldTimeline.HorizontalScrollChanged -= this.OnHorizontalScrollChanged;
        }

        if (newTimeline != null) {
            newTimeline.ZoomChanged += this.OnZoomChanged;
            newTimeline.HorizontalScrollChanged += this.OnHorizontalScrollChanged;

            this.Zoom = newTimeline.Zoom;
            this.HorizontalScroll = newTimeline.HorizontalScroll;
        }

        this.scrollBarMaximumBinder.SwitchModel(newTimeline);
        this.playHeadLocationBinder.SwitchModel(newTimeline?.Timeline);
        this.scrollBarValueBinder.SwitchModel(newTimeline);
    }

    private void OnZoomChanged(object? sender, ValueChangedEventArgs<double> e) {
        this.PART_TrackPanel.OnZoomChanged(e);
        this.Zoom = e.NewValue;
        this.scrollBarMaximumBinder.UpdateControl();
    }

    private void OnHorizontalScrollChanged(object? sender, ValueChangedEventArgs<TimeSpan> e) {
        this.PART_TrackPanel.OnHorizontalScrollChanged(e);
        this.HorizontalScroll = e.NewValue;
    }

    public long SnapTick(long tick, bool useMinorTicks = true) {
        double ticksPerPixel = TimelineUnits.GetTicksPerPixelRatio(this.Zoom);
        long majorStepTicks = this.PART_Ruler.CalculateMajorStepTicks(ticksPerPixel, out int minorDivisions);
        long minorStepTicks = useMinorTicks ? majorStepTicks / minorDivisions : majorStepTicks;
        if (minorStepTicks != 0) {
            return Math.Max(0, (tick + minorStepTicks / 2) / minorStepTicks * minorStepTicks);
        }

        return tick;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);

        TimelineViewState? timeline;
        if (!e.Handled && e.Properties.IsLeftButtonPressed && (timeline = this.Timeline) != null) {
            double mouseX = e.GetPosition(this.PART_ScrollViewer).X;
            double pixelsPerTick = TimelineUnits.GetPixelsPerTickRatio(timeline.Zoom);
            long newTick = timeline.HorizontalScroll.Ticks + (long) (mouseX / pixelsPerTick);
            // newTick = this.SnapTick(newTick, useMinorTicks: true);

            timeline.Timeline.PlayHeadLocation = new TimeSpan(newTick);
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
        TimelineViewState? timeline = this.Timeline;
        if (timeline == null) {
            return;
        }

        KeyModifiers mods = e.KeyModifiers;
        if ((mods & KeyModifiers.Alt) != 0) {
            TimelineTrackControl? track = VisualTreeUtils.FindLogicalParent<TimelineTrackControl>(e.Source as AvaloniaObject);
            track?.Height += e.Delta.Y * 8;
            e.Handled = true;
        }
        else if ((mods & KeyModifiers.Control) != 0) {
            if (this.IsZoomingAndScrollingDisabled()) {
                return;
            }

            e.Handled = true;
            bool shift = (mods & KeyModifiers.Shift) != 0;
            double multiplier = (shift ? 0.2 : 0.4);
            if (e.Delta.Y > 0) {
                multiplier = 1d + multiplier;
            }
            else {
                multiplier = 1d - multiplier;
            }

            double oldZoom = timeline.Zoom;
            double newZoom = oldZoom * multiplier;
            timeline.Zoom = newZoom;
            newZoom = timeline.Zoom; // get again for coerced value

            // managed to get zooming towards the cursor working
            double mouseX = e.GetPosition(this.PART_ScrollViewer).X;
            double oldRatio = TimelineUnits.GetPixelsPerTickRatio(oldZoom);
            double newRatio = TimelineUnits.GetPixelsPerTickRatio(newZoom);
            long oldMousePosTick = timeline.HorizontalScroll.Ticks + (long) (mouseX / oldRatio);
            long newMousePosTick = oldMousePosTick - (long) (mouseX / newRatio);
            timeline.HorizontalScroll = new TimeSpan(Math.Max(0, newMousePosTick));

            e.Handled = true;
        }
        else if ((mods & KeyModifiers.Shift) != 0) {
            if (this.IsZoomingAndScrollingDisabled()) {
                return;
            }

            const double PixelsPerScrollStep = 96.0;
            long ticksDelta = (long) (PixelsPerScrollStep / TimelineUnits.GetPixelsPerTickRatio(timeline.Zoom));
            if (e.Delta.Y < 0 || e.Delta.X < 0) {
                timeline.HorizontalScroll += new TimeSpan(ticksDelta);
            }
            else {
                timeline.HorizontalScroll -= new TimeSpan(ticksDelta);
            }

            e.Handled = true;
        }
    }

    public bool IsFullyVisibleInPanel(TimelineTrackControl control) {
        if (control.IsVisible && control.IsEffectivelyVisible) {
            Rect controlBounds = control.Bounds;
            Matrix? transform = control.TransformToVisual(this.PART_ScrollViewer);
            if (transform.HasValue) {
                Rect transformedBounds = controlBounds.TransformToAABB(transform.Value);

                Vector offset = this.PART_ScrollViewer.Offset;
                Size vp = this.PART_ScrollViewer.Viewport;
                Rect viewport = new Rect(offset.X, offset.Y, vp.Width, vp.Height);

                return transformedBounds.Intersects(viewport);
            }
        }

        return false;
    }
}