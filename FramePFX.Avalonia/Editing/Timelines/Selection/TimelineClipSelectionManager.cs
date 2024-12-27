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
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.BaseFrontEnd;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Editing.Timelines.Selection;

/// <summary>
/// A selection manager which manages the selected clips in a timeline (across all tracks)
/// </summary>
public class TimelineClipSelectionManager : ISelectionManager<IClipElement>, ILightSelectionManager<IClipElement> {
    private readonly IModelControlDictionary<Track, TimelineTrackControl> trackMap;
    private readonly HashSet<IClipElement> selectedClipSet;

    /// <summary>
    /// Gets this track control, whose templated is assumed to be fully loaded
    /// </summary>
    public TimelineControl Timeline { get; }

    public IEnumerable<TimelineTrackControl> TrackControls => this.Timeline.TrackStorage!.GetTracks();

    public IEnumerable<TimelineClipControl> ClipControls => this.TrackControls.SelectMany(x => x.ClipStoragePanel!.GetClips());

    public IEnumerable<IClipElement> SelectedItems => this.selectedClipSet;

    public int Count => this.selectedClipSet.Count;

    public event SelectionChangedEventHandler<IClipElement>? SelectionChanged;
    public event SelectionClearedEventHandler<IClipElement>? SelectionCleared;
    private LightSelectionChangedEventHandler<IClipElement>? LightSelectionChanged;

    event LightSelectionChangedEventHandler<IClipElement>? ILightSelectionManager<IClipElement>.SelectionChanged {
        add => this.LightSelectionChanged += value;
        remove => this.LightSelectionChanged -= value;
    }

    private bool isBatching;
    private List<IClipElement>? batchClips_old;
    private List<IClipElement>? batchClips_new;

    public TimelineClipSelectionManager(TimelineControl timeline) {
        Validate.NotNull(timeline);
        this.trackMap = timeline.TrackStorage?.ItemMap ?? throw new InvalidOperationException("Timeline control template not initialised");
        this.Timeline = timeline;
        this.selectedClipSet = new HashSet<IClipElement>();

        // Preloaded tracks
        foreach (TimelineTrackControl track in this.TrackControls) {
            this.InternalOnTrackAdded(track);
        }
    }

    public void InternalOnTrackAdded(TimelineTrackControl control) {
        control.SelectionManager.SelectionChanged += this.OnTrackSelectionChanged;
        control.SelectionManager.SelectionCleared += this.OnTrackSelectionCleared;
    }

    public void InternalOnTrackRemoving(TimelineTrackControl control) {
        control.SelectionManager.SelectionChanged -= this.OnTrackSelectionChanged;
        control.SelectionManager.SelectionCleared -= this.OnTrackSelectionCleared;
    }

    private void OnTrackSelectionChanged(ISelectionManager<IClipElement> sender, IList<IClipElement>? oldItems, IList<IClipElement>? newItems) {
        if (this.isBatching) {
            // Batch them into one final event that will get called after isBatching is set to false
            if (newItems != null && newItems.Count > 0)
                (this.batchClips_new ??= new List<IClipElement>()).AddRange(newItems);
            if (oldItems != null && oldItems.Count > 0)
                (this.batchClips_old ??= new List<IClipElement>()).AddRange(oldItems);
        }
        else {
            this.OnSelectionChanged(oldItems, newItems);
        }
    }

    private void OnTrackSelectionCleared(ISelectionManager<IClipElement> sender) {
        if (!this.isBatching) {
            List<IClipElement> deselected = new List<IClipElement>();
            TimelineTrackControl track = ((ClipSelectionManager) sender).Track;
            foreach (TimelineClipControl clip in track.ClipStoragePanel!.GetClips()) {
                if (this.selectedClipSet.Remove(clip))
                    deselected.Add(clip);
            }

            this.OnSelectionChanged(GetList(deselected), null);
        }
    }

    public bool IsSelected(IClipElement item) {
        return item.TrackUI.Selection.IsSelected(item);
    }

    public void SetSelection(IClipElement item) {
        this.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<IClipElement> items) {
        this.Clear();
        this.Select(items);
    }

    public void Select(IClipElement item) {
        item.TrackUI.Selection.Select(item);
    }

    // maybe Select(IEnumerable<(Track, List<IClipElement>)>)?

    public void Select(IEnumerable<IClipElement> items) {
        this.DoBatchedEvent(items, (m, c) => m.Select(c));
    }

    public void Unselect(IClipElement item) {
        item.TrackUI.Selection.Unselect(item);
    }

    public void Unselect(IEnumerable<IClipElement> items) {
        this.DoBatchedEvent(items, (m, c) => m.Unselect(c));
    }

    public void ToggleSelected(IClipElement item) {
        item.TrackUI.Selection.ToggleSelected(item);
    }

    private void DoBatchedEvent(IEnumerable<IClipElement> items, Action<ISelectionManager<IClipElement>, IClipElement> action) {
        if (this.isBatching)
            throw new InvalidOperationException("Already batching");

        try {
            this.isBatching = true;
            foreach (IClipElement clip in items) {
                // TODO: could optimise this to figure out a list of clips per track to select
                action(clip.TrackUI.Selection, clip);
            }
        }
        finally {
            this.isBatching = false;
        }

        try {
            this.OnSelectionChanged(GetList(this.batchClips_old), GetList(this.batchClips_new));
        }
        finally {
            this.batchClips_old?.Clear();
            this.batchClips_new?.Clear();
        }
    }

    public void Clear() {
        if (this.Count < 1) {
            return;
        }

        try {
            this.isBatching = true;
            foreach (TimelineTrackControl track in this.TrackControls) {
                track.SelectionManager.Clear();
            }
        }
        finally {
            this.isBatching = false;
        }

        this.selectedClipSet.Clear();
        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }

    public void SelectAll() {
        try {
            this.isBatching = true;
            foreach (TimelineTrackControl track in this.TrackControls) {
                track.SelectionManager.SelectAll();
            }
        }
        finally {
            this.isBatching = false;
        }

        try {
            this.OnSelectionChanged(GetList(this.batchClips_old), GetList(this.batchClips_new));
        }
        finally {
            this.batchClips_old?.Clear();
            this.batchClips_new?.Clear();
        }
    }

    private void OnSelectionChanged(IList<IClipElement>? oldList, IList<IClipElement>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            // Skip if there's no changes
            return;
        }

        if (oldList != null)
            foreach (IClipElement clip in oldList)
                this.selectedClipSet.Remove(clip);

        if (newList != null)
            foreach (IClipElement clip in newList)
                this.selectedClipSet.Add(clip);

        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }

    private static ReadOnlyCollection<IClipElement>? GetList(List<IClipElement>? list) => list == null || list.Count < 1 ? null : list.AsReadOnly();
}