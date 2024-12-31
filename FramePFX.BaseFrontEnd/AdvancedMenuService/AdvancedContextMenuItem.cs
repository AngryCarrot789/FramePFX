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

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FramePFX.AdvancedMenuService;
using FramePFX.BaseFrontEnd.AvControls;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.BaseFrontEnd.AdvancedMenuService;

public class AdvancedContextMenuItem : MenuItem, IAdvancedContextElement {
    protected override Type StyleKeyOverride => typeof(MenuItem);

    public IContextData? CapturedContext => this.Container?.CapturedContext;

    /// <summary>
    /// Gets the container object, that being, the root object that stores the menu item tree that this instance is in
    /// </summary>
    public IAdvancedContainer? Container { get; private set; }

    /// <summary>
    /// Gets the parent context menu item node. This MAY be different from the logical parent menu item
    /// </summary>
    public ItemsControl? ParentNode { get; private set; }

    public BaseContextEntry? Entry { get; private set; }

    private Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion;
    private Dictionary<int, int>? dynamicInserted;
    private IconControl? myIconControl;

    public AdvancedContextMenuItem() { }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        MenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
        Dispatcher.UIThread.InvokeAsync(() => MenuService.NormaliseSeparators(this), DispatcherPriority.Loaded);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        MenuService.ClearDynamicItems(this, ref this.dynamicInserted);
    }

    public virtual void OnAdding(IAdvancedContainer container, ItemsControl parent, BaseContextEntry entry) {
        this.Container = container;
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

        if (this.Entry is SubListContextEntry list)
            MenuService.InsertItemNodes(this.Container!, this, list.ItemList);
    }

    public virtual void OnRemoving() {
        if (this.myIconControl != null) {
            this.myIconControl.Icon = null;
            this.myIconControl = null;
            this.Icon = null;
        }
        
        MenuService.ClearItemNodes(this);
    }

    public virtual void OnRemoved() {
        this.Container = null;
        this.ParentNode = null;
        this.Entry = null;
    }

    public virtual void UpdateCanExecute() {
    }

    public void StoreDynamicGroup(DynamicGroupPlaceholderContextObject groupPlaceholder, int index) {
        (this.dynamicInsertion ??= new Dictionary<int, DynamicGroupPlaceholderContextObject>())[index] = groupPlaceholder;
    }
}