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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia;
using PFXToolKitUI.Avalonia.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Utils;
using FramePFX.Editing.ContextRegistries;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Editing.ResourceManaging.UI;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;
using Key = Avalonia.Input.Key;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Trees;

public abstract class ResourceTreeViewItem : TreeViewItem, IResourceTreeNodeElement, IResourceTreeOrNode {
    public static readonly DirectProperty<ResourceTreeViewItem, bool> IsDroppableTargetOverProperty = AvaloniaProperty.RegisterDirect<ResourceTreeViewItem, bool>(nameof(IsDroppableTargetOver), o => o.IsDroppableTargetOver, (o, v) => o.IsDroppableTargetOver = v);
    public static readonly DirectProperty<ResourceTreeViewItem, bool> IsFolderItemProperty = AvaloniaProperty.RegisterDirect<ResourceTreeViewItem, bool>("IsFolderItem", o => o.IsFolderItem, null);

    public ResourceTreeView? ResourceTree { get; private set; }

    public ResourceTreeViewItem? ParentNode { get; private set; }

    public MovedResource? MovedResource { get; set; }

    public BaseResource? Resource { get; private set; }

    IResourceTreeNodeElement? IResourceTreeNodeElement.Parent => this.ParentNode;
    IResourceTreeElement? IResourceTreeNodeElement.Tree => this.ResourceTree;

    public bool IsDroppableTargetOver {
        get => this.isDroppableTargetOver;
        set => this.SetAndRaise(IsDroppableTargetOverProperty, ref this.isDroppableTargetOver, value);
    }

    public bool IsFolderItem {
        get => this.isFolderItem;
        private set => this.SetAndRaise(IsFolderItemProperty, ref this.isFolderItem, value);
    }

    private readonly IBinder<BaseResource> displayNameBinder = new AvaloniaPropertyToEventPropertyGetSetBinder<BaseResource>(HeaderProperty, nameof(BaseResource.DisplayNameChanged), b => b.Model.DisplayName, (b, v) => b.Model.DisplayName = (string) v);
    private Border? PART_DragDropMoveBorder;
    private Point clickMousePoint;
    private bool isDroppableTargetOver;
    private bool isProcessingAsyncDrop;
    private bool isEditingNameState;
    private string? nameBeforeEditBegin;
    private bool wasSelectedOnPress;
    private bool isFolderItem;

    private TextBlock? PART_HeaderTextBlock;
    private TextBox? PART_HeaderTextBox;

    private ResourceNodeDragState dragBtnState;
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

    protected ResourceTreeViewItem() {
        DragDrop.SetAllowDrop(this, true);
        DataManager.GetContextData(this).Set(DataKeys.ResourceNodeUIKey, this);
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

    #region Model Connection

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
        DataManager.GetContextData(this).Set(DataKeys.ResourceObjectKey, this.Resource);
        AdvancedContextMenu.SetContextRegistry(this, this.IsFolderItem ? ResourceContextRegistry.ResourceFolderContextRegistry : ResourceContextRegistry.ResourceItemContextRegistry);
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
        DataManager.GetContextData(this).Set(DataKeys.ResourceObjectKey, null);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    public virtual void OnMoving(int oldIndex, int newIndex) {
    }

    public virtual void OnMoved(int oldIndex, int newIndex) {
    }

    #endregion

    #region Model to Control objects

    private void OnResourceAdded(ResourceFolder parent, BaseResource item, int index) => this.InsertNode(item, index);

    private void OnResourceRemoved(ResourceFolder parent, BaseResource item, int index) => this.RemoveNode(index);

    private void OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) => HandleMoveEvent(this, e);

    public static void HandleMoveEvent(IResourceTreeOrNode self, ResourceMovedEventArgs e) {
        if (e.OldFolder == e.NewFolder) { // e.IsSameFolder calculates the same information
            // Only the item index is being moved
            ResourceTreeViewItem control = self.GetNodeAt(e.OldIndex);
            if (control.dragBtnState == ResourceNodeDragState.Active) {
                control.CompleteDragForDrop();
            }

            self.MoveNode(e.OldIndex, e.NewIndex);
        }
        else if (self.Resource == e.OldFolder) {
            // The control is being moved from us to another location
            IResourceTreeOrNode? dstNode = ResourceTreeView.FindNodeForResource(self, e.NewFolder);
            if (dstNode == null) {
                throw new Exception("Could not find destination tree node. Is the UI corrupted?");
            }

            ResourceTreeViewItem control = self.GetNodeAt(e.OldIndex);
            self.RemoveNode(e.OldIndex, false);
            dstNode.MovedResource = new MovedResource(control, e.Item);
        }
        else if (self.Resource == e.NewFolder) {
            // The control is being dropped into self, so clear its drag state
            if (!(self.MovedResource is MovedResource moved)) {
                throw new Exception("No information about the moved resource available");
            }

            if (moved.Control.dragBtnState == ResourceNodeDragState.Active) {
                moved.Control.CompleteDragForDrop();
            }

            self.InsertNode(moved.Control, moved.Resource, e.NewIndex);
            self.MovedResource = null;
        }
    }

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

    public void MoveNode(int oldIndex, int newIndex) {
        ResourceTreeView? tree = this.ResourceTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        ResourceTreeViewItem control = (ResourceTreeViewItem) this.Items[oldIndex]!;
        control.OnMoving(oldIndex, newIndex);
        this.Items.RemoveAt(oldIndex);
        this.Items.Insert(newIndex, control);
        control.OnMoved(oldIndex, newIndex);
    }

    #endregion

    #region Drag Drop

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled || this.Resource == null) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            return;
        }

        bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
        if ((e.ClickCount % 2) == 0) {
            if (!isToggle) {
                this.SetCurrentValue(IsExpandedProperty, !this.IsExpanded);
                e.Handled = true;
            }
        }
        else if (e.KeyModifiers == KeyModifiers.None || e.KeyModifiers == KeyModifiers.Control) {
            if (this.dragBtnState == ResourceNodeDragState.None && (this.IsFocused || this.Focus())) {
                this.dragBtnState = ResourceNodeDragState.Initiated;
                e.Pointer.Capture(this);
                this.clickMousePoint = point.Position;
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
                    else if (!this.wasSelectedOnPress || this.ResourceTree.SelectedItems.Count < 2) {
                        // Set as only selection if 0 or 1 items selected, or we aren't selected
                        this.ResourceTree?.SetSelection(this);
                    }
                }
            }

            // handle to stop tree view from selecting stuff
            e.Handled = true;
        }
    }

    private void ResetDragDropState(PointerEventArgs e) {
        this.dragBtnState = ResourceNodeDragState.None;
        if (this == e.Pointer.Captured) {
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased) {
            ResourceNodeDragState lastDragState = this.dragBtnState;
            if (lastDragState != ResourceNodeDragState.None) {
                this.ResetDragDropState(e);
                if (this.ResourceTree != null) {
                    bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
                    int selCount = this.ResourceTree!.SelectedItems.Count;
                    if (selCount == 0) {
                        // very rare scenario, shouldn't really occur
                        this.ResourceTree?.SetSelection(this);
                    }
                    else if (isToggle && this.wasSelectedOnPress && lastDragState != ResourceNodeDragState.Completed) {
                        // Check we want to toggle, check we were selected on click and we probably are still selected,
                        // and also check that the last drag wasn't completed/cancelled just because it feels more normal that way
                        this.SetCurrentValue(IsSelectedProperty, false);
                    }
                    else if (selCount > 1 && !isToggle && lastDragState != ResourceNodeDragState.Completed) {
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
            this.ResetDragDropState(e);
            this.clickMousePoint = new Point(0, 0);
            return;
        }

        if (this.dragBtnState != ResourceNodeDragState.Initiated || this.ResourceTree == null) {
            return;
        }

        if (!(this.Resource is BaseResource resource) || resource.Manager == null) {
            return;
        }

        Point mPos = point.Position;
        Point clickPos = this.clickMousePoint;
        Point change = new Point(Math.Abs(mPos.X - clickPos.X), Math.Abs(mPos.X - clickPos.X));
        if (change.X > 4 || change.Y > 4) {
            this.IsSelected = true;
            List<BaseResource> selection = this.ResourceTree.SelectionManager.SelectedItems.ToList();
            if (selection.Count < 1 || !selection.Contains(this.Resource)) {
                this.ResetDragDropState(e);
                return;
            }

            try {
                DataObject obj = new DataObject();
                obj.Set(ResourceDropRegistry.DropTypeText, selection);

                this.dragBtnState = ResourceNodeDragState.Active;
                DragDrop.DoDragDrop(e, obj, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Link);
            }
            catch (Exception ex) {
                Debug.WriteLine("Exception while executing resource tree item drag drop: " + ex.GetToString());
            }
            finally {
                this.dragBtnState = ResourceNodeDragState.Completed;
                if (this.hasCompletedDrop) {
                    this.hasCompletedDrop = false;
                    // this.IsSelected = false;
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

    private DragLocation GetDragLocation(Point pt, bool useFullHeight) {
        const double NormalBorder = 8.0;
        double middle = this.Bounds.Height / 2.0;
        if (useFullHeight) {
            return DoubleUtils.LessThan(pt.Y, middle) ? DragLocation.Above : DragLocation.Below;
        }
        else if (DoubleUtils.LessThan(pt.Y, NormalBorder)) {
            return DragLocation.Above;
        }
        else if (DoubleUtils.GreaterThanOrClose(pt.Y, this.Bounds.Height - NormalBorder)) {
            return DragLocation.Below;
        }
        else {
            return DragLocation.Inside;
        }
    }

    private void OnDragOver(DragEventArgs e) {
        Point point = e.GetPosition(this);

        EnumDropType dropType = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
        DragLocation location;
        if (!GetResourceListFromDragEvent(e, out List<BaseResource>? items)) {
            e.DragEffects = (DragDropEffects) ResourceDropRegistry.CanDropNativeTypeIntoTreeOrNode(this.ResourceTree!, this, new DataObjectWrapper(e.Data), DataManager.GetFullContextData(this), dropType);
            location = this.GetDragLocation(point, e.DragEffects == DragDropEffects.None);
        }
        else {
            if (this.Resource is ResourceFolder resAsFolder) {
                e.DragEffects = ResourceDropRegistry.CanDropResourceListIntoFolder(resAsFolder, items, dropType) ? (DragDropEffects) dropType : DragDropEffects.None;
                location = this.GetDragLocation(point, e.DragEffects == DragDropEffects.None);
            }
            else {
                e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
                location = this.GetDragLocation(point, true);
            }

            // Ideally should never be null
            ResourceFolder? parent = this.Resource!.Parent;
            if (parent == null || (!parent.IsRoot && items.Any(x => x is ResourceFolder && x.Parent != null && x.Parent.IsParentInHierarchy((ResourceFolder) x)))) {
                e.DragEffects = DragDropEffects.None;
                location = DragLocation.Inside;
            }
        }

        if (location == DragLocation.Inside || e.DragEffects == DragDropEffects.None) {
            this.IsDroppableTargetOver = e.DragEffects != DragDropEffects.None;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
        }
        else {
            this.PART_DragDropMoveBorder!.BorderThickness = location == DragLocation.Above ? new Thickness(0, 1, 0, 0) : new Thickness(0, 0, 0, 1);
        }

        e.Handled = true;
    }

    private void OnDragLeave(DragEventArgs e) {
        if (!this.IsPointerOver) {
            this.IsDroppableTargetOver = false;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
        }
    }

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || this.Resource == null) {
            return;
        }

        try {
            Point point = e.GetPosition(this);

            EnumDropType dropType = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);

            this.isProcessingAsyncDrop = true;
            // Dropped non-resources into this node
            if (!GetResourceListFromDragEvent(e, out List<BaseResource>? droppedItems)) {
                if (!await ResourceDropRegistry.OnDropNativeTypeIntoTreeOrNode(this.ResourceTree!, this, new DataObjectWrapper(e.Data), DataManager.GetFullContextData(this), dropType)) {
                    await IMessageDialogService.Instance.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
                }

                return;
            }

            // First process final drop type, then check if the drop is allowed on this tree node
            // Then from the check drop result we determine if we can drop the list "into" or above/below

            // The logic below only allows dropping Inside when we are a folder and there's no cyclic references
            DragLocation location;
            if (this.Resource is ResourceFolder resAsFolder) {
                e.DragEffects = ResourceDropRegistry.CanDropResourceListIntoFolder(resAsFolder, droppedItems, dropType) ? (DragDropEffects) dropType : DragDropEffects.None;
                location = this.GetDragLocation(point, e.DragEffects == DragDropEffects.None);
            }
            else {
                e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
                location = this.GetDragLocation(point, true);
            }

            if (location == DragLocation.Inside) {
                // We are allowed to drop the list "into" this node
                await ResourceDropRegistry.OnDropResourceListIntoTreeOrNode(this.ResourceTree!, this, droppedItems, DataManager.GetFullContextData(this), (EnumDropType) e.DragEffects);
                return;
            }

            if (dropType != EnumDropType.Move && dropType != EnumDropType.Copy) {
                return;
            }

            // Now we have to process "drag move/copy" drop
            ResourceFolder? myParent = this.Resource!.Parent;
            if (myParent == null || (!myParent.IsRoot && droppedItems.Any(x => x is ResourceFolder && x.Parent != null && x.Parent.IsParentInHierarchy((ResourceFolder) x)))) {
                return;
            }

            List<BaseResource> newList;
            int dropIndex = myParent.IndexOf(this.Resource);

            // TODO: this isn't enough, it's still buggy
            if (location == DragLocation.Below) {
                dropIndex++; // increment to place below
            }

            // We need to do post-processing in case items are moved that both not in and are in this node's current parent
            if (dropType == EnumDropType.Move) {
                droppedItems.Sort((a, b) => {
                    ResourceFolder? parA = a.Parent;
                    ResourceFolder? parB = b.Parent;
                    if (parA != parB || parA == null) {
                        return 0;
                    }

                    return parA.IndexOf(a).CompareTo(parB!.IndexOf(b));
                });

                // int count = droppedItems.Count(x => x.Parent == myParent && x.Parent.IndexOf(x) < dropIndex);
                newList = droppedItems;
                // dropIndex -= count;// = Maths.Clamp(dropIndex - count, 0, parentFolder.Items.Count);
            }
            else {
                List<BaseResource> cloneList = new List<BaseResource>();
                foreach (BaseResource layerInList in droppedItems) {
                    BaseResource clone = BaseResource.Clone(layerInList);
                    if (!TextIncrement.GetIncrementableString(s => myParent.Items.All(x => x.DisplayName != s), clone.DisplayName, out string? name, false))
                        name = clone.DisplayName;
                    clone.DisplayName = name;
                    cloneList.Add(clone);
                }

                newList = cloneList;
            }

            ResourceFolder.MoveListTo(myParent, newList, dropIndex);
            this.ResourceTree?.SelectionManager.SetSelection(newList);

            if (dropType == EnumDropType.Copy) {
                await IResourceLoaderDialogService.Instance.TryLoadResources(newList.ToArray());
            }
        }
#if !DEBUG
        catch (Exception exception) {
            await PFXToolKitUI.Services.Messaging.IMessageDialogService.Instance.ShowMessage("Error", "An error occurred while processing list item drop", exception.ToString());
        }
#endif
        finally {
            this.IsDroppableTargetOver = false;
            this.isProcessingAsyncDrop = false;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
        }
    }

    /// <summary>
    /// Tries to get the list of resources being drag-dropped from the given drag event
    /// </summary>
    /// <param name="e">Drag event (enter, over, drop, etc.)</param>
    /// <param name="resources">The resources in the drag event</param>
    /// <returns>True if there were resources available, otherwise false, meaning no resources are being dragged</returns>
    public static bool GetResourceListFromDragEvent(DragEventArgs e, [NotNullWhen(true)] out List<BaseResource>? resources) {
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