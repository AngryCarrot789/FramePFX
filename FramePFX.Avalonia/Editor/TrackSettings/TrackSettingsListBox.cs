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
using Avalonia;
using Avalonia.Controls;
using FramePFX.Editing;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Avalonia.AvControls.ListBoxes;
using Track = FramePFX.Editing.Track;

namespace FramePFX.Avalonia.Editor.TrackSettings;

public class TrackSettingsListBox : ModelBasedListBox<Track> {
    public static readonly StyledProperty<TimelineViewState?> TimelineProperty = AvaloniaProperty.Register<TrackSettingsListBox, TimelineViewState?>(nameof(Timeline));

    protected override Type StyleKeyOverride => typeof(ListBox);

    public TimelineViewState? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    protected override bool CanDragItemPositionCore => this.Timeline != null;

    public TrackSettingsListBox() : base(8) {
        this.CanDragItemPosition = true;
    }

    static TrackSettingsListBox() {
        TimelineProperty.Changed.AddClassHandler<TrackSettingsListBox, TimelineViewState?>((s, e) => s.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override ModelBasedListBoxItem<Track> CreateItem() => new TrackSettingsListBoxItem();

    private void OnTimelineChanged(TimelineViewState? oldTimeline, TimelineViewState? newTimeline) {
        if (oldTimeline != null) {
            this.ClearModels();
            oldTimeline.Timeline.TrackAdded -= this.TimelineOnTrackAdded;
            oldTimeline.Timeline.TrackRemoved -= this.TimelineOnTrackRemoved;
            oldTimeline.Timeline.TrackMoved -= this.TimelineOnTrackMoved;
        }

        if (newTimeline != null) {
            this.AddModels(newTimeline.Timeline.Tracks);
            newTimeline.Timeline.TrackAdded += this.TimelineOnTrackAdded;
            newTimeline.Timeline.TrackRemoved += this.TimelineOnTrackRemoved;
            newTimeline.Timeline.TrackMoved += this.TimelineOnTrackMoved;
        }
    }

    private void TimelineOnTrackAdded(object? sender, TrackAddedOrRemovedEventArgs e) {
        this.InsertModelAt(e.Index, e.Track);
    }

    private void TimelineOnTrackRemoved(object? sender, TrackAddedOrRemovedEventArgs e) {
        this.RemoveModelAt(e.Index);
    }

    private void TimelineOnTrackMoved(object? sender, TrackMovedEventArgs e) {
        this.MoveModel(e.OldIndex, e.NewIndex);
    }

    protected override void MoveItemIndexOverride(int oldIndex, int newIndex) {
        base.MoveItemIndexOverride(oldIndex, newIndex);
        this.Timeline?.Timeline.MoveTrack(oldIndex, newIndex);
    }
}