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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FramePFX.Avalonia.AdvancedMenuService;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists;

public class ResourceExplorerListBoxItem : ListBoxItem {
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
        DataManager.SetContextData(this, new ContextData().Set(DataKeys.ResourceObjectKey, this.Resource));
        AdvancedContextMenu.SetContextRegistry(this, this.Resource is ResourceFolder ? BaseResource.ResourceFolderContextRegistry : BaseResource.ResourceItemContextRegistry);
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
        DataManager.ClearContextData(this);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    private void UpdateIsOnlineState(ResourceItem resource) {
        this.IsResourceOnline = resource.IsOnline;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
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
                                this.SetCurrentValue(IsSelectedProperty, true);
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
    }

    #region Drag Drop

    public static bool CanBeginDragDrop(KeyModifiers modifiers) {
        return (modifiers & (KeyModifiers.Shift)) == 0;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
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
                        this.SetCurrentValue(IsSelectedProperty, false);
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

    private void CompleteDragForDrop() {
        this.hasCompletedDrop = true;
    }

    private void GetDropBorder(bool useFullHeight, out double borderTop, out double borderBottom) {
        const double NormalBorder = 8.0;
        if (useFullHeight) {
            borderTop = borderBottom = this.Bounds.Height / 2.0;
        }
        else {
            borderTop = NormalBorder;
            borderBottom = this.Bounds.Height - NormalBorder;
        }
    }

    public static EnumDropType CanDropItemsOnResourceFolder(ResourceFolder target, List<BaseResource> items, EnumDropType dropType) {
        if (dropType == EnumDropType.None || dropType == EnumDropType.Link) {
            return EnumDropType.None;
        }

        foreach (BaseResource item in items) {
            if (item is ResourceFolder folder && folder.IsParentInHierarchy(target)) {
                return EnumDropType.None;
            }
            else if (dropType != EnumDropType.Copy) {
                if (target.Contains(item)) {
                    return EnumDropType.None;
                }
            }
        }

        return dropType;
    }

    // True = yes, False = no, Null = invalid due to composition layers
    public static bool? ProcessCanDragOver(AvaloniaObject sender, BaseResource target, DragEventArgs e) {
        e.Handled = true;
        if (GetDropResourceListForEvent(e, out List<BaseResource>? items, out EnumDropType effects)) {
            if (target is ResourceFolder composition) {
                if (!composition.IsRoot && items.Any(x => x is ResourceFolder cl && cl.Parent != null && cl.Parent.IsParentInHierarchy(cl))) {
                    return null;
                }
            }
            else {
                e.DragEffects = (DragDropEffects) ResourceDropRegistry.DropRegistry.CanDrop(target, items, effects, DataManager.GetFullContextData(sender));
            }
        }
        else {
            e.DragEffects = (DragDropEffects) ResourceDropRegistry.DropRegistry.CanDropNative(target, new DataObjectWrapper(e.Data), effects, DataManager.GetFullContextData(sender));
        }

        return e.DragEffects != DragDropEffects.None;
    }

    /// <summary>
    /// Tries to get the list of resources being drag-dropped from the given drag event. Provides the
    /// effects currently applicable for the event regardless of this method's return value
    /// </summary>
    /// <param name="e">Drag event (enter, over, drop, etc.)</param>
    /// <param name="resources">The resources in the drag event</param>
    /// <param name="effects">The effects applicable based on the event's effects and user's keys pressed</param>
    /// <returns>True if there were resources available, otherwise false, meaning no resources are being dragged</returns>
    public static bool GetDropResourceListForEvent(DragEventArgs e, [NotNullWhen(true)] out List<BaseResource>? resources, out EnumDropType effects) {
        effects = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
        if (e.Data.Contains(ResourceDropRegistry.DropTypeText)) {
            object? obj = e.Data.Get(ResourceDropRegistry.DropTypeText);
            if ((resources = (obj as List<BaseResource>)) != null) {
                return true;
            }
        }

        resources = null;
        return false;
    }

    #endregion
}