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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.Timelines;
using FramePFX.Interactivity.Contexts;
using Track = FramePFX.Editing.Timelines.Tracks.Track;

namespace FramePFX.Avalonia.Editing.Timelines.TrackSurfaces;

/// <summary>
/// The list box control which stores the track control surface items
/// </summary>
public class TrackControlSurfaceList : TemplatedControl {
    public static readonly StyledProperty<Timeline?> TimelineProperty = AvaloniaProperty.Register<TrackControlSurfaceList, Timeline?>(nameof(Timeline));

    public IModelControlDictionary<Track, TrackControlSurfaceItem> ItemMap => this.TrackStorage!.ItemMap;

    public Timeline? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    public TimelineControl? TimelineControl { get; set; }

    public TrackControlSurfacePanel? TrackStorage { get; private set; }

    public TrackControlSurfaceList() {
    }

    static TrackControlSurfaceList() {
        TimelineProperty.Changed.AddClassHandler<TrackControlSurfaceList, Timeline?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.TrackStorage = e.NameScope.GetTemplateChild<TrackControlSurfacePanel>("PART_TrackPanel");
        this.TrackStorage.Owner = this;
        this.TrackStorage.ApplyStyling();
        this.TrackStorage.ApplyTemplate();
    }

    public TrackControlSurfaceItem GetTrack(int index) => (TrackControlSurfaceItem) this.TrackStorage!.Children[index];

    private void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline == newTimeline)
            return;
        
        if (oldTimeline != null) {
            oldTimeline.TrackAdded -= this.OnTrackAdded;
            oldTimeline.TrackRemoved -= this.OnTrackRemoved;
            oldTimeline.TrackMoved -= this.OnTrackMoved;
            this.TrackStorage!.ClearTracks();
        }

        if (newTimeline != null) {
            newTimeline.TrackAdded += this.OnTrackAdded;
            newTimeline.TrackRemoved += this.OnTrackRemoved;
            newTimeline.TrackMoved += this.OnTrackMoved;
            this.TrackStorage!.LoadTracks(newTimeline);
        }

        DataManager.GetContextData(this).Set(DataKeys.TimelineKey, newTimeline);
    }

    private void OnTrackAdded(Timeline timeline, Track track, int index) {
        this.TrackStorage!.InsertTrack(track, index);
    }

    private void OnTrackRemoved(Timeline timeline, Track track, int index) {
        this.TrackStorage!.RemoveTrack(index);
    }

    private void OnTrackMoved(Timeline timeline, Track track, int oldindex, int newindex) {
        this.TrackStorage!.MoveTrack(oldindex, newindex);
    }

    public TrackControlSurface GetContentObject(Track track) {
        return this.TrackStorage!.GetContentObject(track);
    }

    public bool ReleaseContentObject(Type trackType, TrackControlSurface contentControl) {
        return this.TrackStorage!.ReleaseContentObject(trackType, contentControl);
    }

    public int IndexOf(TrackControlSurfaceItem item) {
        return this.TrackStorage!.Children.IndexOf(item);
    }

    public void SelectRange(TrackControlSurfaceItem source, PointerEventArgs e) {
    }

    public void SetRangeAnchor(TrackControlSurfaceItem source, PointerEventArgs e) {
    }

    public IEnumerable<TrackControlSurfaceItem> GetTracks() {
        return this.TrackStorage!.Children.Cast<TrackControlSurfaceItem>();
    }
}