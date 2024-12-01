// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of PicNetStudio.
// 
// PicNetStudio is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// PicNetStudio is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with PicNetStudio. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.Editing.Resources.Trees;

public abstract class ResourceTreeView : TreeView, IResourceTreeElement, FramePFX.Editing.ResourceManaging.UI.IResourceTreeElement {
    public static readonly StyledProperty<bool> IsDroppableTargetOverProperty = AvaloniaProperty.Register<ResourceTreeView, bool>("IsDroppableTargetOver");
    public static readonly StyledProperty<ResourceManager?> ResourceManagerProperty = AvaloniaProperty.Register<ResourceTreeView, ResourceManager?>(nameof(ResourceManager));

    private readonly ModelControlDictionary<BaseResource, ResourceTreeViewItem> itemMap = new ModelControlDictionary<BaseResource, ResourceTreeViewItem>();
    private readonly AvaloniaList<ResourceTreeViewItem> selectedItemsList;
    internal readonly Stack<ResourceTreeViewItem> itemCache;
    private bool isProcessingAsyncDrop;
    private ResourceFolder rootFolder;
    private BaseResource targetDropResourceFolder; // the drop target for DragDrop
    private IResourceTreeElement targetDropNodeFolder; // the control associated with the drop resource

    public IModelControlDictionary<BaseResource, ResourceTreeViewItem> ItemMap => this.itemMap;

    public ResourceManager? ResourceManager {
        get => this.GetValue(ResourceManagerProperty);
        set => this.SetValue(ResourceManagerProperty, value);
    }

    public bool IsDroppableTargetOver {
        get => this.GetValue(IsDroppableTargetOverProperty);
        set => this.SetValue(IsDroppableTargetOverProperty, value);
    }

    public MovedResource MovedResource { get; set; }
    
    ResourceTreeView? IResourceTreeElement.ResourceTree => this;

    ResourceTreeViewItem? IResourceTreeElement.ParentNode => null;

    BaseResource IResourceTreeElement.Resource => this.rootFolder;

    public ResourceTreeSelectionManager SelectionManager { get; }

    public ResourceTreeView() {
        this.itemCache = new Stack<ResourceTreeViewItem>();
        this.SelectedItems = this.selectedItemsList = new AvaloniaList<ResourceTreeViewItem>();
        this.SelectionManager = new ResourceTreeSelectionManager(this);
        this.Focusable = true;
        DragDrop.SetAllowDrop(this, true);
        DataManager.SetContextData(this, new ContextData().Set(DataKeys.ResourceTreeUIKey, this));
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled || e.Source is ResourceTreeViewItem) {
            return;
        }

        this.SelectionManager.Clear();
        this.Focus();
    }

    protected abstract ResourceTreeViewItem CreateTreeViewItem();

    private void MarkContainerSelected(Control container, bool selected) {
        container.SetCurrentValue(SelectingItemsControl.IsSelectedProperty, selected);
    }

    static ResourceTreeView() {
        ResourceManagerProperty.Changed.AddClassHandler<ResourceTreeView, ResourceManager?>((o, e) => o.OnResourceManagerChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        DragDrop.DragEnterEvent.AddClassHandler<ResourceTreeView>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<ResourceTreeView>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<ResourceTreeView>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<ResourceTreeView>((o, e) => o.OnDrop(e));
    }

    public ResourceTreeViewItem GetNodeAt(int index) {
        return (ResourceTreeViewItem) this.Items[index]!;
    }

    public void InsertNode(BaseResource item, int index) {
        this.InsertNode(this.GetCachedItemOrNew(), item, index);
    }

    public void InsertNode(ResourceTreeViewItem control, BaseResource layer, int index) {
        control.OnAdding(this, null, layer);
        this.Items.Insert(index, control);
        this.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNode(int index, bool canCache = true) {
        ResourceTreeViewItem control = (ResourceTreeViewItem) this.Items[index]!;
        BaseResource model = control.Resource ?? throw new Exception("Expected node to have a resource");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        this.RemoveResourceMapping(control, model);
        control.OnRemoved();
        if (canCache)
            this.PushCachedItem(control);
    }

    public void MoveNode(int oldIndex, int newIndex) {
        ResourceTreeViewItem control = (ResourceTreeViewItem) this.Items[oldIndex]!;
        this.Items.RemoveAt(oldIndex);
        this.Items.Insert(newIndex, control);
    }

    private void OnResourceManagerChanged(ResourceManager? oldManager, ResourceManager? newManager) {
        if (oldManager != null) {
            this.rootFolder = oldManager.RootContainer;
            this.rootFolder.ResourceAdded -= this.OnResourceAdded;
            this.rootFolder.ResourceRemoved -= this.OnResourceRemoved;
            this.rootFolder.ResourceMoved -= this.OnResourceMoved;
            for (int i = this.Items.Count - 1; i >= 0; i--) {
                this.RemoveNode(i);
            }
        }

        if (newManager != null) {
            this.rootFolder = newManager.RootContainer;
            this.rootFolder.ResourceAdded += this.OnResourceAdded;
            this.rootFolder.ResourceRemoved += this.OnResourceRemoved;
            this.rootFolder.ResourceMoved += this.OnResourceMoved;
            int i = 0;
            foreach (BaseResource resource in this.rootFolder.Items) {
                this.InsertNode(resource, i++);
            }
        }
    }

    private void OnResourceAdded(ResourceFolder parent, BaseResource item, int index) => this.InsertNode(item, index);

    private void OnResourceRemoved(ResourceFolder parent, BaseResource item, int index) => this.RemoveNode(index);

    private void OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) => ResourceTreeViewItem.HandleMoveEvent(this, e);

    public void AddResourceMapping(ResourceTreeViewItem control, BaseResource layer) => this.itemMap.AddMapping(layer, control);

    public void RemoveResourceMapping(ResourceTreeViewItem control, BaseResource layer) => this.itemMap.RemoveMapping(layer, control);

    public ResourceTreeViewItem GetCachedItemOrNew() {
        return this.itemCache.Count > 0 ? this.itemCache.Pop() : this.CreateTreeViewItem();
    }

    public void PushCachedItem(ResourceTreeViewItem item) {
        if (this.itemCache.Count < 128) {
            this.itemCache.Push(item);
        }
    }

    #region Drag drop

    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    private void OnDragOver(DragEventArgs e) {
        // TODO: DragDrop
        // if (this.ResourceManager != null)
        //     this.IsDroppableTargetOver = ResourceTreeViewItem.ProcessCanDragOver(this, this.ResourceManager.RootComposition, e) == true;
    }

    private void OnDragLeave(DragEventArgs e) {
        if (!this.IsPointerOver)
            this.IsDroppableTargetOver = false;
    }

    private async void OnDrop(DragEventArgs e) {
        // e.Handled = true;
        // if (this.isProcessingAsyncDrop || this.Canvas == null) {
        //     return;
        // }
        //
        // try {
        //     this.isProcessingAsyncDrop = true;
        //     if (ResourceTreeViewItem.GetDropResourceListForEvent(e, out List<BaseResource>? list, out EnumDropType effects)) {
        //         await LayerDropRegistry.DropRegistry.OnDropped(this.Canvas.RootComposition, list, effects);
        //     }
        //     else if (!await LayerDropRegistry.DropRegistry.OnDroppedNative(this.Canvas.RootComposition, new DataObjectWrapper(e.Data), effects)) {
        //         await IoC.MessageService.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
        //     }
        // }
        // finally {
        //     this.IsDroppableTargetOver = false;
        //     this.isProcessingAsyncDrop = false;
        // }
    }

    #endregion

    public void SetSelection(ResourceTreeViewItem item) {
        this.SelectedItems.Clear();
        this.SelectedItems.Add(item);
    }

    public void SetSelection(IEnumerable<ResourceTreeViewItem> items) {
        this.SelectedItems.Clear();
        foreach (ResourceTreeViewItem item in items) {
            this.SelectedItems.Add(item);
        }
    }

    public void SetSelection(List<BaseResource> modelItems) {
        this.SelectedItems.Clear();
        foreach (BaseResource item in modelItems) {
            if (this.itemMap.TryGetControl(item, out ResourceTreeViewItem? control)) {
                control.IsSelected = true;
            }
        }
    }

    public static IResourceTreeElement? FindNodeForResource(IResourceTreeElement self, BaseResource resource) {
        ResourceTreeView? root = self.ResourceTree;
        if (root != null) {
            if (root.rootFolder == resource) {
                return root;
            }
            else if (root.targetDropResourceFolder == resource) {
                return root.targetDropNodeFolder;
            }

            return root.ItemMap.GetControl(resource);
        }

        ItemCollection list = ((ItemsControl) self).Items;
        for (int i = 0, count = list.Count; i < count; i++) {
            ResourceTreeViewItem control = (ResourceTreeViewItem) list[i]!;
            if (control.Resource == resource) {
                return control;
            }
        }

        // Oh well
        return null;
    }

    public IResourceManagerElement ManagerUI { get; set; }

    ISelectionManager<BaseResource> FramePFX.Editing.ResourceManaging.UI.IResourceTreeElement.Selection => this.SelectionManager;
}