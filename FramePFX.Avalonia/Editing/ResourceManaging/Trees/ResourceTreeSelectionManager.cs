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
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Interactivity;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Trees;

public class ResourceTreeSelectionManager : ISelectionManager<BaseResource>, ILightSelectionManager<BaseResource> {
    private ResourceTreeView? tree;

    public ResourceTreeView? Tree {
        get => this.tree;
        set {
            ResourceTreeView? oldTree = this.tree;
            ReadOnlyCollection<BaseResource>? oldItems = null;
            INotifyCollectionChanged? listener;
            if (oldTree != null) {
                listener = oldTree.SelectedItems as INotifyCollectionChanged;
                if (value == null) {
                    // Tree is being set to null; clear selection first
                    oldTree.SelectedItems.Clear();
                    if (listener != null)
                        listener.CollectionChanged -= this.OnSelectionCollectionChanged;

                    this.tree = null;
                    return;
                }

                // Since there's an old and new tree, we need to first say cleared then selection
                // changed from old selection to new selection, even if they're the exact same
                if ((oldItems = ProcessList(ControlToModelList(oldTree).ToList())) != null && this.KeepSelectedItemsFromOldTree)
                    oldTree.SelectedItems.Clear();

                if (listener != null)
                    listener.CollectionChanged -= this.OnSelectionCollectionChanged;
            }

            this.tree = value;
            if (value != null) {
                if ((listener = value.SelectedItems as INotifyCollectionChanged) != null)
                    listener.CollectionChanged += this.OnSelectionCollectionChanged;

                if (this.KeepSelectedItemsFromOldTree) {
                    if (oldItems != null)
                        this.Select(oldItems);
                }
                else {
                    ReadOnlyCollection<BaseResource>? newItems = ProcessList(ControlToModelList(value).ToList());
                    this.OnSelectionChanged(oldItems, newItems);
                }
            }
        }
    }

    public int Count => this.tree?.SelectedItems.Count ?? 0;

    /// <summary>
    /// Specifies whether to move the old tree's selected items to the new tree when our <see cref="Tree"/> property changes. True by default.
    /// <br/>
    /// <para>
    /// When true, the old tree's items are saved then the tree is cleared, and the new tree's selection becomes that saved list
    /// </para>
    /// <para>
    /// When false, the <see cref="SelectionCleared"/> event is raised (if the old tree is valid) and then the selection changed event is raised on the new tree's pre-existing selected items.
    /// </para>
    /// </summary>
    public bool KeepSelectedItemsFromOldTree { get; set; } = true;

    public IEnumerable<BaseResource> SelectedItems => this.tree != null ? ControlToModelList(this.tree).ToList() : ImmutableArray<BaseResource>.Empty;

    public event SelectionChangedEventHandler<BaseResource>? SelectionChanged;
    public event SelectionClearedEventHandler<BaseResource>? SelectionCleared;

    private LightSelectionChangedEventHandler<BaseResource>? LightSelectionChanged;

    event LightSelectionChangedEventHandler<BaseResource>? ILightSelectionManager<BaseResource>.SelectionChanged {
        add => this.LightSelectionChanged += value;
        remove => this.LightSelectionChanged -= value;
    }

    public ResourceTreeSelectionManager() {
    }

    public ResourceTreeSelectionManager(ResourceTreeView treeView) {
        this.Tree = treeView;
    }

    private void OnSelectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        switch (e.Action) {
            case NotifyCollectionChangedAction.Add:     this.ProcessTreeSelection(null, e.NewItems ?? null); break;
            case NotifyCollectionChangedAction.Remove:  this.ProcessTreeSelection(e.OldItems, null); break;
            case NotifyCollectionChangedAction.Replace: this.ProcessTreeSelection(e.OldItems, e.NewItems ?? null); break;
            case NotifyCollectionChangedAction.Reset:
                if (this.tree != null)
                    this.OnSelectionCleared();
            break;
            case NotifyCollectionChangedAction.Move: break;
            default:                                 throw new ArgumentOutOfRangeException();
        }
    }

    internal void ProcessTreeSelection(IList? oldItems, IList? newItems) {
        ReadOnlyCollection<BaseResource>? oldList = oldItems?.Cast<ResourceTreeViewItem>().Select(x => x.Resource!).ToList().AsReadOnly();
        ReadOnlyCollection<BaseResource>? newList = newItems?.Cast<ResourceTreeViewItem>().Select(x => x.Resource!).ToList().AsReadOnly();
        if (oldList?.Count > 0 || newList?.Count > 0) {
            this.OnSelectionChanged(oldList, newList);
        }
    }

    private void OnSelectionChanged(ReadOnlyCollection<BaseResource>? oldList, ReadOnlyCollection<BaseResource>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            return;
        }

        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }

    public bool IsSelected(BaseResource item) {
        if (this.tree == null)
            return false;
        if (this.tree.ItemMap.TryGetControl(item, out ResourceTreeViewItem? treeItem))
            return treeItem.IsSelected;
        return false;
    }

    private void OnSelectionCleared() {
        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }

    public void SetSelection(BaseResource item) {
        if (this.tree == null) {
            return;
        }

        this.tree.SelectedItems.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<BaseResource> items) {
        if (this.tree == null) {
            return;
        }

        this.tree.SelectedItems.Clear();
        this.Select(items);
    }

    public void Select(BaseResource item) {
        if (this.tree == null) {
            return;
        }

        if (this.tree.ItemMap.TryGetControl(item, out ResourceTreeViewItem? treeItem)) {
            treeItem.IsSelected = true;
        }
    }

    public void Select(IEnumerable<BaseResource> items) {
        if (this.tree == null) {
            return;
        }

        foreach (BaseResource item in items.ToList()) {
            if (this.tree.ItemMap.TryGetControl(item, out ResourceTreeViewItem? treeItem)) {
                treeItem.IsSelected = true;
            }
        }
    }

    public void Unselect(BaseResource item) {
        if (this.tree == null) {
            return;
        }

        if (this.tree.ItemMap.TryGetControl(item, out ResourceTreeViewItem? treeItem)) {
            treeItem.IsSelected = false;
        }
    }

    public void Unselect(IEnumerable<BaseResource> items) {
        if (this.tree == null) {
            return;
        }

        List<BaseResource> list = items.ToList();
        foreach (BaseResource item in list) {
            if (this.tree.ItemMap.TryGetControl(item, out ResourceTreeViewItem? treeItem)) {
                treeItem.IsSelected = false;
            }
        }
    }

    public void ToggleSelected(BaseResource item) {
        if (this.IsSelected(item))
            this.Unselect(item);
        else
            this.Select(item);
    }

    public void Clear() {
        if (this.tree != null) {
            this.tree.SelectedItems.Clear();
        }
    }

    public void SelectAll() {
        this.tree?.SelectAll();
    }

    private static IEnumerable<BaseResource> ControlToModelList(ResourceTreeView tree) => tree.SelectedItems.Cast<ResourceTreeViewItem>().Select(x => x.Resource!);
    private static ReadOnlyCollection<BaseResource>? ProcessList(List<BaseResource>? list) => list != null && list.Count > 0 ? list.AsReadOnly() : null;
}