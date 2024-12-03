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
using Avalonia.Controls;
using FramePFX.AdvancedMenuService;

namespace FramePFX.Avalonia.AdvancedMenuService;

public static class MenuService {
    public static IEnumerable<IContextObject> CleanEntries(IReadOnlyList<IContextObject> entries) {
        IContextObject? lastEntry = null;
        for (int i = 0, end = entries.Count - 1; i <= end; i++) {
            IContextObject entry = entries[i];
            if (!(entry is SeparatorEntry) || (i != 0 && i != end && !(lastEntry is SeparatorEntry))) {
                yield return entry;
            }

            lastEntry = entry;
        }
    }

    internal static void InsertItemNodes(IAdvancedContainer container, ItemsControl parent, IReadOnlyList<IContextObject> entries) {
        ItemCollection items = parent.Items;
        foreach (IContextObject entry in CleanEntries(entries)) {
            Control element = container.CreateChildItem(entry);
            if (element is AdvancedContextMenuItem menuItem) {
                menuItem.OnAdding(container, parent, (BaseContextEntry) entry);
                items.Add(menuItem);
                menuItem.ApplyStyling();
                menuItem.ApplyTemplate();
                menuItem.OnAdded();
            }
            else {
                items.Add(element);
            }
        }
    }

    internal static void ClearItemNodes(ItemsControl control) {
        ItemCollection list = control.Items;
        IAdvancedContainer container;
        switch (control) {
            case AdvancedContextMenu a: container = a; break;
            case AdvancedContextMenuItem b: container = b.Container; break;
            default: container = null; break;
        }

        for (int i = list.Count - 1; i >= 0; i--) {
            Control item = (Control) list[i]!;
            if (item is AdvancedContextMenuItem menuItem) {
                Type type = menuItem.Entry.GetType();
                menuItem.OnRemoving();
                list.RemoveAt(i);
                menuItem.OnRemoved();
                container?.PushCachedItem(type, item);
            }
            else {
                list.RemoveAt(i);
                if (container != null && item is Separator)
                    container.PushCachedItem(typeof(SeparatorEntry), item);
            }
        }
    }

    /// <summary>
    /// Default implementation for <see cref="IAdvancedContainer.PushCachedItem"/>
    /// </summary>
    public static bool PushCachedItem(Dictionary<Type, Stack<Control>> itemCache, Type entryType, Control item, int maxCache = 16) {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (entryType == null)
            throw new ArgumentNullException(nameof(entryType));

        if (!itemCache.TryGetValue(entryType, out Stack<Control>? stack))
            itemCache[entryType] = stack = new Stack<Control>();
        else if (stack.Count == maxCache)
            return false;

        stack.Push(item);
        return true;
    }

    /// <summary>
    /// Default implementation for <see cref="IAdvancedContainer.PopCachedItem"/>
    /// </summary>
    public static Control? PopCachedItem(Dictionary<Type, Stack<Control>> itemCache, Type entryType) {
        if (entryType == null)
            throw new ArgumentNullException(nameof(entryType));

        if (itemCache.TryGetValue(entryType, out Stack<Control>? stack) && stack.Count > 0)
            return stack.Pop();

        return null;
    }

    /// <summary>
    /// Default implementation for <see cref="IAdvancedContainer.CreateChildItem"/>
    /// </summary>
    public static Control CreateChildItem(IAdvancedContainer container, IContextObject entry) {
        Control? element = container.PopCachedItem(entry.GetType());
        if (element == null) {
            switch (entry) {
                case CommandContextEntry _: element = new AdvancedContextCommandMenuItem(); break;
                case BaseContextEntry _: element = new AdvancedContextMenuItem(); break;
                case SeparatorEntry _: element = new Separator(); break;
                default: throw new Exception("Unknown item type: " + entry?.GetType());
            }
        }

        return element;
    }
}