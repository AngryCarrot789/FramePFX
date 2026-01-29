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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FramePFX.Editing;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Interactivity.Selections;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Avalonia.Editor;

public sealed partial class TimelineTrackControl : UserControl {
    public static readonly DirectProperty<TimelineTrackControl, TrackViewState?> TrackProperty = AvaloniaProperty.RegisterDirect<TimelineTrackControl, TrackViewState?>(nameof(Track), o => o.Track, (o, v) => o.Track = v);

    public TimelineTrackPanel? OwnerPanel { get; private set; }

    public TrackViewState? Track {
        get => field;
        private set => this.SetAndRaise(TrackProperty, ref field, value);
    }

    private readonly IBinder<TrackViewState> heightBinder = new EventUpdateBinder<TrackViewState>(nameof(TrackViewState.HeightChanged), b => b.Control.Height = b.Model.Height);
    
    internal TrackDragHandler DragHandler => field ??= new TrackDragHandler(this);

    public TimelineTrackControl() {
        this.InitializeComponent();
        this.PART_RenderSurface.OwnerTrack = this;
    }

    static TimelineTrackControl() {
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (!e.Properties.IsLeftButtonPressed) {
            return;
        }

        Timeline? timeline;
        TrackViewState? track = this.Track;
        if (track == null || (timeline = track.Track.Timeline) == null) {
            return;
        }

        Point mPos = e.GetPosition(this);
        TimelineViewState timelineVs = TimelineViewState.GetInstance(timeline, track.TopLevelIdentifier);

        if (e.KeyModifiers == KeyModifiers.Shift) {
            // Shift Left click. Select a rectangle of clips and tracks
            e.Handled = true;
        }
        else if (e.KeyModifiers == KeyModifiers.Control) {
            // CTRL Left click. Toggle selected track or clip
            if (this.PART_RenderSurface.TryGetHitClip(mPos, out Clip? clip)) {
                track.SelectedClips.ToggleSelected(clip);
                e.Handled = true;
            }
            else {
                timelineVs.SelectedTracks.ToggleSelected(track.Track);
            }
        }
        else {
            // Left click
            timelineVs.SelectedTracks.SetSelection(track.Track);

            foreach (Track otherTrack in timeline.Tracks) {
                TrackViewState.GetInstance(otherTrack, track.TopLevelIdentifier).SelectedClips.DeselectAll();
            }

            if (this.PART_RenderSurface.TryGetHitClip(mPos, out Clip? hitClip)) {
                track.SelectedClips.Select(hitClip);
                e.Handled = true;
            }

            this.DragHandler.OnLeftPointerPressed(mPos, hitClip, e);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        this.DragHandler.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        this.DragHandler.OnPointerReleased(e);
    }

    private void SelectedClipsOnCollectionChanged(object? sender, ListSelectionModelChangedEventArgs<Clip> e) {
        this.OnClipSelectionChanged(e.RemovedItems, e.AddedItems);
    }

    public void OnClipSelectionChanged(IList<Clip> removedClips, IList<Clip> addedClips) {
        this.PART_RenderSurface.OnClipSelectionChanged(removedClips, addedClips);
    }

    public void InvalidateTrackRender() {
        this.PART_RenderSurface.InvalidateVisual();
    }

    public void OnZoomChanged(ValueChangedEventArgs<double> e) {
    }
    
    public void OnHorizontalScrollChanged(ValueChangedEventArgs<TimeSpan> e) {
    }
    
    internal void OnConnecting(TimelineTrackPanel panel, TrackViewState track) {
        this.OwnerPanel = panel;
        this.Track = track;
        this.PART_RenderSurface.Track = track;
    }

    internal void OnConnected() {
        this.heightBinder.Attach(this, this.Track!);
        this.Track!.SelectedClips.SelectionChanged += this.SelectedClipsOnCollectionChanged;
    }

    internal void OnRemoving() {
        this.heightBinder.Detach();
        this.Track!.SelectedClips.SelectionChanged -= this.SelectedClipsOnCollectionChanged;
    }

    internal void OnRemoved() {
        this.Track = null;
        this.OwnerPanel = null;
        this.PART_RenderSurface.Track = null;
    }
}