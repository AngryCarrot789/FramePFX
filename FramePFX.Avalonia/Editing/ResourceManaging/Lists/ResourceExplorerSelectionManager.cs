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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Interactivity;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists;

public class ResourceExplorerSelectionManager : ISelectionManager<BaseResource>, ILightSelectionManager<BaseResource> {
    private readonly AvaloniaList<object> mySelectionList;
    
    public ResourceExplorerListBox ListBox { get; }

    public IEnumerable<BaseResource> SelectedItems => this.SelectedControls.Select(x => x.Resource!);

    public IEnumerable<ResourceExplorerListBoxItem> SelectedControls => this.mySelectionList.Cast<ResourceExplorerListBoxItem>();

    public int Count => this.mySelectionList.Count;

    public event SelectionChangedEventHandler<BaseResource>? SelectionChanged;
    public event SelectionClearedEventHandler<BaseResource>? SelectionCleared;

    private LightSelectionChangedEventHandler<BaseResource>? LightSelectionChanged;
    event LightSelectionChangedEventHandler<BaseResource>? ILightSelectionManager<BaseResource>.SelectionChanged {
        add => this.LightSelectionChanged += value;
        remove => this.LightSelectionChanged -= value;
    }
    
    private bool isBatching;
    private List<BaseResource>? batchResources_old;
    private List<BaseResource>? batchResources_new;
    
    public ResourceExplorerSelectionManager(ResourceExplorerListBox listBox) {
        this.ListBox = listBox;
        this.ListBox.SelectedItems = this.mySelectionList = new AvaloniaList<object>();
        this.mySelectionList.CollectionChanged += this.OnSelectionCollectionChanged;
    }

    private void OnSelectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                this.ProcessTreeSelection(null, e.NewItems ?? null);
                break;
            case NotifyCollectionChangedAction.Remove:
                this.ProcessTreeSelection(e.OldItems, null);
                break;
            case NotifyCollectionChangedAction.Replace:
                this.ProcessTreeSelection(e.OldItems, e.NewItems ?? null);
                break;
            case NotifyCollectionChangedAction.Reset:
                this.RaiseSelectionCleared();
                break;
            case NotifyCollectionChangedAction.Move: break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    internal void ProcessTreeSelection(IList? oldItems, IList? newItems) {
        List<BaseResource>? oldList = oldItems?.Cast<ResourceExplorerListBoxItem>().Select(x => x.Resource!).ToList();
        List<BaseResource>? newList = newItems?.Cast<ResourceExplorerListBoxItem>().Select(x => x.Resource!).ToList();
        if (this.isBatching) {
            // Batch them into one final event that will get called after isBatching is set to false
            if (newList != null && newList.Count > 0)
                (this.batchResources_new ??= new List<BaseResource>()).AddRange(newList);
            if (oldList != null && oldList.Count > 0)
                (this.batchResources_old ??= new List<BaseResource>()).AddRange(oldList);
        }
        else if (oldList?.Count > 0 || newList?.Count > 0) {
            this.RaiseSelectionChanged(GetList(oldList), GetList(newList));
        }
    }
    
    public bool IsSelected(BaseResource item) {
        return this.ListBox.ItemMap.TryGetControl(item, out ResourceExplorerListBoxItem? control) && control.IsSelected;
    }

    public void SetSelection(BaseResource item) {
        this.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<BaseResource> items) {
        this.Clear();
        this.Select(items);
    }

    public void Select(BaseResource item) {
        if (this.ListBox.ItemMap.TryGetControl(item, out ResourceExplorerListBoxItem? control)) {
            control.IsSelected = true;
        }
    }

    public void Select(IEnumerable<BaseResource> items) {
        try {
            this.isBatching = true;
            foreach (BaseResource resource in items) {
                this.Select(resource);
            }
        }
        finally {
            this.isBatching = false;
        }
        
        try {
            this.RaiseSelectionChanged(GetList(this.batchResources_old), GetList(this.batchResources_new));
        }
        finally {
            this.batchResources_old?.Clear();
            this.batchResources_new?.Clear();
        }
    }

    public void Unselect(BaseResource item) {
        if (this.ListBox.ItemMap.TryGetControl(item, out ResourceExplorerListBoxItem? control)) {
            control.IsSelected = false;
        }
    }

    public void Unselect(IEnumerable<BaseResource> items) {
        try {
            this.isBatching = true;
            foreach (BaseResource resource in items) {
                this.Unselect(resource);
            }
        }
        finally {
            this.isBatching = false;
        }
        
        try {
            this.RaiseSelectionChanged(GetList(this.batchResources_old), GetList(this.batchResources_new));
        }
        finally {
            this.batchResources_old?.Clear();
            this.batchResources_new?.Clear();
        }
    }

    public void ToggleSelected(BaseResource item) {
        if (this.ListBox.ItemMap.TryGetControl(item, out ResourceExplorerListBoxItem? control)) {
            control.IsSelected = !control.IsSelected;
        }
    }

    public void Clear() {
        this.mySelectionList.Clear();
    }
    
    public void SelectAll() {
        try {
            this.isBatching = true;
            foreach (ResourceExplorerListBoxItem control in this.ListBox.ItemMap.Controls) {
                control.IsSelected = true;
            }
        }
        finally {
            this.isBatching = false;
        }
        
        try {
            this.RaiseSelectionChanged(GetList(this.batchResources_old), GetList(this.batchResources_new));
        }
        finally {
            this.batchResources_old?.Clear();
            this.batchResources_new?.Clear();
        }
    }
    
    private void RaiseSelectionChanged(ReadOnlyCollection<BaseResource>? oldList, ReadOnlyCollection<BaseResource>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            return;
        }
        
        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }
    
    public void RaiseSelectionCleared() {
        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }
    
    private static ReadOnlyCollection<BaseResource>? GetList(List<BaseResource>? list) => list == null || list.Count < 1 ? null : list.AsReadOnly();
}