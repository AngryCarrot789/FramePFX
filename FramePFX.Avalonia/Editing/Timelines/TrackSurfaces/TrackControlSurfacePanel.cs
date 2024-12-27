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
using FramePFX.BaseFrontEnd;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Tracks;

namespace FramePFX.Avalonia.Editing.Timelines.TrackSurfaces;

public class TrackControlSurfacePanel : Panel {
    private readonly ModelControlDictionary<Track, TrackControlSurfaceItem> itemMap;
    private readonly Dictionary<Type, Stack<TrackControlSurface>> itemContentCacheMap;
    private readonly Stack<TrackControlSurfaceItem> cachedItems;

    public IModelControlDictionary<Track, TrackControlSurfaceItem> ItemMap => this.itemMap;

    public TrackControlSurfaceList? Owner { get; set; }

    public TrackControlSurfacePanel() {
        this.cachedItems = new Stack<TrackControlSurfaceItem>();
        this.itemMap = new ModelControlDictionary<Track, TrackControlSurfaceItem>();
        this.itemContentCacheMap = new Dictionary<Type, Stack<TrackControlSurface>>();
    }

    public void InsertTrack(Track track, int index) {
        Controls list = this.Children;
        // for (int i = list.Count - 1; i >= index; i--) {
        //     ((TrackControlSurfaceListBoxItem) list[i]!).IndexInList = i + 1;
        // }

        TrackControlSurfaceItem control = this.cachedItems.Count > 0 ? this.cachedItems.Pop() : new TrackControlSurfaceItem();
        this.itemMap.AddMapping(track, control);
        // control.IndexInList = index;
        control.OnAddingToList(this.Owner!, track, index);
        list.Insert(index, control);
        // UpdateLayout must be called explicitly, so that the visual tree
        // can be measured, allowing templates to be applied
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAddedToList();
    }

    public void RemoveTrack(int index) {
        Controls list = this.Children;
        // for (int i = list.Count - 1; i > index /* not >= since we remove one at index */; i--) {
        //     ((TrackControlSurfaceListBoxItem) list[i]!).IndexInList = i - 1;
        // }

        TrackControlSurfaceItem control = (TrackControlSurfaceItem) list[index]!;
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

    public void MoveTrack(int oldIndex, int newIndex) {
        Controls list = this.Children;
        TrackControlSurfaceItem control = (TrackControlSurfaceItem) list[oldIndex]!;
        control.OnIndexMoving(oldIndex, newIndex);
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, control);
        control.OnIndexMoved(oldIndex, newIndex);

        // int min = Math.Min(oldIndex, newIndex), max = Math.Max(oldIndex, newIndex);
        // for (int i = min; i <= max; i++) {
        //     ((TrackControlSurfaceListBoxItem) list[i]!).IndexInList = i;
        // }
    }

    private void ResetControl(TrackControlSurfaceItem control) {
        TrackControlSurfaceItem.InternalSetIsSelected(control, false);
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

    protected override Size MeasureOverride(Size availableSize) {
        Size size = new Size();
        Controls items = this.Children;
        int count = items.Count;
        for (int i = 0; i < count; i++) {
            Control element = items[i];
            element.Measure(availableSize);
            Size elemSz = element.DesiredSize;
            size = new Size(Math.Max(size.Width, elemSz.Width), size.Height + elemSz.Height);
        }

        if (count > 1) {
            size = size.WithHeight(size.Height + (count - 1));
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize) {
        Controls items = this.Children;
        int count = items.Count;
        double rectY = 0.0;
        double num = 0.0;
        for (int i = 0; i < count; ++i) {
            Control element = items[i];
            rectY += num;
            num = element.DesiredSize.Height;
            element.Arrange(new Rect(0, rectY, Math.Max(finalSize.Width, element.DesiredSize.Width), num));
            rectY += 1;
        }

        return finalSize;
    }

    public void ClearTracks() {
        for (int i = this.Children.Count - 1; i >= 0; i--) {
            this.RemoveTrack(i);
        }
    }

    public void LoadTracks(Timeline timeline) {
        int i = 0;
        foreach (Track track in timeline.Tracks) {
            this.InsertTrack(track, i++);
        }
    }
}