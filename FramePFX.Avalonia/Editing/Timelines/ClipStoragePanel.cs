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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Utils;
using Track = FramePFX.Editing.Timelines.Tracks.Track;

namespace FramePFX.Avalonia.Editing.Timelines;

public class ClipStoragePanel : Panel, IEnumerable<TimelineClipControl> {
    public static readonly DirectProperty<ClipStoragePanel, Track?> TrackProperty = AvaloniaProperty.RegisterDirect<ClipStoragePanel, Track?>(nameof(Track), o => o.Track);
    private readonly ModelControlDictionary<Clip, TimelineClipControl> itemMap = new ModelControlDictionary<Clip, TimelineClipControl>();

    private readonly Stack<TimelineClipControl> itemCache;

    public Track? Track => this.TrackControl?.Track;

    public TimelineTrackControl? TrackControl { get; private set; }

    public TimelineControl? TimelineControl { get; private set; }

    public IModelControlDictionary<Clip, TimelineClipControl> ItemMap => this.itemMap;
    
    public bool IsConnected { get; private set; }
    
    public TimelineClipControl this[int index] {
        get => (TimelineClipControl) this.Children[index];
    }
    
    public ClipStoragePanel() {
        this.itemCache = new Stack<TimelineClipControl>();
    }

    public void Connect(TimelineTrackControl trackControl) {
        Validate.NotNull(trackControl);

        this.TimelineControl = trackControl.TimelineControl ?? throw new InvalidOperationException("TimelineTrackControl does not have a timeline control associated");
        this.TrackControl = trackControl;
        this.IsConnected = true;

        if (trackControl.Track is Track track) {
            this.OnTrackChanged(null, track);
        }
    }

    public void OnTrackChanged(Track? oldTrack, Track? newTrack) => this.RaisePropertyChanged(TrackProperty, oldTrack, newTrack);

    public IEnumerable<TimelineClipControl> GetClips() => this.Children.Cast<TimelineClipControl>();

    public TimelineClipControl GetClipAt(int index) => (TimelineClipControl) this.Children[index];

    public void InsertClip(Clip clip, int index) {
        this.InsertClip(this.itemCache.Count > 0 ? this.itemCache.Pop() : new TimelineClipControl(), clip, index);
    }

    public void InsertClip(TimelineClipControl control, Clip clip, int index) {
        if (this.Track == null)
            throw new InvalidOperationException("Cannot insert clips without a track associated");
        this.itemMap.AddMapping(clip, control);
        control.OnConnecting(this, clip);
        this.Children.Insert(index, control);
        control.ApplyTemplate();
        control.ApplyTemplate();
        control.OnConnected();
    }

    public void RemoveClipInternal(int index, bool canCache = true) {
        TimelineClipControl control = (TimelineClipControl) this.Children[index];
        this.itemMap.RemoveMapping(control.ClipModel!, control);
        control.OnDisconnecting();
        this.Children.RemoveAt(index);
        control.OnDisconnected();
        if (canCache && this.itemCache.Count < 16)
            this.itemCache.Push(control);
    }

    public void ClearClipsInternal(bool canCache = true) {
        int count = this.Children.Count;
        for (int i = count - 1; i >= 0; i--) {
            this.RemoveClipInternal(i, canCache);
        }
    }

    protected override Size MeasureOverride(Size availableSize) {
        if (this.Track != null) {
            availableSize = new Size(availableSize.Width, this.Track.Height);
        }

        Size total = new Size();
        Controls items = this.Children;
        int count = items.Count;
        for (int i = 0; i < count; i++) {
            Control item = items[i];
            item.Measure(availableSize);
            Size size = item.DesiredSize;
            total = new Size(Math.Max(total.Width, size.Width), Math.Max(total.Height, size.Height));
        }

        return new Size(total.Width, availableSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize) {
        Controls items = this.Children;
        for (int i = 0, count = items.Count; i < count; i++) {
            TimelineClipControl clip = (TimelineClipControl) items[i];
            clip.Arrange(new Rect(clip.PixelBegin, 0, clip.PixelWidth, finalSize.Height));
        }

        return finalSize;
    }

    public IEnumerator<TimelineClipControl> GetEnumerator() => this.Children.Cast<TimelineClipControl>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}