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
using Avalonia;
using Avalonia.Controls;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.Editing.Timelines.TrackSurfacesOLD;

public class TrackControlSurfaceListBoxItem : ListBoxItem {
    public static readonly DirectProperty<TrackControlSurfaceListBoxItem, Track?> TrackProperty = AvaloniaProperty.RegisterDirect<TrackControlSurfaceListBoxItem, Track?>(nameof(Track), o => o.Track);

    private Track? myTrack;
    private bool wasFocusedBeforeMoving;
    private readonly ContextData contextData;

    public Track? Track {
        get => this.myTrack;
        private set => this.SetAndRaise(TrackProperty, ref this.myTrack, value);
    }

    // public int IndexInList { get; internal set; } = -1;
    public int IndexInList => this.TrackList!.IndexFromContainer(this);
    
    public TrackControlSurfaceListBox? TrackList { get; private set; }

    public TrackControlSurfaceListBoxItem() {
        DataManager.SetContextData(this, this.contextData = new ContextData());
    }
    
    public void SetTrackElement(ITrackElement? element) {
        // don't try to invalidate if it was already removed before
        if (element == null && !this.contextData.ContainsKey(DataKeys.TrackUIKey))
            return;
        
        this.contextData.Set(DataKeys.TrackUIKey, element);
        DataManager.InvalidateInheritedContext(this);
    }

    public void OnAddingToList(TrackControlSurfaceListBox ownerList, Track track, int index) {
        this.Track = track ?? throw new ArgumentNullException(nameof(track));
        this.TrackList = ownerList;
        this.Track.HeightChanged += this.OnTrackHeightChanged;
        this.Content = ownerList.GetContentObject(track);
    }

    public void OnAddedToList() {
        TrackControlSurface control = (TrackControlSurface) this.Content!;
        control.ApplyStyling();
        control.ApplyTemplate();
        control.Connect(this);
        this.Height = this.Track!.Height;
        this.contextData.Set(DataKeys.TrackKey, this.Track);
        DataManager.InvalidateInheritedContext(this);
    }

    public void OnRemovingFromList() {
        this.Track!.HeightChanged -= this.OnTrackHeightChanged;
        TrackControlSurface content = (TrackControlSurface) this.Content!;
        content.Disconnect();
        this.Content = null;
        this.TrackList!.ReleaseContentObject(this.Track.GetType(), content);
        this.contextData.Set(DataKeys.TrackKey, null).Set(DataKeys.TrackUIKey, null);
        DataManager.InvalidateInheritedContext(this);
    }

    public void OnRemovedFromList() {
        this.TrackList = null;
        this.Track = null;
        this.CoerceValue(IsPointerOverProperty);
    }

    public void OnIndexMoving(int oldIndex, int newIndex) {
        this.wasFocusedBeforeMoving = this.IsFocused;
    }

    public void OnIndexMoved(int oldIndex, int newIndex) {
        this.Height = this.Track!.Height;
        if (this.wasFocusedBeforeMoving) {
            this.wasFocusedBeforeMoving = false;
            this.Focus();
        }
    }
    
    private void OnTrackHeightChanged(Track track) {
        this.Height = track.Height;
    }
}