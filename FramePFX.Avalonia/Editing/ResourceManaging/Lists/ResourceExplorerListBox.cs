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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Editing.ResourceManaging.Trees;
using FramePFX.BaseFrontEnd;
using FramePFX.BaseFrontEnd.AdvancedMenuService;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.BaseFrontEnd.ResourceManaging;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Editing.ContextRegistries;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Services.Messaging;
using ResourceManager = FramePFX.Editing.ResourceManaging.ResourceManager;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists;

public class ResourceExplorerListBox : ListBox, IResourceListElement {
    public static readonly StyledProperty<ResourceManager?> ResourceManagerProperty = AvaloniaProperty.Register<ResourceExplorerListBox, ResourceManager?>(nameof(ResourceManager));
    public static readonly StyledProperty<ResourceFolder?> CurrentFolderProperty = AvaloniaProperty.Register<ResourceExplorerListBox, ResourceFolder?>(nameof(CurrentFolder));

    private const int MaxItemCacheSize = 64;
    private const int MaxItemContentCacheSize = 16;

    private readonly ModelControlDictionary<BaseResource, ResourceExplorerListBoxItem> itemMap;
    private readonly Dictionary<Type, Stack<ResourceExplorerListItemContent>> itemContentCacheMap;
    private readonly Stack<ResourceExplorerListBoxItem> itemCache;
    private bool isProcessingManagerCurrentFolderChanged;
    private bool isProcessingAsyncDrop;

    public ResourceManager? ResourceManager {
        get => this.GetValue(ResourceManagerProperty);
        set => this.SetValue(ResourceManagerProperty, value);
    }

    public ResourceFolder? CurrentFolder {
        get => this.GetValue(CurrentFolderProperty);
        set => this.SetValue(CurrentFolderProperty, value);
    }

    public ResourceExplorerSelectionManager SelectionManager { get; }

    public IModelControlDictionary<BaseResource, ResourceExplorerListBoxItem> ItemMap => this.itemMap;

    public IResourceManagerElement ManagerUI { get; set; }
    IResourceTreeNodeElement? IResourceListElement.CurrentFolderTreeNode => this.CurrentFolder is ResourceFolder folder && !folder.IsRoot ? this.ManagerUI.GetTreeNode(folder) : null;
    IResourceListItemElement? IResourceListElement.CurrentFolderItem => this.CurrentFolder is ResourceFolder folder && !folder.IsRoot ? this.itemMap.GetControl(folder) : null;
    ISelectionManager<BaseResource> IResourceListElement.Selection => this.SelectionManager;

    public ResourceExplorerListBox() {
        this.SelectionMode = SelectionMode.Multiple;
        this.itemContentCacheMap = new Dictionary<Type, Stack<ResourceExplorerListItemContent>>();
        this.itemCache = new Stack<ResourceExplorerListBoxItem>();
        this.itemMap = new ModelControlDictionary<BaseResource, ResourceExplorerListBoxItem>();
        this.SelectionManager = new ResourceExplorerSelectionManager(this);
        this.Focusable = true;
        DataManager.GetContextData(this).Set(DataKeys.ResourceListUIKey, this);
        DragDrop.SetAllowDrop(this, true);
    }

    static ResourceExplorerListBox() {
        ResourceManagerProperty.Changed.AddClassHandler<ResourceExplorerListBox, ResourceManager?>((d, e) => d.OnResourceManagerChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        CurrentFolderProperty.Changed.AddClassHandler<ResourceExplorerListBox, ResourceFolder?>((d, e) => d.OnCurrentFolderChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        PointerPressedEvent.AddClassHandler<ResourceExplorerListBox>((d, e) => d.OnPreviewPointerPressed(e), RoutingStrategies.Tunnel);
        DragDrop.DragEnterEvent.AddClassHandler<ResourceExplorerListBox>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<ResourceExplorerListBox>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<ResourceExplorerListBox>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<ResourceExplorerListBox>((o, e) => o.OnDrop(e));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        AdvancedContextMenu.SetContextRegistry(this, ResourceContextRegistry.ResourceSurfaceContextRegistry);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    private void OnPreviewPointerPressed(PointerPressedEventArgs e) {
        PointerPointProperties props = e.GetCurrentPoint(this).Properties;
        switch (props.PointerUpdateKind) {
            case PointerUpdateKind.XButton1Pressed:
                this.CurrentFolder = this.CurrentFolder?.Parent ?? this.ResourceManager?.RootContainer;
                e.Handled = true;
            break;
            case PointerUpdateKind.LeftButtonPressed:
                // We want to clear the selection manager when the user clicks an
                // item, because the list box's implementation of selecting a single
                // item involves removing each now-not-selected item, one-by-one,
                // which does work but we also want to clear all the items in the tree 
                // which won't happen since it's only clearing the items in the list.
                // Basically, this is a wonky workaround for inaccessible code. Saves rewriting it
                if (this.SelectionManager.Count < 1) {
                    this.SelectionManager.RaiseSelectionCleared();
                }
                else if (e.KeyModifiers == KeyModifiers.None) {
                    this.SelectionManager.Clear();
                }

                if (!VisualTreeUtils.IsTemplatedItemOrDescended<ResourceExplorerListBoxItem>(e.Source as AvaloniaObject)) {
                    this.Focus();
                }

            break;
        }
    }

    private void OnCurrentFolderChanged(ResourceFolder? oldFolder, ResourceFolder? newFolder) {
        if (oldFolder != null) {
            oldFolder.ResourceAdded -= this.CurrentFolder_OnResourceAdded;
            oldFolder.ResourceRemoved -= this.CurrentFolder_OnResourceRemoved;
            oldFolder.ResourceMoved -= this.CurrentFolder_OnResourceMoved;
            for (int i = this.Items.Count - 1; i >= 0; i--) {
                this.RemoveResourceInternal(i);
            }
        }

        if (newFolder != null) {
            newFolder.ResourceAdded += this.CurrentFolder_OnResourceAdded;
            newFolder.ResourceRemoved += this.CurrentFolder_OnResourceRemoved;
            newFolder.ResourceMoved += this.CurrentFolder_OnResourceMoved;
            int i = 0;
            foreach (BaseResource resource in newFolder.Items) {
                this.InsertResourceInternal(resource, i++);
            }
        }

        ResourceManager? manager;
        if (!this.isProcessingManagerCurrentFolderChanged && (manager = this.ResourceManager) != null) {
            manager.CurrentFolder = newFolder ?? manager.RootContainer;
        }
    }

    private void CurrentFolder_OnResourceAdded(ResourceFolder parent, BaseResource item, int index) {
        this.InsertResourceInternal(item, index);
    }

    private void CurrentFolder_OnResourceRemoved(ResourceFolder parent, BaseResource item, int index) {
        this.RemoveResourceInternal(index);
    }

    private void CurrentFolder_OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) {
        if (e.IsSameFolder) {
            // Item was moved within the current folder itself
            this.MoveResourceInternal(e.OldIndex, e.NewIndex);
        }
        else if (e.NewFolder == sender) {
            // It was effectively added
            this.InsertResourceInternal(e.Item, e.NewIndex);
        }
        else {
            // It was effectively removed
            this.RemoveResourceInternal(e.OldIndex);
        }
    }

    private void OnResourceManagerChanged(ResourceManager? oldManager, ResourceManager? newManager) {
        if (oldManager != null) {
            oldManager.CurrentFolderChanged -= this.OnManagerCurrentFolderChanged;
        }

        if (newManager != null) {
            newManager.CurrentFolderChanged += this.OnManagerCurrentFolderChanged;
        }

        this.CurrentFolder = newManager?.CurrentFolder;
    }

    private void OnManagerCurrentFolderChanged(ResourceManager manager, ResourceFolder oldFolder, ResourceFolder newFolder) {
        try {
            this.isProcessingManagerCurrentFolderChanged = true;
            this.CurrentFolder = newFolder;
        }
        finally {
            this.isProcessingManagerCurrentFolderChanged = false;
        }
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) {
        return new ResourceExplorerListBoxItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) {
        return this.NeedsContainer<ResourceExplorerListBoxItem>(item, out recycleKey);
    }

    private void InsertResourceInternal(BaseResource resource, int index) {
        ResourceExplorerListBoxItem control = this.itemCache.Count > 0 ? this.itemCache.Pop() : new ResourceExplorerListBoxItem();
        this.itemMap.AddMapping(resource, control);
        control.OnAddingToList(this, resource);
        this.Items.Insert(index, control);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAddedToList();
    }

    private void RemoveResourceInternal(int index) {
        ResourceExplorerListBoxItem control = (ResourceExplorerListBoxItem) this.Items[index]!;
        this.itemMap.RemoveMapping(control.Resource!, control);
        control.OnRemovingFromList();
        this.Items.RemoveAt(index);
        control.OnRemovedFromList();
        if (this.itemCache.Count < MaxItemCacheSize)
            this.itemCache.Push(control);
    }

    private void MoveResourceInternal(int oldIndex, int newIndex) {
        ResourceExplorerListBoxItem control = (ResourceExplorerListBoxItem) this.Items[oldIndex]!;
        this.Items.RemoveAt(oldIndex);
        this.Items.Insert(newIndex, control);
    }

    /// <summary>
    /// Either returns a cached content object from resource type, or creates a new instance of it.
    /// <see cref="ReleaseContentObject"/> should be called after the returned object is no longer needed,
    /// in order to help with performance (saves re-creating the object and applying styles)
    /// </summary>
    /// <param name="resource">The resource object type</param>
    /// <returns>A reused or new content object</returns>
    public ResourceExplorerListItemContent GetContentObject(BaseResource resource) {
        ResourceExplorerListItemContent content;
        if (this.itemContentCacheMap.TryGetValue(resource.GetType(), out Stack<ResourceExplorerListItemContent>? stack) && stack.Count > 0) {
            content = stack.Pop();
        }
        else {
            content = ResourceExplorerListItemContent.Registry.NewInstance(resource);
        }

        return content;
    }

    /// <summary>
    /// Adds the given content object to our internal cache (keyed by the given resource type) if the cache
    /// is small enough, otherwise the object is forgotten and garbage collected (at least, that's the intent;
    /// bugs in the disconnection code may prevent that).
    /// The content object should not be used after this call, instead use <see cref="GetContentObject"/>
    /// </summary>
    /// <param name="resourceType">The resource object type</param>
    /// <param name="content">The content object type that is no longer in use</param>
    /// <returns>True when the object was cached, false when the cache is too large and could not fit the object in</returns>
    public bool ReleaseContentObject(BaseResource resource, ResourceExplorerListItemContent content) {
        Type resourceType = resource.GetType();
        if (!this.itemContentCacheMap.TryGetValue(resourceType, out Stack<ResourceExplorerListItemContent>? stack)) {
            this.itemContentCacheMap[resourceType] = stack = new Stack<ResourceExplorerListItemContent>();
        }
        else if (stack.Count == MaxItemContentCacheSize) {
            return false;
        }

        stack.Push(content);
        return true;
    }

    #region Drop

    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    private void OnDragOver(DragEventArgs e) {
        EnumDropType dropType = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
        if (!ResourceTreeViewItem.GetResourceListFromDragEvent(e, out List<BaseResource>? droppedItems)) {
            e.DragEffects = (DragDropEffects) ResourceDropRegistry.CanDropNativeTypeIntoListOrItem(this, null, new DataObjectWrapper(e.Data), DataManager.GetFullContextData(this), dropType);
        }
        else {
            ResourceFolder? folder = this.ResourceManager?.RootContainer;
            if (folder != null) {
                e.DragEffects = ResourceDropRegistry.CanDropResourceListIntoFolder(folder, droppedItems, dropType) ? (DragDropEffects) dropType : DragDropEffects.None;
            }
            else {
                e.DragEffects = DragDropEffects.None;
            }
        }

        // this.IsDroppableTargetOver = e.DragEffects != DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDragLeave(DragEventArgs e) {
        // if (!this.IsPointerOver) {
        //     this.IsDroppableTargetOver = false;
        // }
    }

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || !(this.ResourceManager?.RootContainer is ResourceFolder folder)) {
            return;
        }

        try {
            this.isProcessingAsyncDrop = true;
            EnumDropType dropType = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
            if (!ResourceTreeViewItem.GetResourceListFromDragEvent(e, out List<BaseResource>? droppedItems)) {
                // Dropped non-resources into this node
                if (!await ResourceDropRegistry.OnDropNativeTypeIntoListOrItem(this, null, new DataObjectWrapper(e.Data), DataManager.GetFullContextData(this), dropType)) {
                    await IMessageDialogService.Instance.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
                }

                return;
            }

            // First process final drop type, then check if the drop is allowed on this tree node
            // Then from the check drop result we determine if we can drop the list "into" or above/below

            e.DragEffects = ResourceDropRegistry.CanDropResourceListIntoFolder(folder, droppedItems, dropType) ? (DragDropEffects) dropType : DragDropEffects.None;
            await ResourceDropRegistry.OnDropResourceListIntoListItem(this, null, droppedItems, DataManager.GetFullContextData(this), (EnumDropType) e.DragEffects);
        }
#if !DEBUG
        catch (Exception exception) {
            await FramePFX.IoC.MessageService.ShowMessage("Error", "An error occurred while processing list item drop", exception.ToString());
        }
#endif
        finally {
            // this.IsDroppableTargetOver = false;
            this.isProcessingAsyncDrop = false;
        }
    }

    #endregion
}