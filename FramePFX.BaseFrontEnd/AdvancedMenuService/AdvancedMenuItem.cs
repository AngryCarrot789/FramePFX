// 
// Copyright (c) 2024-2024 REghZy
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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FramePFX.AdvancedMenuService;
using FramePFX.BaseFrontEnd.AvControls;
using FramePFX.Utils.Collections.Observable;

namespace FramePFX.BaseFrontEnd.AdvancedMenuService;

/// <summary>
/// A menu item that participates in the advanced menu service
/// </summary>
public class AdvancedMenuItem : MenuItem, IAdvancedMenuOrItem {
    protected override Type StyleKeyOverride => typeof(MenuItem);

    /// <summary>
    /// Gets the container object, that being, the root object that stores the menu item tree that this instance is in
    /// </summary>
    public IAdvancedMenu? OwnerMenu { get; private set; }

    /// <summary>
    /// Gets the parent context menu item node. This MAY be different from the logical parent menu item
    /// </summary>
    public IAdvancedMenuOrItem? ParentNode { get; private set; }

    public BaseContextEntry? Entry { get; private set; }

    bool IAdvancedMenuOrItem.IsOpen => this.IsSubMenuOpen;
    
    protected Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion;
    protected Dictionary<int, int>? dynamicInserted;
    private IconControl? myIconControl;

    public AdvancedMenuItem() { }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        AdvancedMenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
        Dispatcher.UIThread.InvokeAsync(() => AdvancedMenuService.NormaliseSeparators(this), DispatcherPriority.Loaded);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
    }

    public virtual void OnAdding(IAdvancedMenu menu, IAdvancedMenuOrItem parent, BaseContextEntry entry) {
        this.OwnerMenu = menu;
        this.ParentNode = parent;
        this.Entry = entry;
    }

    public virtual void OnAdded() {
        this.Header = this.Entry!.DisplayName;
        if (this.Entry.Description != null)
            ToolTip.SetTip(this, this.Entry.Description ?? "");

        if (this.Entry.Icon != null) {
            this.myIconControl = new IconControl() {
                Icon = this.Entry.Icon,
                Stretch = (Stretch) this.Entry.StretchMode
            };
            
            this.Icon = this.myIconControl;
        }
        
        if (this.Entry is ContextEntryGroup list) {
            AdvancedMenuService.InsertItemNodes(this, list.Items);
            list.Items.ItemsAdded += this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved += this.ItemsOnItemsRemoved;
            list.Items.ItemMoved += this.ItemsOnItemMoved;
            list.Items.ItemReplaced += this.ItemsOnItemReplaced;
        }
    }

    public virtual void OnRemoving() {
        if (this.Entry is ContextEntryGroup list) {
            list.Items.ItemsAdded -= this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved -= this.ItemsOnItemsRemoved;
            list.Items.ItemMoved -= this.ItemsOnItemMoved;
            list.Items.ItemReplaced -= this.ItemsOnItemReplaced;
        }
        
        if (this.myIconControl != null) {
            this.myIconControl.Icon = null;
            this.myIconControl = null;
            this.Icon = null;
        }
        
        AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
        AdvancedMenuService.ClearItemNodes(this);
    }

    public virtual void OnRemoved() {
        this.OwnerMenu = null;
        this.ParentNode = null;
        this.Entry = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (change.Property == IsSubMenuOpenProperty) {
            if (this.IsSubMenuOpen) {
                AdvancedMenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
                Debug.WriteLine($"Generated dynamic items for menu item shown: " + (this.dynamicInserted?.Count ?? 0));
            }
            else {
                Debug.WriteLine($"Cleared '{this.dynamicInserted?.Count ?? 0}' dynamic items for menu item hidden");
                AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
            }
        }
    }

    public virtual void UpdateCanExecute() {
    }

    public void StoreDynamicGroup(DynamicGroupPlaceholderContextObject groupPlaceholder, int index) {
        (this.dynamicInsertion ??= new Dictionary<int, DynamicGroupPlaceholderContextObject>())[index] = groupPlaceholder;
    }
    
    private void ItemsOnItemsAdded(IObservableList<IContextObject> list, IList<IContextObject> items, int index) {
        AdvancedMenuService.InsertItemNodesWithDynamicSupport(this, index, new List<IContextObject>(items), ref this.dynamicInsertion, ref this.dynamicInserted);
    }
        
    private void ItemsOnItemsRemoved(IObservableList<IContextObject> list, IList<IContextObject> items, int index) {
        AdvancedMenuService.RemoveItemNodesWithDynamicSupport(this.OwnerMenu!, this, index, new List<IContextObject>(items), ref this.dynamicInsertion, ref this.dynamicInserted);
    }
        
    private void ItemsOnItemMoved(IObservableList<IContextObject> list, IContextObject item, int oldIndex, int newIndex) {
        AdvancedMenuService.RemoveItemNodesWithDynamicSupport(this.OwnerMenu!, this, oldIndex, [item], ref this.dynamicInsertion, ref this.dynamicInserted);
        AdvancedMenuService.InsertItemNodesWithDynamicSupport(this, newIndex, [item], ref this.dynamicInsertion, ref this.dynamicInserted);
    }
        
    private void ItemsOnItemReplaced(IObservableList<IContextObject> list, IContextObject oldItem, IContextObject newItem, int index) {
        AdvancedMenuService.RemoveItemNodesWithDynamicSupport(this.OwnerMenu!, this, index, [oldItem], ref this.dynamicInsertion, ref this.dynamicInserted);
        AdvancedMenuService.InsertItemNodesWithDynamicSupport(this, index, [newItem], ref this.dynamicInsertion, ref this.dynamicInserted);
    }
}