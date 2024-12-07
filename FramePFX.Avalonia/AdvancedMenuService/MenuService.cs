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

    private static void NormalizeSeparators(ItemCollection items) {
        bool lastVisibleWasEntry = false;

        for (int i = 0; i < items.Count; i++) {
            object? current = items[i];
            if (current is Separator separator) {
                // A separator is visible only if the last visible item was an entry
                // and there's a visible entry after this separator
                if (!lastVisibleWasEntry || !HasVisibleEntryAfter(items, i)) {
                    separator.IsVisible = false;
                }
                else {
                    lastVisibleWasEntry = false; // Reset because we're on a separator
                }
            }
            else if (((Visual) current!).IsVisible) {
                lastVisibleWasEntry = true; // Mark that we found a visible entry
            }
        }
    }

    public static void ProcessSeparators(ItemsControl control) {
        ItemCollection list = control.Items;
        NormalizeSeparators(list);

        // This does not work so well
        // object? previousItem = null;
        // Separator? continueLastSeparator = null;
        // int continuousDisabledControlCount = 0;
        // for (int i = 0, endIndex = list.Count - 1; i <= endIndex; i++) {
        //     object? item = list[i];
        //     if (item is Separator separator) {
        //         if (continuousDisabledControlCount > 0 || i == 0 || i == endIndex || previousItem is Separator || (previousItem is Control prevControl && !prevControl.IsVisible)) {
        //             separator.IsVisible = false;
        //         }
        //         else {
        //             separator.IsVisible = true;
        //         }
        //         
        //         continuousDisabledControlCount = 0;
        //         continueLastSeparator = null;
        //     }
        //     else if (item is Control theControl) {
        //         if (!theControl.IsVisible) {
        //             if (previousItem is Separator prevSeparator) {
        //                 Debug.Assert(continuousDisabledControlCount == 0);
        //                 continueLastSeparator = prevSeparator;
        //             }
        //             else {
        //                 Debug.Assert(continuousDisabledControlCount > 0);
        //             }
        //
        //             continuousDisabledControlCount++;
        //         }
        //         else {
        //             if (continuousDisabledControlCount > 0) {
        //                 continuousDisabledControlCount = 0;
        //                 continueLastSeparator!.IsVisible = true;
        //                 continueLastSeparator = null;
        //             }
        //
        //         }
        //     }
        //     
        //     previousItem = item;
        // }
        //
        // if (continuousDisabledControlCount > 0) {
        //     continueLastSeparator!.IsVisible = false;
        // }
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
        ItemCollection list = control.Items;
        Control item = (Control) list[index]!;
        if (item is AdvancedContextMenuItem menuItem) {
            Type type = menuItem.Entry.GetType();
            menuItem.OnRemoving();
            list.RemoveAt(index);
            menuItem.OnRemoved();
            container?.PushCachedItem(type, item);
        }
        else {
            list.RemoveAt(index);
            if (container != null && item is Separator)
                container.PushCachedItem(typeof(SeparatorEntry), item);
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