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
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FramePFX.Avalonia.AdvancedMenuService;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Editing.ResourceManaging.Trees;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Editing.ContextRegistries;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Services.Messaging;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists;

public class ResourceExplorerListBoxItem : ListBoxItem, IResourceListItemElement {
    public static readonly DirectProperty<ResourceExplorerListBoxItem, bool> IsResourceOnlineProperty = AvaloniaProperty.RegisterDirect<ResourceExplorerListBoxItem, bool>(nameof(IsResourceOnline), o => o.IsResourceOnline);
    public static readonly StyledProperty<bool> IsDroppableTargetOverProperty = AvaloniaProperty.Register<ResourceExplorerListBoxItem, bool>(nameof(IsDroppableTargetOver));
    public static readonly StyledProperty<string?> DisplayNameProperty = AvaloniaProperty.Register<ResourceExplorerListBoxItem, string?>(nameof(DisplayName));

    public bool IsResourceOnline {
        get => this.isResourceOnline;
        private set => this.SetAndRaise(IsResourceOnlineProperty, ref this.isResourceOnline, value);
    }

    public bool IsDroppableTargetOver {
        get => this.GetValue(IsDroppableTargetOverProperty);
        set => this.SetValue(IsDroppableTargetOverProperty, value);
    }

    public string? DisplayName {
        get => this.GetValue(DisplayNameProperty);
        set => this.SetValue(DisplayNameProperty, value);
    }

    /// <summary>
    /// Gets our connected resource model
    /// </summary>
    public BaseResource? Resource { get; private set; }

    /// <summary>
    /// Gets our resource explorer list box
    /// </summary>
    public ResourceExplorerListBox? ResourceExplorerList { get; private set; }

    private readonly IBinder<BaseResource> displayNameBinder = new GetSetAutoUpdateAndEventPropertyBinder<BaseResource>(DisplayNameProperty, nameof(BaseResource.DisplayNameChanged), b => b.Model.DisplayName, (b, v) => b.Model.DisplayName = (string) v);
    private bool isResourceOnline;
    private string? displayName;
    private Point originMousePoint;
    private bool isDragActive;
    private bool isDragDropping;
    private bool isProcessingAsyncDrop;

    private enum DragState {
        None = 0, // No drag drop has been started yet
        Standby = 1, // User left-clicked, so wait for enough move mvoement
        Active = 2, // User moved their mouse enough. DragDrop is running

        Completed = 3 // Layer dropped, this is used to ensure we don't restart
        // when the mouse moves again until they release the left mouse
    }

    private DragState dragBtnState;
    private bool hasCompletedDrop;
    private bool wasSelectedOnPress;

    public ResourceExplorerListBoxItem() {
    }

    static ResourceExplorerListBoxItem() {
        DragDrop.DragEnterEvent.AddClassHandler<ResourceExplorerListBoxItem>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<ResourceExplorerListBoxItem>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<ResourceExplorerListBoxItem>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<ResourceExplorerListBoxItem>((o, e) => o.OnDrop(e));
    }

    public void OnAddingToList(ResourceExplorerListBox explorerList, BaseResource resource) {
        this.ResourceExplorerList = explorerList;
        this.Resource = resource;
        this.Content = explorerList.GetContentObject(resource);
        DragDrop.SetAllowDrop(this, resource is ResourceFolder);
    }

    public void OnAddedToList() {
        this.displayNameBinder.Attach(this, this.Resource!);
        if (this.Resource is ResourceItem item) {
            item.OnlineStateChanged += this.UpdateIsOnlineState;
            this.UpdateIsOnlineState(item);
        }
        else {
            // Probably a folder so it's online
            this.IsResourceOnline = true;
        }

        ResourceExplorerListItemContent content = (ResourceExplorerListItemContent) this.Content!;
        content.ApplyStyling();
        content.ApplyTemplate();
        content.Connect(this);
        DataManager.GetContextData(this).Set(DataKeys.ResourceObjectKey, this.Resource);
        AdvancedContextMenu.SetContextRegistry(this, this.Resource is ResourceFolder ? ResourceContextRegistry.ResourceFolderContextRegistry : ResourceContextRegistry.ResourceItemContextRegistry);
    }

    public void OnRemovingFromList() {
        this.displayNameBinder.Detach();
        if (this.Resource is ResourceItem item) {
            item.OnlineStateChanged -= this.UpdateIsOnlineState;
        }

        ResourceExplorerListItemContent content = (ResourceExplorerListItemContent) this.Content!;
        content.Disconnect();
        this.Content = null;
        this.ResourceExplorerList!.ReleaseContentObject(this.Resource!, content);
    }

    public void OnRemovedFromList() {
        this.ResourceExplorerList = null;
        this.Resource = null;
        DataManager.GetContextData(this).Remove(DataKeys.ResourceObjectKey);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    private void UpdateIsOnlineState(ResourceItem resource) {
        this.IsResourceOnline = resource.IsOnline;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
            if (e.ClickCount % 2 == 0 && e.KeyModifiers == KeyModifiers.None) {
                if (this.Resource is ResourceFolder folder) {
                    if (this.ResourceExplorerList != null) {
                        this.ResourceExplorerList.CurrentFolder = folder;
                    }
                }

                e.Handled = true;
            }
            else if (CanBeginDragDrop(e.KeyModifiers)) {
                if ((this.IsFocused || this.Focus()) && !this.isDragDropping) {
                    this.dragBtnState = DragState.Standby;
                    e.Pointer.Capture(this);
                    this.originMousePoint = point.Position;
                    this.isDragActive = true;
                    if (this.ResourceExplorerList != null && this.Resource != null) {
                        this.wasSelectedOnPress = this.IsSelected;
                        bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
                        if (isToggle) {
                            if (this.wasSelectedOnPress) {
                                // do nothing; toggle selection in mouse release
                            }
                            else {
                                this.ResourceExplorerList.SelectionManager?.Select(this.Resource!);
                            }
                        }
                        else if (this.ResourceExplorerList.SelectionManager.Count < 2 || !this.wasSelectedOnPress) {
                            // Set as only selection if 0 or 1 items selected, or we aren't selected
                            this.ResourceExplorerList.SelectionManager?.SetSelection(this.Resource!);
                        }
                    }
                }

                // handle to stop tree view from selecting stuff
                e.Handled = true;
            }
        }

        base.OnPointerPressed(e);
    }

    #region Drag Drop

    public static bool CanBeginDragDrop(KeyModifiers modifiers) {
        return (modifiers & (KeyModifiers.Shift)) == 0;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased) {
            if (this.isDragActive) {
                DragState lastDragState = this.dragBtnState;
                this.dragBtnState = DragState.None;
                this.isDragActive = false;
                if (ReferenceEquals(e.Pointer.Captured, this)) {
                    e.Pointer.Capture(null);
                }

                if (this.ResourceExplorerList != null && this.Resource != null) {
                    bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
                    int selCount = this.ResourceExplorerList.SelectionManager.Count;
                    if (selCount == 0) {
                        // very rare scenario, shouldn't really occur
                        this.ResourceExplorerList.SelectionManager.SetSelection(this.Resource);
                    }
                    else if (isToggle && this.wasSelectedOnPress && lastDragState != DragState.Completed) {
                        // Check we want to toggle, check we were selected on click and we probably are still selected,
                        // and also check that the last drag wasn't completed/cancelled just because it feels more normal that way
                        this.ResourceExplorerList.SelectionManager.Unselect(this.Resource);
                    }
                    else if (selCount > 1 && !isToggle && lastDragState != DragState.Completed) {
                        this.ResourceExplorerList.SelectionManager.SetSelection(this.Resource);
                    }
                }
                // if (this.dragBtnState != DragState.Completed && (e.KeyModifiers & KeyModifiers.Control) != 0) {
                //     this.SetCurrentValue(IsSelectedProperty, !this.IsSelected);
                //     e.Handled = true;
                // }
                // else if (this.LayerTree != null && this.LayerTree.SelectedItems.Count > 1) {
                //     this.LayerTree?.SetSelection(this);
                // }
            }
        }

        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        PointerPoint point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed) {
            if (ReferenceEquals(e.Pointer.Captured, this)) {
                e.Pointer.Capture(null);
            }

            this.isDragActive = false;
            this.originMousePoint = new Point(0, 0);
            return;
        }

        if (this.dragBtnState != DragState.Standby) {
            return;
        }

        if (!this.isDragActive || this.isDragDropping || this.ResourceExplorerList == null || this.Resource == null) {
            return;
        }

        if (!(this.Resource is BaseResource resource) || resource.Manager == null) {
            return;
        }

        Point posA = point.Position;
        Point posB = this.originMousePoint;
        Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
        if (change.X > 4 || change.Y > 4) {
            List<BaseResource> selection = this.ResourceExplorerList.SelectionManager.SelectedItems.ToList();
            if (selection.Count < 1 || !selection.Contains(this.Resource)) {
                this.IsSelected = true;
            }

            try {
                this.isDragDropping = true;
                DataObject obj = new DataObject();
                obj.Set(ResourceDropRegistry.DropTypeText, selection);

                this.dragBtnState = DragState.Active;
                DragDrop.DoDragDrop(e, obj, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Link);
            }
            catch (Exception ex) {
                Debug.WriteLine("Exception while executing resource tree item drag drop: " + ex.GetToString());
            }
            finally {
                this.dragBtnState = DragState.Completed;
                this.isDragDropping = false;

                if (this.hasCompletedDrop) {
                    this.hasCompletedDrop = false;
                    this.IsSelected = false;
                }
            }
        }
    }

    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    private void OnDragOver(DragEventArgs e) {
        EnumDropType dropType = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
        if (!ResourceTreeViewItem.GetResourceListFromDragEvent(e, out List<BaseResource>? droppedItems)) {
            e.DragEffects = (DragDropEffects) ResourceDropRegistry.CanDropNativeTypeIntoListOrItem(this.ResourceExplorerList!, this, new DataObjectWrapper(e.Data), DataManager.GetFullContextData(this), dropType);
        }
        else {
            if (this.Resource is ResourceFolder resAsFolder) {
                e.DragEffects = ResourceDropRegistry.CanDropResourceListIntoFolder(resAsFolder, droppedItems, dropType) ? (DragDropEffects) dropType : DragDropEffects.None;
            }
            else {
                e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
            }

            // Ideally should never be null
            ResourceFolder? parent = this.Resource!.Parent;
            if (parent == null || (!parent.IsRoot && droppedItems.Any(x => x is ResourceFolder && x.Parent != null && x.Parent.IsParentInHierarchy((ResourceFolder) x)))) {
                e.DragEffects = DragDropEffects.None;
            }
        }

        this.IsDroppableTargetOver = e.DragEffects != DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDragLeave(DragEventArgs e) {
        if (!this.IsPointerOver) {
            this.IsDroppableTargetOver = false;
        }
    }

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || this.Resource == null) {
            return;
        }

        try {
            EnumDropType dropType = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
            this.isProcessingAsyncDrop = true;
            // Dropped non-resources into this node
            if (!ResourceTreeViewItem.GetResourceListFromDragEvent(e, out List<BaseResource>? droppedItems)) {
                if (!await ResourceDropRegistry.OnDropNativeTypeIntoListOrItem(this.ResourceExplorerList!, this, new DataObjectWrapper(e.Data), DataManager.GetFullContextData(this), dropType)) {
                    await IMessageDialogService.Instance.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
                }

                return;
            }

            // First process final drop type, then check if the drop is allowed on this tree node
            // Then from the check drop result we determine if we can drop the list "into" or above/below

            if (this.Resource is ResourceFolder resAsFolder) {
                e.DragEffects = ResourceDropRegistry.CanDropResourceListIntoFolder(resAsFolder, droppedItems, dropType) ? (DragDropEffects) dropType : DragDropEffects.None;
                await ResourceDropRegistry.OnDropResourceListIntoListItem(this.ResourceExplorerList!, this, droppedItems, DataManager.GetFullContextData(this), (EnumDropType) e.DragEffects);
            }
        }
#if !DEBUG
        catch (Exception exception) {
            await FramePFX.IoC.MessageService.ShowMessage("Error", "An error occurred while processing list item drop", exception.ToString());
        }
#endif
        finally {
            this.IsDroppableTargetOver = false;
            this.isProcessingAsyncDrop = false;
        }
    }

    #endregion
}