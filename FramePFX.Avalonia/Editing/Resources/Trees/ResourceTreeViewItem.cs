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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;
using Key = Avalonia.Input.Key;

namespace FramePFX.Avalonia.Editing.Resources.Trees;

public abstract class ResourceTreeViewItem : TreeViewItem, IResourceTreeNodeElement, IResourceTreeElement {
    public static readonly StyledProperty<bool> IsDroppableTargetOverProperty = ResourceTreeView.IsDroppableTargetOverProperty.AddOwner<ResourceTreeView>();
    public static readonly DirectProperty<ResourceTreeViewItem, bool> IsFolderItemProperty = AvaloniaProperty.RegisterDirect<ResourceTreeViewItem, bool>("IsFolderItem", o => o.IsFolderItem, null);

    public ResourceTreeView? ResourceTree { get; private set; }
    
    public ResourceTreeViewItem? ParentNode { get; private set; }
    
    public MovedResource MovedResource { get; set; }
    
    public BaseResource? Resource { get; private set; }
    
    IResourceTreeNodeElement? IResourceTreeNodeElement.Parent => this.ParentNode;
    FramePFX.Editing.ResourceManaging.UI.IResourceTreeElement? IResourceTreeNodeElement.Tree => this.ResourceTree;

    public bool IsDroppableTargetOver {
        get => this.GetValue(IsDroppableTargetOverProperty);
        set => this.SetValue(IsDroppableTargetOverProperty, value);
    }

    public bool IsFolderItem {
        get => this.isFolderItem;
        private set => this.SetAndRaise(IsFolderItemProperty, ref this.isFolderItem, value);
    }
    
    private readonly IBinder<BaseResource> displayNameBinder = new GetSetAutoUpdateAndEventPropertyBinder<BaseResource>(HeaderProperty, nameof(BaseResource.DisplayNameChanged), b => b.Model.DisplayName, (b, v) => b.Model.DisplayName = (string) v);
    private Border? PART_DragDropMoveBorder;
    private bool isDragDropping;
    private bool isDragActive;
    private Point originMousePoint;
    private bool isProcessingAsyncDrop;
    private bool isEditingNameState;
    private string? nameBeforeEditBegin;
    private bool wasSelectedOnPress;
    private bool isFolderItem;

    private TextBlock? PART_HeaderTextBlock;
    private TextBox? PART_HeaderTextBox;
    private readonly ContextData contextData;

    private enum DragState {
        None = 0, // No drag drop has been started yet
        Standby = 1, // User left-clicked, so wait for enough move mvoement
        Active = 2, // User moved their mouse enough. DragDrop is running

        Completed = 3 // Layer dropped, this is used to ensure we don't restart
        // when the mouse moves again until they release the left mouse
    }

    private DragState dragBtnState;
    private bool hasCompletedDrop;

    BaseResource? IResourceTreeNodeElement.Resource => this.Resource;

    public bool EditNameState {
        get => this.isEditingNameState;
        set {
            if (this.PART_HeaderTextBox == null)
                throw new InvalidOperationException("Too early to use this property. Let node to initialise first");
            if (this.Resource == null)
                throw new InvalidOperationException("Invalid node; no layer object");

            if (value == this.isEditingNameState)
                return;

            if (!this.isEditingNameState) {
                this.nameBeforeEditBegin = this.Resource.DisplayName;
                this.isEditingNameState = true;
            }
            else {
                this.isEditingNameState = false;
            }

            this.UpdateHeaderEditorControls();
        }
    }

    public ResourceTreeViewItem() {
        DragDrop.SetAllowDrop(this, true);
        DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.ResourceNodeUIKey, this));
    }

    static ResourceTreeViewItem() {
        DragDrop.DragEnterEvent.AddClassHandler<ResourceTreeViewItem>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<ResourceTreeViewItem>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<ResourceTreeViewItem>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<ResourceTreeViewItem>((o, e) => o.OnDrop(e));
        IsSelectedProperty.Changed.AddClassHandler<ResourceTreeViewItem, bool>((o, e) => {
        });
    }

    private void UpdateHeaderEditorControls() {
        if (this.isEditingNameState) {
            this.PART_HeaderTextBox!.IsVisible = true;
            this.PART_HeaderTextBlock!.IsVisible = false;
            BugFix.TextBox_FocusSelectAll(this.PART_HeaderTextBox);
        }
        else {
            this.PART_HeaderTextBox!.IsVisible = false;
            this.PART_HeaderTextBlock!.IsVisible = true;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_DragDropMoveBorder = e.NameScope.GetTemplateChild<Border>(nameof(this.PART_DragDropMoveBorder));
        this.PART_HeaderTextBlock = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_HeaderTextBlock));
        this.PART_HeaderTextBox = e.NameScope.GetTemplateChild<TextBox>(nameof(this.PART_HeaderTextBox));
        this.PART_HeaderTextBox.KeyDown += this.PART_HeaderTextBoxOnKeyDown;
        this.PART_HeaderTextBox.LostFocus += this.PART_HeaderTextBoxOnLostFocus;
        this.UpdateHeaderEditorControls();
    }

    private void PART_HeaderTextBoxOnLostFocus(object? sender, RoutedEventArgs e) {
        this.EditNameState = false;
        if (this.Resource != null) {
            this.Resource.DisplayName = this.nameBeforeEditBegin ?? "Layer Object";
        }
    }

    private void PART_HeaderTextBoxOnKeyDown(object? sender, KeyEventArgs e) {
        if (this.EditNameState && (e.Key == Key.Escape || e.Key == Key.Enter)) {
            string oldName = this.nameBeforeEditBegin ?? "Layer Object";
            string newName = this.PART_HeaderTextBox!.Text ?? "Layer Object";

            this.EditNameState = false;
            if (e.Key == Key.Escape) {
                if (this.Resource != null)
                    this.Resource.DisplayName = oldName;
            }
            else {
                if (this.Resource != null)
                    this.Resource.DisplayName = newName;
            }

            this.Focus();
            e.Handled = true;
        }

        // FIX prevent arrow key presses in text box from changing the selected tree item.
        // Weirdly, it's only an issue when pressing Down (and I guess up too) and
        // Right (when at the end of the text box)
        if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down) {
            e.Handled = true;
        }
    }

    public virtual void OnAdding(ResourceTreeView tree, ResourceTreeViewItem? parentNode, BaseResource resource) {
        this.ResourceTree = tree;
        this.ParentNode = parentNode;
        this.Resource = resource;
        this.IsFolderItem = resource is ResourceFolder;
    }

    public virtual void OnAdded() {
        if (this.Resource is ResourceFolder folder) {
            folder.ResourceAdded += this.OnResourceAdded;
            folder.ResourceRemoved += this.OnResourceRemoved;
            folder.ResourceMoved += this.OnResourceMoved;
            int i = 0;
            foreach (BaseResource item in folder.Items) {
                this.InsertNode(item, i++);
            }
        }

        this.displayNameBinder.Attach(this, this.Resource!);
        DataManager.SetContextData(this, this.contextData.Set(DataKeys.ResourceObjectKey, this.Resource));
    }

    public virtual void OnRemoving() {
        if (this.Resource is ResourceFolder folder) {
            folder.ResourceAdded -= this.OnResourceAdded;
            folder.ResourceRemoved -= this.OnResourceRemoved;
            folder.ResourceMoved -= this.OnResourceMoved;
        }

        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--) {
            this.RemoveNode(i);
        }

        this.displayNameBinder.Detach();
    }

    public virtual void OnRemoved() {
        this.ResourceTree = null;
        this.ParentNode = null;
        this.Resource = null;
        this.IsFolderItem = false;
        DataManager.ClearContextData(this);
    }
    
    private void OnResourceAdded(ResourceFolder parent, BaseResource item, int index) => this.InsertNode(item, index);

    private void OnResourceRemoved(ResourceFolder parent, BaseResource item, int index) => this.RemoveNode(index);

    private void OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) => HandleMoveEvent(this, e);

    public ResourceTreeViewItem GetNodeAt(int index) => (ResourceTreeViewItem) this.Items[index]!;

    public void InsertNode(BaseResource item, int index) {
        this.InsertNode(null, item, index);
    }

    public void InsertNode(ResourceTreeViewItem? control, BaseResource layer, int index) {
        ResourceTreeView? tree = this.ResourceTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot add children when we have no resource tree associated");
        if (control == null)
            control = tree.GetCachedItemOrNew();

        control.OnAdding(tree, this, layer);
        this.Items.Insert(index, control);
        tree.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNode(int index, bool canCache = true) {
        ResourceTreeView? tree = this.ResourceTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        ResourceTreeViewItem control = (ResourceTreeViewItem) this.Items[index]!;
        BaseResource resource = control.Resource ?? throw new Exception("Invalid application state");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        tree.RemoveResourceMapping(control, resource);
        control.OnRemoved();
        if (canCache)
            tree.PushCachedItem(control);
    }

    #region Drag Drop

    public static bool CanBeginDragDrop(KeyModifiers modifiers) {
        return (modifiers & (KeyModifiers.Shift)) == 0;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled || this.Resource == null) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
            bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
            if ((e.ClickCount % 2) == 0) {
                if (!isToggle) {
                    this.SetCurrentValue(IsExpandedProperty, !this.IsExpanded);
                    e.Handled = true;
                }
            }
            else if (CanBeginDragDrop(e.KeyModifiers)) {
                if ((this.IsFocused || this.Focus()) && !this.isDragDropping) {
                    this.dragBtnState = DragState.Standby;
                    e.Pointer.Capture(this);
                    this.originMousePoint = point.Position;
                    this.isDragActive = true;
                    if (this.ResourceTree != null) {
                        this.wasSelectedOnPress = this.IsSelected;
                        if (isToggle) {
                            if (this.wasSelectedOnPress) {
                                // do nothing; toggle selection in mouse release
                            }
                            else {
                                this.SetCurrentValue(IsSelectedProperty, true);
                            }
                        }
                        else if (this.ResourceTree.SelectedItems.Count < 2 || !this.wasSelectedOnPress) {
                            // Set as only selection if 0 or 1 items selected, or we aren't selected
                            this.ResourceTree?.SetSelection(this);
                        }
                    }
                }

                // handle to stop tree view from selecting stuff
                e.Handled = true;
            }
        }
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

                if (this.ResourceTree != null) {
                    bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
                    int selCount = this.ResourceTree!.SelectedItems.Count;
                    if (selCount == 0) {
                        // very rare scenario, shouldn't really occur
                        this.ResourceTree?.SetSelection(this);
                    }
                    else if (isToggle && this.wasSelectedOnPress && lastDragState != DragState.Completed) {
                        // Check we want to toggle, check we were selected on click and we probably are still selected,
                        // and also check that the last drag wasn't completed/cancelled just because it feels more normal that way
                        this.SetCurrentValue(IsSelectedProperty, false);
                    }
                    else if (selCount > 1 && !isToggle && lastDragState != DragState.Completed) {
                        this.ResourceTree?.SetSelection(this);
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

        if (!this.isDragActive || this.isDragDropping || this.ResourceTree == null) {
            return;
        }

        if (!(this.Resource is BaseResource resource) || resource.Manager == null) {
            return;
        }

        Point posA = point.Position;
        Point posB = this.originMousePoint;
        Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
        if (change.X > 4 || change.Y > 4) {
            List<BaseResource> selection = this.ResourceTree.SelectionManager.SelectedItems.ToList();
            if (selection.Count < 1 || !selection.Contains(this.Resource)) {
                this.IsSelected = true;
            }

            try {
                this.isDragDropping = true;
                DataObject obj = new DataObject();
                obj.Set(ResourceDropRegistry.DropTypeText, selection);

                this.dragBtnState = DragState.Active;
                DragDrop.DoDragDrop(e, obj, DragDropEffects.Move | DragDropEffects.Copy);
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
    
    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
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

    private void OnDragOver(DragEventArgs e) {
        if (this.Resource == null) {
            return;
        }

        bool isDropAbove;
        bool? canDragOver = ProcessCanDragOver(this, this.Resource, e);
        if (!canDragOver.HasValue) {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        const DragDropEffects args = DragDropEffects.Move | DragDropEffects.Copy;
        this.GetDropBorder(!(canDragOver ?? false), out double borderTop, out double borderBottom);
        Point point = e.GetPosition(this);
        if (DoubleUtils.LessThan(point.Y, borderTop)) {
            isDropAbove = true;
            e.DragEffects = args;
        }
        else if (DoubleUtils.GreaterThanOrClose(point.Y, borderBottom)) {
            isDropAbove = false;
            e.DragEffects = args;
        }
        else {
            if (canDragOver == true) {
                this.IsDroppableTargetOver = true;
                e.DragEffects = args;
            }

            this.PART_DragDropMoveBorder!.BorderThickness = default;
            return;
        }

        this.PART_DragDropMoveBorder!.BorderThickness = isDropAbove ? new Thickness(0, 1, 0, 0) : new Thickness(0, 0, 0, 1);
        e.Handled = true;
    }

    private void OnDragLeave(DragEventArgs e) {
        if (!this.IsPointerOver) {
            this.IsDroppableTargetOver = false;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
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

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || !(this.Resource is BaseResource layer) || layer.Parent == null) {
            return;
        }

        try {
            this.isProcessingAsyncDrop = true;
            Point point = e.GetPosition(this);
            double borderTop, borderBottom;
            if (!GetDropResourceListForEvent(e, out List<BaseResource>? srcItemList, out EnumDropType dropType)) {
                this.GetDropBorder(true, out borderTop, out borderBottom);
                bool thing = DoubleUtils.LessThan(point.Y, borderTop);
                ContextData ctx = new ContextData(DataManager.GetFullContextData((ResourceTreeViewItem) e.Source!)).Set(ResourceDropRegistry.IsAboveTarget, thing);
                if (await ResourceDropRegistry.DropRegistry.OnDroppedNative(layer, new DataObjectWrapper(e.Data), dropType, ctx)) {
                    return;
                }

                await IoC.MessageService.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
                return;
            }

            bool isDropAbove;
            bool? canDragOver = ProcessCanDragOver(this, this.Resource, e);
            if (!canDragOver.HasValue) {
                return;
            }

            List<BaseResource> newList;
            this.GetDropBorder(!(canDragOver ?? false), out borderTop, out borderBottom);
            if (DoubleUtils.LessThan(point.Y, borderTop)) {
                isDropAbove = true;
            }
            else if (DoubleUtils.GreaterThanOrClose(point.Y, borderBottom)) {
                isDropAbove = false;
            }
            else if (layer is ResourceFolder folder) {
                if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
                    return;
                }

                newList = new List<BaseResource>();
                foreach (BaseResource item in srcItemList) {
                    if (item is ResourceFolder composition && composition.IsParentInHierarchy(folder)) {
                        continue;
                    }

                    if (dropType == EnumDropType.Copy) {
                        BaseResource clone = BaseResource.Clone(item);
                        if (!TextIncrement.GetIncrementableString((s => true), clone.DisplayName, out string name))
                            name = clone.DisplayName;
                        clone.DisplayName = name;
                        folder.AddItem(clone);
                    }
                    else if (item.Parent != null) {
                        if (item.Parent != folder) {
                            newList.Add(item);
                            item.Parent.MoveItemTo(folder, item);
                        }
                    }
                    else {
                        Debug.Assert(false, "No parent");
                        // ???
                        // AppLogger.Instance.WriteLine("A resource was dropped with a null parent???");
                    }
                }

                return;
            }
            else {
                await ResourceDropRegistry.DropRegistry.OnDropped(layer, srcItemList, dropType);
                return;
            }

            // TODO: fix stack overflow when dropping a layer into itself...
            // if (layer.Parent == null || ResourceDropRegistry.CanDropItems(layer.Parent, list, effects, false) == EnumDropType.None)
            //     return;

            // I think this works?
            ResourceFolder? target = layer.Parent;
            if (target == null || (!target.IsRoot && srcItemList.Any(x => x is ResourceFolder cl && cl.Parent != null && cl.Parent.IsParentInHierarchy(cl)))) {
                return;
            }

            int index;
            bool isLayerInList = false;
            switch (dropType) {
                case EnumDropType.Move:
                    isLayerInList = srcItemList.Remove(layer);
                    index = layer.Parent.IndexOf(layer);
                    newList = srcItemList;
                    break;
                case EnumDropType.Copy: {
                    index = layer.Parent.IndexOf(layer);
                    List<BaseResource> cloneList = new List<BaseResource>();
                    foreach (BaseResource layerInList in srcItemList) {
                        BaseResource clone = BaseResource.Clone(layerInList);
                        if (!TextIncrement.GetIncrementableString((s => true), clone.DisplayName, out string name))
                            name = clone.DisplayName;
                        clone.DisplayName = name;
                        cloneList.Add(clone);
                    }

                    newList = cloneList;
                    break;
                }
                default: return;
            }

            int moveIndex = isDropAbove ? index : (index + 1);
            foreach (BaseResource item in newList) {
                if (item.Parent != null) {
                    item.Parent.MoveItemTo(layer.Parent, item, moveIndex++);
                }
                else {
                    layer.Parent.InsertItem(moveIndex++, item);
                }
            }

            if (dropType == EnumDropType.Move && isLayerInList) {
                newList.Add(layer);
            }

            this.ResourceTree!.SelectionManager.SetSelection(newList);
        }
        finally {
            this.IsDroppableTargetOver = false;
            this.isProcessingAsyncDrop = false;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
        }
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

    public static void HandleMoveEvent(IResourceTreeElement self, ResourceMovedEventArgs e) {
        if (e.OldFolder == self.Resource) {
            // The item in our collection is being moved
            IResourceTreeElement? dstNode = ResourceTreeView.FindNodeForResource(self, e.NewFolder);
            if (dstNode == null) {
                // Instead of throwing, we could just remove the track or insert a new track, instead of
                // trying to re-use existing controls, at the cost of performance.
                // However, moving clips between tracks in different timelines is not directly supported
                // so there's no need to support it here
                throw new Exception("Could not find destination tree node. Is the UI corrupted?");
            }

            ResourceTreeViewItem control = self.GetNodeAt(e.OldIndex);
            self.RemoveNode(e.OldIndex, false);
            dstNode.MovedResource = new MovedResource(control, e.Item);
        }
        else if (e.NewFolder == self.Resource) {
            if (!(self.MovedResource is MovedResource moved)) {
                throw new Exception("Clip control being moved is null. Is the UI timeline corrupted or did the clip move between timelines?");
            }

            // The control was dropped into self, so clear its drag state
            if (moved.Control.isDragDropping) {
                moved.Control.CompleteDragForDrop();
            }
            
            self.InsertNode(moved.Control, moved.Resource, e.NewIndex);
            self.MovedResource = null;
        }
    }
}