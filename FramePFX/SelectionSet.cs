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

using System.Collections;
using System.Collections.ObjectModel;
using PFXToolKitUI.Interactivity.Selections;
using PFXToolKitUI.Utils;

namespace FramePFX;

public sealed class SelectionSet<T> : IEnumerable<T> {
    private static readonly IList<T> EmptyList = ReadOnlyCollection<T>.Empty;
    private readonly HashSet<T> selectedItems;

    public int Count => this.selectedItems.Count;

    public IReadOnlySet<T> SelectedItems => this.selectedItems;

    public event EventHandler<ListSelectionModelChangedEventArgs<T>>? SelectionChanged;

    public SelectionSet() {
        this.selectedItems = new HashSet<T>();
    }

    public bool Select(T item) {
        if (this.selectedItems.Add(item)) {
            this.SelectionChanged?.Invoke(this, new ListSelectionModelChangedEventArgs<T>([item], EmptyList));
            return true;
        }

        return false;
    }

    public void SelectRange(IEnumerable<T> items) {
        EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> added = this.selectedItems.UnionAddEx(items);
            if (added.Count > 0) {
                handlers(this, new ListSelectionModelChangedEventArgs<T>(added.AsReadOnly(), EmptyList));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            foreach (T item in items) {
                this.selectedItems.Add(item);
            }
        }
    }

    public void SelectRange(IList<T> items, int index, int count) {
        EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> added = new List<T>(count);
            for (int i = 0; i < count; i++) {
                T item = items[index + i];
                if (this.selectedItems.Add(item)) {
                    added.Add(item);
                }
            }

            if (added.Count > 0) {
                handlers(this, new ListSelectionModelChangedEventArgs<T>(added.AsReadOnly(), EmptyList));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            for (int i = 0; i < count; i++) {
                this.selectedItems.Add(items[index + i]);
            }
        }
    }

    public bool Deselect(T item) {
        if (this.selectedItems.Remove(item)) {
            this.SelectionChanged?.Invoke(this, new ListSelectionModelChangedEventArgs<T>(EmptyList, [item]));
            return true;
        }

        return false;
    }

    public void DeselectRange(IEnumerable<T> items) {
        EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> removed = this.selectedItems.UnionRemoveEx(items);
            if (removed.Count > 0) {
                handlers(this, new ListSelectionModelChangedEventArgs<T>(EmptyList, removed.AsReadOnly()));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            foreach (T item in items) {
                this.selectedItems.Remove(item);
            }
        }
    }

    public void DeselectRange(IList<T> items, int index, int count) {
        EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> removed = new List<T>(count);
            for (int i = 0; i < count; i++) {
                T item = items[index + i];
                if (this.selectedItems.Remove(item)) {
                    removed.Add(item);
                }
            }

            if (removed.Count > 0) {
                handlers(this, new ListSelectionModelChangedEventArgs<T>(EmptyList, removed.AsReadOnly()));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            for (int i = 0; i < count; i++) {
                this.selectedItems.Remove(items[index + i]);
            }
        }
    }

    public bool IsSelected(T item) => this.selectedItems.Contains(item);

    public void DeselectAll() {
        if (this.selectedItems.Count > 0) {
            EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
            if (handlers != null) {
                List<T> changedList = this.selectedItems.ToList();
                this.selectedItems.Clear();
                handlers(this, new ListSelectionModelChangedEventArgs<T>(EmptyList, changedList.AsReadOnly()));
            }
            else {
                this.selectedItems.Clear();
            }
        }
    }

    public void SetSelection(T item) {
        this.DeselectAll();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<T> items) {
        this.DeselectAll();
        this.SelectRange(items);
    }

    /// <summary>
    /// Toggles the selected state of the item
    /// </summary>
    /// <param name="item">The item to toggle the selection state of</param>
    /// <returns>The new selection state</returns>
    public bool ToggleSelected(T item) {
        if (this.Deselect(item)) {
            return false;
        }
        else {
            return this.Select(item); // assert true
        }
    }

    public IEnumerator<T> GetEnumerator() => this.selectedItems.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.selectedItems.GetEnumerator();
}