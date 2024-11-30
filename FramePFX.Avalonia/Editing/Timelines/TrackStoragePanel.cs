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
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Tracks;

namespace FramePFX.Avalonia.Editing.Timelines;

public class TrackStoragePanel : StackPanel {
    public static readonly StyledProperty<Timeline?> TimelineProperty = AvaloniaProperty.Register<TrackStoragePanel, Timeline?>(nameof(Timeline));
    private readonly ModelControlDictionary<Track, TimelineTrackControl> itemMap = new ModelControlDictionary<Track, TimelineTrackControl>();
    public IModelControlDictionary<Track, TimelineTrackControl> ItemMap => this.itemMap;

    public Timeline? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    /// <summary>
    /// Gets the timeline control that this panel is stored in
    /// </summary>
    public TimelineControl? TimelineControl { get; private set; }

    private readonly Stack<TimelineTrackControl> cachedTracks;

    public TrackStoragePanel() {
        this.cachedTracks = new Stack<TimelineTrackControl>();
    }

    static TrackStoragePanel() {
        TimelineProperty.Changed.AddClassHandler<TrackStoragePanel, Timeline?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    public void SetTimelineControl(TimelineControl control) => this.TimelineControl = control;

    private void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.TrackAdded -= this.OnTrackAdded;
            oldTimeline.TrackRemoved -= this.OnTrackRemoved;
            oldTimeline.TrackMoved -= this.OnTrackIndexMoved;
            oldTimeline.MaxDurationChanged -= this.OnMaxDurationChanged;
            for (int i = this.Children.Count - 1; i >= 0; i--) {
                this.RemoveTrackInternal(i);
            }
        }

        if (newTimeline != null) {
            newTimeline.TrackAdded += this.OnTrackAdded;
            newTimeline.TrackRemoved += this.OnTrackRemoved;
            newTimeline.TrackMoved += this.OnTrackIndexMoved;
            newTimeline.MaxDurationChanged += this.OnMaxDurationChanged;
            int i = 0;
            foreach (Track track in newTimeline.Tracks) {
                this.InsertTrackInternal(track, i++);
            }
        }
    }

    private void OnMaxDurationChanged(Timeline timeline) => this.InvalidateMeasure();

    private void OnTrackAdded(Timeline timeline, Track track, int index) {
        this.InsertTrackInternal(track, index);
    }

    private void OnTrackRemoved(Timeline timeline, Track track, int index) {
        this.RemoveTrackInternal(index);
    }

    private void OnTrackIndexMoved(Timeline timeline, Track track, int oldIndex, int newIndex) {
        TimelineTrackControl control = (TimelineTrackControl) this.Children[oldIndex];
        control.OnIndexMoving(oldIndex, newIndex);
        this.Children.RemoveAt(oldIndex);
        this.Children.Insert(newIndex, control);
        control.OnIndexMoved(oldIndex, newIndex);
        this.InvalidateMeasure();
    }

    private void InsertTrackInternal(Track track, int index) {
        TimelineTrackControl control = this.cachedTracks.Count > 0 ? this.cachedTracks.Pop() : new TimelineTrackControl();
        this.itemMap.AddMapping(track, control);
        control.OnConnecting(this, track);
        this.Children.Insert(index, control);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnConnected();
        this.InvalidateMeasure();
        this.InvalidateVisual();
        this.TimelineControl!.ClipSelectionManager!.InternalOnTrackAdded(control);
    }

    private void RemoveTrackInternal(int index) {
        TimelineTrackControl control = (TimelineTrackControl) this.Children[index];
        Track model = control.Track!;
        this.TimelineControl!.ClipSelectionManager!.InternalOnTrackRemoving(control);
        this.itemMap.RemoveMapping(model, control);
        control.OnDisconnecting();
        this.Children.RemoveAt(index);
        control.OnDisconnected();
        if (this.cachedTracks.Count < 4) {
            this.ResetControl(control);
            this.cachedTracks.Push(control);
        }

        this.InvalidateMeasure();
        this.InvalidateVisual();
    }
    
    private void ResetControl(TimelineTrackControl control) {
        TimelineTrackControl.InternalSetIsSelected(control, false);
    }

    protected override Size MeasureOverride(Size availableSize) {
        double totalHeight = 0d;
        double maxWidth = 0d;
        Controls items = this.Children;
        int count = items.Count;
        for (int i = 0; i < count; i++) {
            TimelineTrackControl track = (TimelineTrackControl) items[i];
            track.Measure(availableSize);
            totalHeight += track.DesiredSize.Height;
            maxWidth = Math.Max(maxWidth, track.Bounds.Width);
        }

        // the gap between tracks, only when there's 2 or more tracks obviously
        if (count > 1) {
            totalHeight += count - 1;
        }

        if (this.Timeline is Timeline t && this.TimelineControl != null) {
            maxWidth = this.TimelineControl.Zoom * t.MaxDuration;
        }

        return new Size(maxWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize) {
        double totalY = 0d;
        Controls items = this.Children;
        for (int i = 0, count = items.Count; i < count; i++) {
            TimelineTrackControl track = (TimelineTrackControl) items[i];
            track.Arrange(new Rect(new Point(0, totalY), new Size(finalSize.Width, track.DesiredSize.Height)));
            totalY += track.Bounds.Height + 1d; // +1d for the gap between tracks
        }

        return finalSize;
    }

    /// <summary>
    /// Gets a track control from a model, or null if one does not exist
    /// </summary>
    /// <param name="track">The model</param>
    /// <returns>The control</returns>
    public TimelineTrackControl? GetTrackByModel(Track track) {
        return this.itemMap.TryGetControl(track, out TimelineTrackControl? trackControl) ? trackControl : null;
    }

    public IEnumerable<TimelineTrackControl> GetTracks() => this.Children.Cast<TimelineTrackControl>();

    public void OnZoomChanged(double newZoom) {
        foreach (TimelineTrackControl track in this.GetTracks()) {
            track.OnZoomChanged(newZoom);
        }
    }

    public TimelineTrackControl GetTrack(int i) => (TimelineTrackControl) this.Children[i];

    public void SetPlayHeadToMouseCursor(PointerEventArgs e) {
        if (this.TimelineControl != null) {
            this.TimelineControl.SetPlayHeadToMouseCursor(e.GetPosition(this).X);
        }
    }
}