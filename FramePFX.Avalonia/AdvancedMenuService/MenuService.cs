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
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using FramePFX.AdvancedMenuService;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.AdvancedMenuService;

public static class MenuService {
    private static bool HasVisibleEntryAfter(ItemCollection items, int index) {
        for (int i = index + 1; i < items.Count; i++) {
            if (!(items[i] is Separator) && ((Visual) items[i]!).IsVisible)
                return true;
        }

        return false;
    }
    
    private static bool IsSeparatorNotNeeded(ItemCollection items, int index) {
        if (items[index] is Separator) {
            if (index > 0 && items[index - 1] is CaptionSeparator prevCaption && prevCaption.IsVisible)
                return true;
            return (index < items.Count - 1) && items[index + 1] is CaptionSeparator nextCaption && nextCaption.IsVisible;
        }

        return false;
    }
    
    // I think it needs to be longer
    private static void GoBackAndHideSeparatorsUntilNonSeparatorReached(ItemCollection items, int index) {
        for (int i = index - 1; i >= 0; i--) {
            if (items[i] is Separator separator) {
                separator.IsVisible = false;
            }
            else {
                break;
            }
        }
    }

    /// <summary>
    /// Calculates which controls are visible. It is assumed the control's items are all
    /// fully loaded and their current visibility reflects their true underlying state 
    /// </summary>
    /// <param name="control">The control whose items should be processed</param>
    public static void NormaliseSeparators(ItemsControl control) {
        ItemCollection items = control.Items;
        bool lastVisibleWasEntry = false;
        for (int i = 0; i < items.Count; i++) {
            object? current = items[i];
            if (current is Separator separator) {
                if (IsSeparatorNotNeeded(items, i)) {
                    separator.IsVisible = false;
                }
                else if (!lastVisibleWasEntry || !HasVisibleEntryAfter(items, i)) {
                    separator.IsVisible = false;
                }
                else {
                    lastVisibleWasEntry = false;
                }
            }
            else if (((Visual) current!).IsVisible) {
                lastVisibleWasEntry = true;
                if (current is CaptionSeparator && i > 0 && items[i - 1] is Separator) {
                    GoBackAndHideSeparatorsUntilNonSeparatorReached(items, i);
                }
            }
        }
    }

    public static IEnumerable<IContextObject> CleanEntries(IReadOnlyList<IContextObject> entries) {
        return entries;
        // IContextObject? lastEntry = null;
        // for (int i = 0, end = entries.Count - 1; i <= end; i++) {
        //     IContextObject entry = entries[i];
        //     if (!(entry is SeparatorEntry) || (i != 0 && i != end && !(lastEntry is SeparatorEntry))) {
        //         yield return entry;
        //     }
        //     lastEntry = entry;
        // }
    }

    internal static void InsertItemNodes(IAdvancedContainer container, ItemsControl parent, IReadOnlyList<IContextObject> entries) {
        InsertItemNodes(container, parent, parent.Items.Count, entries);
    }

    internal static void InsertItemNodes(IAdvancedContainer container, ItemsControl parent, int index, IReadOnlyList<IContextObject> entries) {
        ItemCollection items = parent.Items;
        int i = index;
        foreach (IContextObject entry in CleanEntries(entries)) {
            if (entry is DynamicGroupContextObject) {
                (parent as IAdvancedContextElement)?.StoreDynamicGroup((DynamicGroupContextObject) entry, i);
                continue;
            }

            Control element = container.CreateChildItem(entry);
            if (element is AdvancedContextMenuItem menuItem) {
                menuItem.OnAdding(container, parent, (BaseContextEntry) entry);
                items.Insert(i++, menuItem);
                menuItem.ApplyStyling();
                menuItem.ApplyTemplate();
                menuItem.OnAdded();
            }
            else if (element is IAdvancedEntryConnection connection) {
                connection.OnAdding(container, parent, entry);
                items.Insert(i++, element);
                element.ApplyStyling();
                element.ApplyTemplate();
                connection.OnAdded();
            }
            else {
                items.Insert(i++, element);
            }
        }
    }

    internal static void ClearItemNodes(ItemsControl control) {
        ItemCollection list = control.Items;
        IAdvancedContainer? container = (control as IAdvancedContextElement)?.Container;
        for (int i = list.Count - 1; i >= 0; i--) {
            RemoveItemNode(control, i, container);
        }
    }

    internal static void ClearItemNodeRange(ItemsControl control, int index, int count) {
        IAdvancedContainer? container = (control as IAdvancedContextElement)?.Container;
        for (int i = index + count - 1; i >= index; i--) {
            RemoveItemNode(control, i, container);
        }
    }

    internal static void RemoveItemNode(ItemsControl control, int index, IAdvancedContainer? container) {
        ItemCollection items = control.Items;
        Control element = (Control) items[index]!;
        if (element is AdvancedContextMenuItem menuItem) {
            Type type = menuItem.Entry.GetType();
            menuItem.OnRemoving();
            items.RemoveAt(index);
            menuItem.OnRemoved();
            container?.PushCachedItem(type, element);
        }
        else if (element is IAdvancedEntryConnection connection) {
            Type type = connection.Entry!.GetType();
            connection.OnRemoving();
            items.RemoveAt(index);
            connection.OnRemoved();
            container?.PushCachedItem(type, element);
        }
        else {
            items.RemoveAt(index);
            if (container != null) {
                if (element is Separator)
                    container.PushCachedItem(typeof(SeparatorEntry), element);
                else if (element is CaptionSeparator)
                    container.PushCachedItem(typeof(CaptionSeparator), element);
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
                case CaptionEntry _: element = new CaptionSeparator(); break;
                default: throw new Exception("Unknown item type: " + entry?.GetType());
            }
        }

        return element;
    }

    public static void GenerateDynamicItems(IAdvancedContextElement element, ref Dictionary<int, DynamicGroupContextObject>? dynamicInsertion, ref Dictionary<int, int>? dynamicInserted) {
        ClearDynamicItems(element, ref dynamicInsertion, ref dynamicInserted);
        if (dynamicInsertion == null || dynamicInsertion.Count < 1) {
            return;
        }

        IContextData context = element.Context ?? EmptyContext.Instance;

        dynamicInserted ??= new Dictionary<int, int>();

        int offset = 0;
        List<KeyValuePair<int, DynamicGroupContextObject>> items = dynamicInsertion.OrderBy(x => x.Key).ToList();
        foreach (KeyValuePair<int, DynamicGroupContextObject> item in items) {
            // The key is a marker, we still need to post process the true index
            // This is also why we must insert from start to end
            int index = item.Key + offset;
            List<IContextObject> generated = item.Value.DynamicGroup.GenerateItems(context);
            InsertItemNodes(element.Container, (ItemsControl) element, index, generated);
            dynamicInserted[index] = generated.Count;
            offset += generated.Count;
        }
    }

    public static void ClearDynamicItems(IAdvancedContextElement element, ref Dictionary<int, DynamicGroupContextObject>? dynamicInsertion, ref Dictionary<int, int>? dynamicInserted) {
        if (dynamicInserted == null || dynamicInserted.Count < 1) {
            return;
        }

        List<KeyValuePair<int, int>> items = dynamicInserted.OrderBy(x => x.Key).Reverse().ToList();
        foreach (KeyValuePair<int, int> item in items) {
            ClearItemNodeRange((ItemsControl) element, item.Key, item.Value);
        }

        dynamicInserted.Clear();
    }
}