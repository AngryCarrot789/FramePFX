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
using Avalonia;
using Avalonia.Controls;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.Editing.Timelines.TrackSurfacesOLD;

/// <summary>
/// The list box control which stores the track control surface items
/// </summary>
public class TrackControlSurfaceListBox : ListBox {
    private readonly Stack<TrackControlSurfaceListBoxItem> cachedItems;
    private readonly Dictionary<Type, Stack<TrackControlSurface>> itemContentCacheMap;
    private readonly ModelControlDictionary<Track, TrackControlSurfaceListBoxItem> itemMap = new ModelControlDictionary<Track, TrackControlSurfaceListBoxItem>();

    public static readonly StyledProperty<Timeline?> TimelineProperty = AvaloniaProperty.Register<TrackControlSurfaceListBox, Timeline?>(nameof(Timeline));

    public IModelControlDictionary<Track, TrackControlSurfaceListBoxItem> ItemMap => this.itemMap;
    
    public Timeline? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }
    
    public TimelineControl? TimelineControl { get; set; }

    public TrackControlSurfaceListBox() {
        this.SelectionMode = SelectionMode.Multiple;
        this.cachedItems = new Stack<TrackControlSurfaceListBoxItem>();
        this.itemContentCacheMap = new Dictionary<Type, Stack<TrackControlSurface>>();
    }

    static TrackControlSurfaceListBox() {
        TimelineProperty.Changed.AddClassHandler<TrackControlSurfaceListBox, Timeline?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    public TrackControlSurfaceListBoxItem GetTrack(int index) => (TrackControlSurfaceListBoxItem) this.Items[index]!;
    
    private void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline == newTimeline)
            return;
        if (oldTimeline != null) {
            oldTimeline.TrackAdded -= this.OnTrackAdded;
            oldTimeline.TrackRemoved -= this.OnTrackRemoved;
            oldTimeline.TrackMoved -= this.OnTrackMoved;
            for (int i = this.Items.Count - 1; i >= 0; i--) {
                this.RemoveTrackInternal(i);
            }
        }

        if (newTimeline != null) {
            newTimeline.TrackAdded += this.OnTrackAdded;
            newTimeline.TrackRemoved += this.OnTrackRemoved;
            newTimeline.TrackMoved += this.OnTrackMoved;

            DataManager.SetContextData(this, new ContextData().Set(DataKeys.TimelineKey, newTimeline));

            int i = 0;
            foreach (Track track in newTimeline.Tracks) {
                this.InsertTrackInternal(track, i++);
            }
        }
        else {
            DataManager.ClearContextData(this);
        }
    }

    private void OnTrackAdded(Timeline timeline, Track track, int index) {
        this.InsertTrackInternal(track, index);
    }

    private void OnTrackRemoved(Timeline timeline, Track track, int index) {
        this.RemoveTrackInternal(index);
    }

    private void InsertTrackInternal(Track track, int index) {
        ItemCollection list = this.Items;
        // for (int i = list.Count - 1; i >= index; i--) {
        //     ((TrackControlSurfaceListBoxItem) list[i]!).IndexInList = i + 1;
        // }
        
        TrackControlSurfaceListBoxItem control = this.cachedItems.Count > 0 ? this.cachedItems.Pop() : new TrackControlSurfaceListBoxItem();
        this.itemMap.AddMapping(track, control);
        // control.IndexInList = index;
        control.OnAddingToList(this, track, index);
        list.Insert(index, control);
        // UpdateLayout must be called explicitly, so that the visual tree
        // can be measured, allowing templates to be applied
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAddedToList();
    }

    private void RemoveTrackInternal(int index) {
        ItemCollection list = this.Items;
        // for (int i = list.Count - 1; i > index /* not >= since we remove one at index */; i--) {
        //     ((TrackControlSurfaceListBoxItem) list[i]!).IndexInList = i - 1;
        // }
        
        TrackControlSurfaceListBoxItem control = (TrackControlSurfaceListBoxItem) list[index]!;
        this.itemMap.RemoveMapping(control.Track!, control);
        
        control.OnRemovingFromList();
        // control.IndexInList = -1;
        list.RemoveAt(index);
        control.OnRemovedFromList();
        if (this.cachedItems.Count < 8) {
            this.ResetControl(control);
            this.cachedItems.Push(control);
        }
    }

    private void OnTrackMoved(Timeline timeline, Track track, int oldIndex, int newIndex) {
        ItemCollection list = this.Items;
        TrackControlSurfaceListBoxItem control = (TrackControlSurfaceListBoxItem) list[oldIndex]!;
        control.OnIndexMoving(oldIndex, newIndex);
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, control);
        control.OnIndexMoved(oldIndex, newIndex);
        
        // int min = Math.Min(oldIndex, newIndex), max = Math.Max(oldIndex, newIndex);
        // for (int i = min; i <= max; i++) {
        //     ((TrackControlSurfaceListBoxItem) list[i]!).IndexInList = i;
        // }
    }

    private void ResetControl(TrackControlSurfaceListBoxItem item) {
        item.ClearValue(ListBoxItem.IsSelectedProperty);
    }

    public TrackControlSurface GetContentObject(Track track) {
        TrackControlSurface content;
        if (this.itemContentCacheMap.TryGetValue(track.GetType(), out Stack<TrackControlSurface>? stack) && stack.Count > 0) {
            content = stack.Pop();
        }
        else {
            content = TrackControlSurface.Registry.NewInstance(track);
        }

        return content;
    }

    public bool ReleaseContentObject(Type trackType, TrackControlSurface contentControl) {
        if (!this.itemContentCacheMap.TryGetValue(trackType, out Stack<TrackControlSurface>? stack)) {
            this.itemContentCacheMap[trackType] = stack = new Stack<TrackControlSurface>();
        }
        else if (stack.Count == 4) {
            return false;
        }

        stack.Push(contentControl);
        return true;
    }
}