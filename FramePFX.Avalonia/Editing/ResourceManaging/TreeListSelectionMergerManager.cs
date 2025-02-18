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
using System.Linq;
using FramePFX.Avalonia.Editing.ResourceManaging.Lists;
using FramePFX.Avalonia.Editing.ResourceManaging.Trees;
using FramePFX.Editing.ResourceManaging;
using PFXToolKitUI.Interactivity;

namespace FramePFX.Avalonia.Editing.ResourceManaging;

public class TreeListSelectionMergerManager : IResourceSelectionManager {
    public ResourceExplorerListBox ListBox { get; }

    public ResourceTreeView TreeView { get; }

    public ISelectionManager<BaseResource> Tree => this.TreeView.SelectionManager;

    public ISelectionManager<BaseResource> List => this.ListBox.SelectionManager;

    public bool SyncTreeWithList {
        get => this.syncTreeWithList;
        set {
            if (this.syncTreeWithList == value) {
                return;
            }

            this.syncTreeWithList = value;
            if (value) {
                this.SyncImmediateListSelection();
            }
        }
    }

    private bool isUpdatingList, isUpdatingTree;
    private bool syncTreeWithList = true;

    public TreeListSelectionMergerManager(ResourceExplorerListBox listBox, ResourceTreeView treeView) {
        this.ListBox = listBox;
        this.TreeView = treeView;

        listBox.SelectionManager.SelectionChanged += this.OnListBoxSelectionChanged;
        listBox.SelectionManager.SelectionCleared += this.OnListBoxSelectionCleared;
        treeView.SelectionManager.SelectionChanged += this.OnTreeViewSelectionChanged;
        treeView.SelectionManager.SelectionCleared += this.OnTreeViewSelectionCleared;
        this.SyncImmediateListSelection();
    }

    private void SyncImmediateListSelection() {
        if (this.isUpdatingList) {
            return;
        }

        this.isUpdatingList = true;

        try {
            if (this.ListBox.SelectionManager.Count > 0)
                this.ListBox.SelectionManager.Clear();

            if (this.Tree.Count > 0)
                this.ListBox.SelectionManager.Select(this.Tree.SelectedItems.ToList());
        }
        finally {
            this.isUpdatingList = false;
        }
    }

    private void OnListBoxSelectionCleared(ISelectionManager<BaseResource> sender) {
        if (this.isUpdatingList)
            return;

        try {
            this.isUpdatingTree = true;
            this.TreeView.SelectionManager.Clear();
        }
        finally {
            this.isUpdatingTree = false;
        }
    }

    private void OnTreeViewSelectionCleared(ISelectionManager<BaseResource> sender) {
        if (this.isUpdatingTree) {
            return;
        }

        try {
            this.isUpdatingList = true;
            this.ListBox.SelectionManager.Clear();
        }
        finally {
            this.isUpdatingList = false;
        }
    }

    private void OnListBoxSelectionChanged(ISelectionManager<BaseResource> sender, IList<BaseResource>? oldItems, IList<BaseResource>? newItems) {
        if (this.isUpdatingList) {
            return;
        }

        try {
            this.isUpdatingTree = true;
            if (oldItems != null && oldItems.Count > 0)
                this.TreeView.SelectionManager.Unselect(oldItems);
            if (newItems != null && newItems.Count > 0)
                this.TreeView.SelectionManager.Select(newItems);
        }
        finally {
            this.isUpdatingTree = false;
        }
    }

    private void OnTreeViewSelectionChanged(ISelectionManager<BaseResource> sender, IList<BaseResource>? oldItems, IList<BaseResource>? newItems) {
        if (this.isUpdatingTree) {
            return;
        }

        try {
            this.isUpdatingList = true;
            if (oldItems != null && oldItems.Count > 0)
                this.ListBox.SelectionManager.Unselect(oldItems);
            if (newItems != null && newItems.Count > 0)
                this.ListBox.SelectionManager.Select(newItems);
        }
        finally {
            this.isUpdatingList = false;
        }
    }
}