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
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.AdvancedMenuService;

public class AdvancedContextMenu : ContextMenu, IAdvancedContainer, IAdvancedContextElement {
    // We maintain a map of the registries to the context menu. This is to
    // save memory, since we don't have to create a context menu for each handler
    private static readonly Dictionary<ContextRegistry, AdvancedContextMenu> contextMenus;

    public static readonly AttachedProperty<ContextRegistry?> ContextRegistryProperty = AvaloniaProperty.RegisterAttached<AdvancedContextMenu, Control, ContextRegistry?>("ContextRegistry");
    private static readonly AttachedProperty<AdvancedContextMenu?> AdvancedContextMenuProperty = AvaloniaProperty.RegisterAttached<AdvancedContextMenu, Control, AdvancedContextMenu?>("AdvancedContextMenu");

    public static readonly StyledProperty<string?> ContextCaptionProperty = AvaloniaProperty.Register<AdvancedContextMenu, string?>(nameof(ContextCaption));

    public string? ContextCaption {
        get => this.GetValue(ContextCaptionProperty);
        set => this.SetValue(ContextCaptionProperty, value);
    }

    private readonly IBinder<ContextRegistry> captionBinder;
    private readonly Dictionary<Type, Stack<Control>> itemCache;
    private readonly List<Control> owners;
    private Control? currentTarget;
    private Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion;
    private Dictionary<int, int>? dynamicInserted;
    private bool ignoreUpdateNormalisation;

    public IContextData? CapturedContext { get; private set; }
    IAdvancedContainer IAdvancedContextElement.Container => this;

    public AdvancedContextMenu() {
        this.captionBinder = new GetSetAutoUpdateAndEventPropertyBinder<ContextRegistry>(ContextCaptionProperty, nameof(this.ContextRegistry.CaptionChanged), (b) => b.Model.Caption, null);
        this.itemCache = new Dictionary<Type, Stack<Control>>();
        this.owners = new List<Control>();
        this.Opening += this.OnOpening;
        this.Closed += this.OnClosed;
        this.ignoreUpdateNormalisation = true;
    }

    public void UpdateSubListVisibility() {
        if (this.ignoreUpdateNormalisation)
            return;

        this.ignoreUpdateNormalisation = true;
        MenuService.NormaliseSeparators(this);
        this.ignoreUpdateNormalisation = false;
    }

    static AdvancedContextMenu() {
        contextMenus = new Dictionary<ContextRegistry, AdvancedContextMenu>();
        ContextRegistryProperty.Changed.AddClassHandler<Control, ContextRegistry?>((d, e) => OnContextRegistryChanged(d, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) {
        return new AdvancedContextMenuItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) {
        if (item is MenuItem || item is Separator || item is CaptionSeparator) {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    private void OnClosed(object? sender, RoutedEventArgs e) {
        this.ClearContext();
        this.ignoreUpdateNormalisation = true;
    }

    private void OnOpening(object? sender, CancelEventArgs e) {
        if (this.currentTarget == null) {
            e.Cancel = true;
            return;
        }

        this.CaptureContextFromObject(this.currentTarget);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        MenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
        Dispatcher.UIThread.InvokeAsync(() => {
            MenuService.NormaliseSeparators(this);
            this.ignoreUpdateNormalisation = false;
        }, DispatcherPriority.Loaded);

        if (this.ContextRegistry.IsOpened)
            this.ContextRegistry.OnClosed();

        this.ContextRegistry.OnOpened(this.CapturedContext ?? EmptyContext.Instance);
        this.captionBinder.Attach(this, this.ContextRegistry);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        MenuService.ClearDynamicItems(this, ref this.dynamicInserted);
        this.captionBinder.Detach();
        if (this.ContextRegistry.IsOpened)
            this.ContextRegistry.OnClosed();
    }

    private void OnOwnerRequestedContext(object? sender, ContextRequestedEventArgs e) {
        this.currentTarget = sender as Control;
    }

    private void ClearContext() {
        DataManager.ClearInheritedContextData(this);
        this.CapturedContext = null;
        this.currentTarget = null;
    }

    private void CaptureContextFromObject(InputElement inputElement) { ;
        DataManager.MakeInheritedContextData(this, this.CapturedContext = DataManager.GetFullContextData(inputElement));
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
                contextMenus[newValue] = menu = new AdvancedContextMenu() {
                    ContextRegistry = newValue
                };

                List<IContextObject> contextObjects = new List<IContextObject>();

                int i = 0;
                foreach (KeyValuePair<string, IContextGroup> entry in newValue.Groups) {
                    if (i++ != 0)
                        contextObjects.Add(new SeparatorEntry());

                    switch (entry.Value) {
                        case FixedContextGroup fixedGroup:     contextObjects.AddRange(fixedGroup.Items); break;
                        case DynamicContextGroup dynamicGroup: contextObjects.Add(new DynamicGroupPlaceholderContextObject(dynamicGroup)); break;
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

    public ContextRegistry ContextRegistry { get; init; }

    private void AddOwner(Control target) {
        this.owners.Add(target);
        target.ContextRequested += this.OnOwnerRequestedContext;
    }

    private bool RemoveOwnerAndShouldDestroy(Control target) {
        if (this.owners.Remove(target))
            target.ContextRequested -= this.OnOwnerRequestedContext;
        return this.owners.Count == 0;
    }

    public bool PushCachedItem(Type entryType, Control item) => MenuService.PushCachedItem(this.itemCache, entryType, item);

    public Control? PopCachedItem(Type entryType) => MenuService.PopCachedItem(this.itemCache, entryType);

    public Control CreateChildItem(IContextObject entry) => MenuService.CreateChildItem(this, entry);

    public void StoreDynamicGroup(DynamicGroupPlaceholderContextObject groupPlaceholder, int index) {
        (this.dynamicInsertion ??= new Dictionary<int, DynamicGroupPlaceholderContextObject>())[index] = groupPlaceholder;
    }

    public static void SetContextRegistry(Control obj, ContextRegistry? value) => obj.SetValue(ContextRegistryProperty, value);
}