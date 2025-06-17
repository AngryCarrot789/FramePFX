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
using FramePFX.Editing.UI;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Utils;

namespace FramePFX.Avalonia.Editing.Timelines.Selection;

/// <summary>
/// A selection manager which manages the selected tracks in a timeline
/// </summary>
public class TrackSelectionManager : ISelectionManager<ITrackElement>, ILightSelectionManager<ITrackElement> {
    private readonly TimelineControl timeline;
    private readonly List<TimelineControl.TrackElementImpl> refToAllTrackElements;
    private readonly HashSet<TimelineControl.TrackElementImpl> selectedElements;

    public IEnumerable<ITrackElement> SelectedItems { get; }

    public int Count => this.selectedElements.Count;

    public event SelectionChangedEventHandler<ITrackElement>? SelectionChanged;
    public event SelectionClearedEventHandler<ITrackElement>? SelectionCleared;
    public event LightSelectionChangedEventHandler<ITrackElement>? LightSelectionChanged;

    internal TrackSelectionManager(TimelineControl timeline, List<TimelineControl.TrackElementImpl> refToAllTrackElements) {
        this.timeline = timeline;
        this.refToAllTrackElements = refToAllTrackElements;
        this.SelectedItems = this.selectedElements = new HashSet<TimelineControl.TrackElementImpl>();
    }

    /// <summary>
    /// Updates the UI selection states based on what is selected in this selection manager
    /// </summary>
    public void UpdateSelection() {
        foreach (TimelineControl.TrackElementImpl track in this.selectedElements) {
            this.SetSelectionState(track, true);
        }
    }

    /// <summary>
    /// Updates the UI selection state based on if the track is currently selected in this selection manager
    /// </summary>
    /// <param name="track"></param>
    public void UpdateSelection(ITrackElement track) => this.SetSelectionState((TimelineControl.TrackElementImpl) track, null);

    /// <summary>
    /// Force update the selection state of the given element's underlying tracks
    /// UI elements with the new value, or automatically calculate it
    /// </summary>
    /// <param name="track">The track</param>
    /// <param name="isSelected">The new selection state. Null to figure out automatically</param>
    private void SetSelectionState(TimelineControl.TrackElementImpl track, bool? isSelected) {
        track.UpdateSelected(isSelected ?? this.selectedElements.Contains(track));
    }

    // OLD: before converting list box into TemplatedControl + panel for manual control over everything
    // private void OnSurfaceSelectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    //      switch (e.Action) {
    //         case NotifyCollectionChangedAction.Add: this.ProcessTreeSelection(null, e.NewItems ?? null); break;
    //         case NotifyCollectionChangedAction.Remove: this.ProcessTreeSelection(e.OldItems, null); break;
    //         case NotifyCollectionChangedAction.Replace: this.ProcessTreeSelection(e.OldItems, e.NewItems ?? null); break;
    //         case NotifyCollectionChangedAction.Reset: this.OnSelectionCleared(); break;
    //         case NotifyCollectionChangedAction.Move: break;
    //         default: throw new ArgumentOutOfRangeException();
    //     }
    // }
    // internal void ProcessTreeSelection(IList? oldItems, IList? newItems) {
    //     ReadOnlyCollection<ITrackElement>? oldList = GetList(oldItems);
    //     ReadOnlyCollection<ITrackElement>? newList = GetList(newItems);
    //     if (oldList?.Count > 0 || newList?.Count > 0) {
    //         this.OnSelectionChanged(oldList, newList);
    //     }
    // }
    // private void OnSelectionChanged(ReadOnlyCollection<ITrackElement>? oldList, ReadOnlyCollection<ITrackElement>? newList) {
    //     if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
    //         return;
    //     }
    //
    //     if (oldList != null)
    //         this.UpdateSelection(oldList, false);
    //     if (newList != null)
    //         this.UpdateSelection(newList, true);
    //
    //     this.SelectionChanged?.Invoke(this, oldList, newList);
    //     this.LightSelectionChanged?.Invoke(this);
    // }

    public bool IsSelected(ITrackElement item) {
        return this.selectedElements.Contains((TimelineControl.TrackElementImpl) item);
    }

    public void SetSelection(ITrackElement item) {
        this.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<ITrackElement> items) {
        this.Clear();
        this.Select(items);
    }

    private bool SelectInternal(ITrackElement item) {
        TimelineControl.TrackElementImpl theTrack = (TimelineControl.TrackElementImpl) item;
        if (this.selectedElements.Add(theTrack)) {
            this.SetSelectionState(theTrack, true);
            return true;
        }

        return false;
    }

    private bool UnselectInternal(ITrackElement item) {
        TimelineControl.TrackElementImpl theTrack = (TimelineControl.TrackElementImpl) item;
        if (this.selectedElements.Remove(theTrack)) {
            this.SetSelectionState(theTrack, false);
            return true;
        }

        return false;
    }

    public void Select(ITrackElement item) {
        if (this.SelectInternal(item)) {
            this.OnSelectionChanged(null, new SingletonList<ITrackElement>(item));
        }
    }

    public void Select(IEnumerable<ITrackElement> items) {
        List<ITrackElement> list = new List<ITrackElement>();
        foreach (ITrackElement item in items.ToList()) {
            if (this.SelectInternal(item))
                list.Add(item);
        }

        this.OnSelectionChanged(null, list.AsReadOnly());
    }

    public void Unselect(ITrackElement item) {
        if (this.UnselectInternal(item))
            this.OnSelectionChanged(new SingletonList<ITrackElement>(item), null);
    }

    public void Unselect(IEnumerable<ITrackElement> items) {
        List<ITrackElement> list = new List<ITrackElement>();
        foreach (ITrackElement element in items.ToList()) {
            if (this.UnselectInternal(element))
                list.Add(element);
        }

        this.OnSelectionChanged(list.AsReadOnly(), null);
    }

    public void ToggleSelected(ITrackElement item) {
        TimelineControl.TrackElementImpl theTrack = (TimelineControl.TrackElementImpl) item;
        if (this.selectedElements.Add(theTrack)) { // Added successfully, so say it's selected
            this.SetSelectionState(theTrack, true);
            this.OnSelectionChanged(null, new SingletonList<ITrackElement>(theTrack));
        }
        else { // Already added, so deselect
            this.selectedElements.Remove(theTrack);
            this.SetSelectionState(theTrack, false);
            this.OnSelectionChanged(new SingletonList<ITrackElement>(theTrack), null);
        }
    }

    public void Clear() {
        this.selectedElements.Clear();
        this.OnSelectionCleared();
    }

    public void SelectAll() {
        this.Select(this.refToAllTrackElements);
    }

    private void OnSelectionChanged(IList<ITrackElement>? oldList, IList<ITrackElement>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            return;
        }

        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }

    private void OnSelectionCleared() {
        foreach (TimelineControl.TrackElementImpl track in this.timeline.myTrackElements) {
            this.SetSelectionState(track, false);
        }

        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }

    internal void InternalOnTrackRemoving(TimelineTrackControl control) {
        if (!(control.TrackElement is ITrackElement element))
            throw new Exception("Track control has no element associated");

        this.Unselect(element);
    }
}