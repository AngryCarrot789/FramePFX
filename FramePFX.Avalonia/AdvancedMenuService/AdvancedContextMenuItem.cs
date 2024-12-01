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

using System;
using Avalonia.Controls;
using FramePFX.AdvancedMenuService;

namespace FramePFX.Avalonia.AdvancedMenuService;

public class AdvancedContextMenuItem : MenuItem {
    protected override Type StyleKeyOverride => typeof(MenuItem);

    /// <summary>
    /// Gets the container object, that being, the root object that stores the menu item tree that this instance is in
    /// </summary>
    public IAdvancedContainer Container { get; private set; }

    /// <summary>
    /// Gets the parent context menu item node. This MAY be different from the logical parent menu item
    /// </summary>
    public ItemsControl ParentNode { get; private set; }

    public BaseContextEntry Entry { get; private set; }

    public AdvancedContextMenuItem() { }

    public virtual void OnAdding(IAdvancedContainer container, ItemsControl parent, BaseContextEntry entry) {
        this.Container = container;
        this.ParentNode = parent;
        this.Entry = entry;
    }

    public virtual void OnAdded() {
        this.Header = this.Entry.DisplayName;
        ToolTip.SetTip(this, this.Entry.Description ?? "");
        if (this.Entry is SubListContextEntry list) {
            MenuService.InsertItemNodes(this.Container, this, list.ItemList);
        }
    }

    public virtual void OnRemoving() {
        MenuService.ClearItemNodes(this);
    }

    public virtual void OnRemoved() {
        this.Container = null;
        this.ParentNode = null;
        this.Entry = null;
    }

    public virtual void UpdateCanExecute() {
    }
}