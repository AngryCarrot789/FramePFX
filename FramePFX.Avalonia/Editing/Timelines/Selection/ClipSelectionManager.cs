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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;

namespace FramePFX.Avalonia.Editing.Timelines.Selection;

/// <summary>
/// A selection manager that manages clip selection across a single track
/// </summary>
public class ClipSelectionManager : ISelectionManager<IClipElement>, ILightSelectionManager<IClipElement> {
    private readonly HashSet<IClipElement> selectionSet;

    /// <summary>
    /// Gets this track control, whose templated is assumed to be fully loaded
    /// </summary>
    public TimelineTrackControl Track { get; }

    public IEnumerable<IClipElement> SelectedItems => this.selectionSet;

    public int Count => this.selectionSet.Count;

    public event SelectionChangedEventHandler<IClipElement>? SelectionChanged;
    public event SelectionClearedEventHandler<IClipElement>? SelectionCleared;
    private LightSelectionChangedEventHandler<IClipElement>? LightSelectionChanged;

    event LightSelectionChangedEventHandler<IClipElement>? ILightSelectionManager<IClipElement>.SelectionChanged {
        add => this.LightSelectionChanged += value;
        remove => this.LightSelectionChanged -= value;
    }

    public ClipSelectionManager(TimelineTrackControl track) {
        this.Track = track;
        this.selectionSet = new HashSet<IClipElement>();
    }

    public bool IsSelected(IClipElement item) {
        return this.selectionSet.Contains(item);
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
        if (!this.selectionSet.Add(item))
            return;

        TimelineClipControl.InternalUpdateIsSelected((TimelineClipControl) item, true);
        this.OnSelectionChanged(null, new List<IClipElement>() { item }.AsReadOnly());
    }

    public void Select(IEnumerable<IClipElement> items) {
        List<IClipElement> clips = new List<IClipElement>();
        foreach (IClipElement clipToSelect in items) {
            if (!this.selectionSet.Contains(clipToSelect))
                clips.Add(clipToSelect);
        }

        if (clips.Count > 0) {
            foreach (IClipElement clipToSelect in clips) {
                this.selectionSet.Add(clipToSelect);
                TimelineClipControl.InternalUpdateIsSelected((TimelineClipControl) clipToSelect, true);
            }

            this.OnSelectionChanged(null, clips.AsReadOnly());
        }
    }

    public void Unselect(IClipElement item) {
        if (this.selectionSet.Remove(item)) {
            TimelineClipControl.InternalUpdateIsSelected((TimelineClipControl) item, false);
            this.OnSelectionChanged(new List<IClipElement>() { item }.AsReadOnly(), null);
        }
    }

    public void Unselect(IEnumerable<IClipElement> items) {
        List<IClipElement> clips = new List<IClipElement>();
        foreach (IClipElement clipToSelect in items) {
            if (this.selectionSet.Contains(clipToSelect))
                clips.Add(clipToSelect);
        }

        if (clips.Count > 0) {
            foreach (IClipElement clipToDeselect in clips) {
                this.selectionSet.Remove(clipToDeselect);
                TimelineClipControl.InternalUpdateIsSelected((TimelineClipControl) clipToDeselect, false);
            }

            this.OnSelectionChanged(clips.AsReadOnly(), null);
        }
    }

    public void ToggleSelected(IClipElement item) => this.ToggleSelected<IClipElement>(item);

    private void OnSelectionChanged(ReadOnlyCollection<IClipElement>? oldList, ReadOnlyCollection<IClipElement>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            return;
        }

        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }

    public void Clear() {
        this.OnPreSelectionCleared();
        this.selectionSet.Clear();
        this.selectionSet.Clear();
        this.OnSelectionCleared();
    }

    public void SelectAll() {
        this.Select(this.Track.ClipStoragePanel!.GetClips());
    }

    private void OnPreSelectionCleared() {
        foreach (IClipElement control in this.selectionSet) {
            TimelineClipControl.InternalUpdateIsSelected((TimelineClipControl) control, false);
        }
    }

    private void OnSelectionCleared() {
        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }
}