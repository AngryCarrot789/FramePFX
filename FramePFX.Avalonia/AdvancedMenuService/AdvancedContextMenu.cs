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
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FramePFX.AdvancedMenuService;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.AdvancedMenuService;

public class AdvancedContextMenu : ContextMenu, IAdvancedContainer, IAdvancedContextElement {
    // We maintain a map of the registries to the context menu. This is to
    // save memory, since we don't have to create a context menu for each handler
    private static readonly Dictionary<ContextRegistry, AdvancedContextMenu> contextMenus;

    public static readonly AttachedProperty<ContextRegistry?> ContextRegistryProperty = AvaloniaProperty.RegisterAttached<AdvancedContextMenu, Control, ContextRegistry?>("ContextRegistry");
    private static readonly AttachedProperty<AdvancedContextMenu?> AdvancedContextMenuProperty = AvaloniaProperty.RegisterAttached<AdvancedContextMenu, Control, AdvancedContextMenu?>("AdvancedContextMenu");

    private static void SetAdvancedContextMenu(Control obj, AdvancedContextMenu? value) => obj.SetValue(AdvancedContextMenuProperty, value);
    private static AdvancedContextMenu? GetAdvancedContextMenu(Control obj) => obj.GetValue(AdvancedContextMenuProperty);

    private readonly Dictionary<Type, Stack<Control>> itemCache;
    private readonly List<Control> owners;
    private Control? currentTarget;
    private Dictionary<int, DynamicGroupContextObject>? dynamicInsertion;
    private Dictionary<int, int>? dynamicInserted;

    public IContextData? Context { get; private set; }
    IAdvancedContainer IAdvancedContextElement.Container => this;

    protected override Type StyleKeyOverride => typeof(ContextMenu);

    public AdvancedContextMenu() {
        this.itemCache = new Dictionary<Type, Stack<Control>>();
        this.owners = new List<Control>();
        this.Opening += this.OnOpening;
        this.Closed += this.OnClosed;
    }

    private void OnClosed(object? sender, RoutedEventArgs e) {
        this.ClearContext();
    }

    private void OnOpening(object? sender, CancelEventArgs e) {
        if (this.currentTarget == null) {
            e.Cancel = true;
            return;
        }

        this.CaptureContextFromObject(this.currentTarget);
    }

    static AdvancedContextMenu() {
        contextMenus = new Dictionary<ContextRegistry, AdvancedContextMenu>();
        ContextRegistryProperty.Changed.AddClassHandler<Control, ContextRegistry?>((d, e) => OnContextRegistryChanged(d, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        MenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
        Dispatcher.UIThread.InvokeAsync(() => MenuService.ProcessSeparators(this), DispatcherPriority.Loaded);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        MenuService.ClearDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
    }

    private void OnOwnerRequestedContext(object? sender, ContextRequestedEventArgs e) {
        this.currentTarget = sender as Control;
    }

    private void ClearContext() {
        DataManager.ClearContextData(this);
        this.Context = null;
        this.currentTarget = null;
    }

    private void CaptureContextFromObject(InputElement inputElement) {
        DataManager.SetContextData(this, this.Context = DataManager.GetFullContextData(inputElement));
    }

    private static void OnContextRegistryChanged(Control target, ContextRegistry? oldValue, ContextRegistry? newValue) {
        if (ReferenceEquals(oldValue, newValue)) {
            return; // should be impossible... but just in case let's check
        }

        if (oldValue != null && contextMenus.TryGetValue(oldValue, out AdvancedContextMenu? oldMenu)) {
            if (oldMenu.RemoveOwnerAndShouldDestroy(target)) {
                contextMenus.Remove(oldValue); // remove the menu to prevent a memory leak I guess?
            }
        }

        if (newValue != null) {
            // Generate context menu, if required
            if (!contextMenus.TryGetValue(newValue, out AdvancedContextMenu? menu)) {
                contextMenus[newValue] = menu = new AdvancedContextMenu();
                List<IContextObject> contextObjects = new List<IContextObject>();

                int i = 0;
                foreach (KeyValuePair<string, IContextGroup> entry in newValue.Groups) {
                    if (i++ != 0)
                        contextObjects.Add(new SeparatorEntry());

                    if (entry.Value is FixedContextGroup fixedGroup) {
                        contextObjects.AddRange(fixedGroup.Items);
                    }
                    else if (entry.Value is DynamicContextGroup dynamicContextGroup) {
                        contextObjects.Add(new DynamicGroupContextObject(dynamicContextGroup));
                    }
                }
                
                MenuService.InsertItemNodes(menu, menu, contextObjects);
                
                // int sI = 0;
                // if (sI++ != 0)
                //     menu.Items.Add(new Separator());
                // int count = menu.Items.Count;
                // MenuService.InsertItemNodes(menu, menu, entry.Value.Items);
                // if (menu.Items.Count == count) { // no items added, so don't insert separator
                //     sI = 0;
                // }
            }

            // Slide in and add ContextRequested handler before the base ContextMenu
            // class does, so that we can update the target trying to open the menu
            menu.AddOwner(target);
            target.ContextMenu = menu;
        }
        else {
            target.ContextMenu = null;
        }
    }

    private void AddOwner(Control target) {
        this.owners.Add(target);
        target.ContextRequested += this.OnOwnerRequestedContext;
    }

    private bool RemoveOwnerAndShouldDestroy(Control target) {
        if (this.owners.Remove(target))
            target.ContextRequested -= this.OnOwnerRequestedContext;
        return this.owners.Count == 0;
    }

    public static void SetContextRegistry(Control obj, ContextRegistry? value) => obj.SetValue(ContextRegistryProperty, value);

    public bool PushCachedItem(Type entryType, Control item) => MenuService.PushCachedItem(this.itemCache, entryType, item);

    public Control? PopCachedItem(Type entryType) => MenuService.PopCachedItem(this.itemCache, entryType);

    public Control CreateChildItem(IContextObject entry) => MenuService.CreateChildItem(this, entry);
    
    public void StoreDynamicGroup(DynamicGroupContextObject group, int index) {
        (this.dynamicInsertion ??= new Dictionary<int, DynamicGroupContextObject>())[index] = group;
    }
}