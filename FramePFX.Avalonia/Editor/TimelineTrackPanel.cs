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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using FramePFX.Editing;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Avalonia.Editor;

/// <summary>
/// A panel which contains the tracks (<see cref="TimelineTrackControl"/>)
/// </summary>
public class TimelineTrackPanel : Panel {
    public static readonly StyledProperty<TimelineViewState?> TimelineProperty = AvaloniaProperty.Register<TimelineTrackPanel, TimelineViewState?>(nameof(Timeline));
    
    private const double Spacing = 1.0;

    /// <summary>
    /// The maximum number of track controls that can be cached
    /// </summary>
    public const int MaxCachedTracks = 8;

    /// <summary>
    /// Gets or sets the timeline
    /// </summary>
    public TimelineViewState? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }
    
    public TimelineControl TimelineControl {
        get => field ?? throw new InvalidOperationException("No timeline control connected");
        internal set => field = value;
    }

    private readonly Stack<TimelineTrackControl> trackCache;

    public TimelineTrackPanel() {
        this.trackCache = new Stack<TimelineTrackControl>(MaxCachedTracks);
    }

    static TimelineTrackPanel() {
        TimelineProperty.Changed.AddClassHandler<TimelineTrackPanel, TimelineViewState?>((s, e) => s.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnTimelineChanged(TimelineViewState? oldTimeline, TimelineViewState? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.Timeline.TrackAdded -= this.TimelineOnTrackAdded;
            oldTimeline.Timeline.TrackRemoved -= this.TimelineOnTrackRemoved;
            oldTimeline.Timeline.TrackMoved -= this.TimelineOnTrackMoved;
            
            ReadOnlyCollection<Track> tracks = oldTimeline.Timeline.Tracks;
            for (int i = tracks.Count - 1; i >= 0; i--) {
                this.TimelineOnTrackRemoved(null, new TrackAddedOrRemovedEventArgs(tracks[i], i));
            }
        }

        if (newTimeline != null) {
            newTimeline.Timeline.TrackAdded += this.TimelineOnTrackAdded;
            newTimeline.Timeline.TrackRemoved += this.TimelineOnTrackRemoved;
            newTimeline.Timeline.TrackMoved += this.TimelineOnTrackMoved;
            
            int i = 0;
            foreach (Track track in newTimeline.Timeline.Tracks) {
                this.TimelineOnTrackAdded(null, new TrackAddedOrRemovedEventArgs(track, i++));
            }
        }
    }
    
    private void TimelineOnTrackAdded(object? sender, TrackAddedOrRemovedEventArgs e) {
        TimelineTrackControl control = GetOrCreateTrack();
        control.OnConnecting(this, TrackViewState.GetInstance(e.Track, this.Timeline!.TopLevelIdentifier));
        this.Children.Insert(e.Index, control);
        control.OnConnected();
        return;
        
        TimelineTrackControl GetOrCreateTrack() {
            if (!this.trackCache.TryPop(out TimelineTrackControl? c))
                c = new TimelineTrackControl();
            return c;
        }
    }

    private void TimelineOnTrackRemoved(object? sender, TrackAddedOrRemovedEventArgs e) {
        TimelineTrackControl control = (TimelineTrackControl) this.Children[e.Index];
        control.OnRemoving();
        this.Children.RemoveAt(e.Index);
        control.OnRemoved();
        this.TryPushCachedTrack(control);
    }

    private void TimelineOnTrackMoved(object? sender, TrackMovedEventArgs e) {
        this.Children.Move(e.OldIndex, e.NewIndex);
    }

    private void TryPushCachedTrack(TimelineTrackControl control) {
        if (this.trackCache.Count < MaxCachedTracks) {
            this.trackCache.Push(control);
        }
    }

    public bool IsFullyVisibleInPanel(TimelineTrackControl control) {
        return this.TimelineControl.IsFullyVisibleInPanel(control);
    }

    protected override Size MeasureOverride(Size availableSize) {
        Size stackDesiredSize = new Size();
        Controls children = this.Children;
        Size layoutSlotSize = availableSize;
        bool hasVisibleChild = false;

        layoutSlotSize = layoutSlotSize.WithHeight(Double.PositiveInfinity);

        for (int i = 0, count = children.Count; i < count; ++i) {
            Control child = children[i];
            bool isVisible = child.IsVisible;
            if (isVisible && !hasVisibleChild) {
                hasVisibleChild = true;
            }

            child.Measure(layoutSlotSize);
            stackDesiredSize = stackDesiredSize.WithWidth(Math.Max(stackDesiredSize.Width, child.DesiredSize.Width));
            stackDesiredSize = stackDesiredSize.WithHeight(stackDesiredSize.Height + (isVisible ? Spacing : 0) + child.DesiredSize.Height);
        }

        stackDesiredSize = stackDesiredSize.WithHeight(stackDesiredSize.Height - (hasVisibleChild ? Spacing : 0));
        return stackDesiredSize;
    }

    /// <summary>
    /// Content arrangement.
    /// </summary>
    /// <param name="finalSize">Arrange size</param>
    protected override Size ArrangeOverride(Size finalSize) {
        Controls children = this.Children;
        Rect rcChild = new Rect(finalSize);
        double previousChildSize = 0.0;

        for (int i = 0, count = children.Count; i < count; ++i) {
            Control child = children[i];
            if (!child.IsVisible) {
                continue;
            }

            rcChild = rcChild.WithY(rcChild.Y + previousChildSize);
            previousChildSize = child.DesiredSize.Height;
            rcChild = rcChild.WithHeight(previousChildSize);
            rcChild = rcChild.WithWidth(Math.Max(finalSize.Width, child.DesiredSize.Width));
            previousChildSize += Spacing;
            child.Arrange(rcChild);
        }

        return finalSize;
    }

    public void OnZoomChanged(ValueChangedEventArgs<double> e) {
        this.InvalidateRenderForAllTracks();
        foreach (Control control in this.Children) {
            ((TimelineTrackControl) control).OnZoomChanged(e);
        }
    }

    public void OnHorizontalScrollChanged(ValueChangedEventArgs<TimeSpan> e) {
        this.InvalidateRenderForAllTracks();
        foreach (Control control in this.Children) {
            ((TimelineTrackControl) control).OnHorizontalScrollChanged(e);
        }
    }

    public void InvalidateRenderForAllTracks() {
        foreach (Control control in this.Children) {
            ((TimelineTrackControl) control).InvalidateTrackRender();
        }
    }
}