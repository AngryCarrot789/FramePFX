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
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using ResourceManager = FramePFX.Editing.ResourceManaging.ResourceManager;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists;

public class ResourceExplorerListBox : ListBox, IResourceListElement {
    public static readonly StyledProperty<ResourceManager?> ResourceManagerProperty = AvaloniaProperty.Register<ResourceExplorerListBox, ResourceManager?>(nameof(ResourceManager));
    public static readonly StyledProperty<ResourceFolder?> CurrentFolderProperty = AvaloniaProperty.Register<ResourceExplorerListBox, ResourceFolder?>(nameof(CurrentFolder));

    public ResourceManager? ResourceManager {
        get => this.GetValue(ResourceManagerProperty);
        set => this.SetValue(ResourceManagerProperty, value);
    }
    
    public ResourceFolder? CurrentFolder {
        get => this.GetValue(CurrentFolderProperty);
        set => this.SetValue(CurrentFolderProperty, value);
    }
    
    public ResourceExplorerSelectionManager SelectionManager { get; }

    private const int MaxItemCacheSize = 64;
    private const int MaxItemContentCacheSize = 16;

    private readonly ModelControlDictionary<BaseResource, ResourceExplorerListBoxItem> itemMap;
    private readonly Dictionary<Type, Stack<ResourceExplorerListItemContent>> itemContentCacheMap;
    private readonly Stack<ResourceExplorerListBoxItem> itemCache;
    private bool isProcessingManagerCurrentFolderChanged;
    
    public IModelControlDictionary<BaseResource, ResourceExplorerListBoxItem> ItemMap => this.itemMap;

    public ResourceExplorerListBox() {
        this.SelectionMode = SelectionMode.Multiple;
        this.itemContentCacheMap = new Dictionary<Type, Stack<ResourceExplorerListItemContent>>();
        this.itemCache = new Stack<ResourceExplorerListBoxItem>();
        this.itemMap = new ModelControlDictionary<BaseResource, ResourceExplorerListBoxItem>();
        this.SelectionManager = new ResourceExplorerSelectionManager(this);
        this.Focusable = true;
        DataManager.SetContextData(this, new ContextData().Set(DataKeys.ResourceListUIKey, this));
    }

    static ResourceExplorerListBox() {
        ResourceManagerProperty.Changed.AddClassHandler<ResourceExplorerListBox, ResourceManager?>((d, e) => d.OnResourceManagerChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        CurrentFolderProperty.Changed.AddClassHandler<ResourceExplorerListBox, ResourceFolder?>((d, e) => d.OnCurrentFolderChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        PointerPressedEvent.AddClassHandler<ResourceExplorerListBox>((d, e) => d.OnPreviewPointerPressed(e), RoutingStrategies.Tunnel);
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
                else {
                    this.SelectionManager.Clear();
                }

                if (!VisualTreeUtils.IsTemplatedItemOrChild<ResourceExplorerListBoxItem>(e.Source as AvaloniaObject)) {
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
            manager.CurrentFolder = newFolder;
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

    public IResourceManagerElement ManagerUI { get; set; }
    IResourceTreeNodeElement? IResourceListElement.CurrentFolder => this.CurrentFolder is ResourceFolder folder && !folder.IsRoot ? this.ManagerUI.GetNode(folder) : null;
    ISelectionManager<BaseResource> IResourceListElement.Selection => this.SelectionManager;
}