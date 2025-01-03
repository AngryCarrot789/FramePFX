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
using FFmpeg.AutoGen;
using FramePFX.AdvancedMenuService;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.BaseFrontEnd.AdvancedMenuService;

/// <summary>
/// The advanced menu service provides dynamic context and top-level menu item generation
/// </summary>
public static class AdvancedMenuService {
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
            else if (((Visual) items[i]!).IsVisible) {
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
        // # Caption (general)
        // Rename
        // Change Colour
        // # Separator
        // Group Items
        // # Separator
        // # Caption (Modify Online State)
        // Set Offline
        // # Separator
        // Delete

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
                    separator.IsVisible = true;
                    lastVisibleWasEntry = false;
                }
            }
            else if (((Visual) current!).IsVisible) {
                lastVisibleWasEntry = true;
                if (current is CaptionSeparator && i > 0) {
                    GoBackAndHideSeparatorsUntilNonSeparatorReached(items, i);
                }
            }
        }
    }

    internal static void InsertItemNodes(IAdvancedMenuOrItem item, IReadOnlyList<IContextObject> entries) {
        InsertItemNodes(item, item.Items.Count, entries);
    }

    internal static void InsertItemNodes(IAdvancedMenuOrItem item, int index, IReadOnlyList<IContextObject> entries) {
        int i = index;
        foreach (IContextObject entry in entries) {
            if (entry is DynamicGroupPlaceholderContextObject) {
                item.StoreDynamicGroup((DynamicGroupPlaceholderContextObject) entry, i);
            }
            else {
                Control element = item.OwnerMenu!.CreateItem(entry);
                if (element is AdvancedMenuItem menuItem) {
                    menuItem.OnAdding(item.OwnerMenu, item, (BaseContextEntry) entry);
                    item.Items.Insert(i++, menuItem);
                    menuItem.ApplyStyling();
                    menuItem.ApplyTemplate();
                    menuItem.OnAdded();
                }
                else if (element is IAdvancedEntryConnection connection) {
                    connection.OnAdding(item.OwnerMenu, item, entry);
                    item.Items.Insert(i++, element);
                    element.ApplyStyling();
                    element.ApplyTemplate();
                    connection.OnAdded();
                }
                else {
                    item.Items.Insert(i++, element);
                }
            }
        }
    }

    internal static void ClearItemNodes(ItemsControl control) {
        ItemCollection list = control.Items;
        IAdvancedMenu? container = control as IAdvancedMenu ?? (control as IAdvancedMenuOrItem)?.OwnerMenu;
        for (int i = list.Count - 1; i >= 0; i--) {
            RemoveItemNode(container, control, i);
        }
    }

    internal static void ClearItemNodeRange(ItemsControl control, int index, int count) {
        IAdvancedMenu? container = control as IAdvancedMenu ?? (control as IAdvancedMenuOrItem)?.OwnerMenu;
        for (int i = index + count - 1; i >= index; i--) {
            RemoveItemNode(container, control, i);
        }
    }

    internal static void RemoveItemNode(IAdvancedMenu? container, ItemsControl control, int index) {
        ItemCollection items = control.Items;
        Control element = (Control) items[index]!;
        if (element is AdvancedMenuItem menuItem) {
            Type type = menuItem.Entry!.GetType();
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
            }
        }
    }

    /// <summary>
    /// Default implementation for <see cref="IAdvancedMenu.PushCachedItem"/>
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
    /// Default implementation for <see cref="IAdvancedMenu.PopCachedItem"/>
    /// </summary>
    public static Control? PopCachedItem(Dictionary<Type, Stack<Control>> itemCache, Type entryType) {
        if (entryType == null)
            throw new ArgumentNullException(nameof(entryType));

        if (itemCache.TryGetValue(entryType, out Stack<Control>? stack) && stack.Count > 0)
            return stack.Pop();

        return null;
    }

    /// <summary>
    /// Default implementation for <see cref="IAdvancedMenu.CreateItem"/>
    /// </summary>
    public static Control CreateChildItem(IAdvancedMenu menu, IContextObject entry) {
        Control? element = menu.PopCachedItem(entry.GetType());
        if (element == null) {
            switch (entry) {
                case CommandContextEntry _: element = new AdvancedCommandMenuItem(); break;
                case CustomContextEntry _:  element = new AdvancedCustomMenuItem(); break;
                case BaseContextEntry _:    element = new AdvancedMenuItem(); break;
                case SeparatorEntry _:      element = new Separator() { Margin = new Thickness(5, 2) }; break;
                case CaptionEntry _:        element = new CaptionSeparator(); break;
                default:                    throw new Exception("Unknown item type: " + entry?.GetType());
            }
        }

        return element;
    }

    public static void GenerateDynamicItems(IAdvancedMenuOrItem element, ref Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion, ref Dictionary<int, int>? dynamicInserted) {
        ClearDynamicItems(element, ref dynamicInserted);
        if (dynamicInsertion == null || dynamicInsertion.Count < 1) {
            return;
        }

        IAdvancedMenu menu = element.OwnerMenu ?? throw new InvalidOperationException("No menu available from menu item");
        IContextData context = menu.CapturedContext ?? EmptyContext.Instance;

        dynamicInserted ??= new Dictionary<int, int>();

        int offset = 0;
        List<KeyValuePair<int, DynamicGroupPlaceholderContextObject>> items = dynamicInsertion.OrderBy(x => x.Key).ToList();
        foreach (KeyValuePair<int, DynamicGroupPlaceholderContextObject> item in items) {
            // The key is a marker, we still need to post process the true index
            // This is also why we must insert from start to end
            int index = item.Key + offset;
            List<IContextObject> generated = item.Value.DynamicGroup.GenerateItems(context);
            InsertItemNodes(element, index, generated);
            dynamicInserted[index] = generated.Count;
            offset += generated.Count;
        }
    }

    public static int GenerateDynamicItems(IAdvancedMenuOrItem element, int logicalIndex, DynamicGroupPlaceholderContextObject group, ref Dictionary<int, int>? dynamicInserted) {
        IAdvancedMenu menu = element.OwnerMenu ?? throw new InvalidOperationException("No menu available from menu item");
        IContextData context = menu.CapturedContext ?? EmptyContext.Instance;

        dynamicInserted ??= new Dictionary<int, int>();

        // The key is a marker, we still need to post process the true index
        // This is also why we must insert from start to end
        List<IContextObject> generated = group.DynamicGroup.GenerateItems(context);
        InsertItemNodes(element, logicalIndex, generated);
        dynamicInserted[logicalIndex] = generated.Count;
        return generated.Count;
    }

    public static void ClearDynamicItems(IAdvancedMenuOrItem element, ref Dictionary<int, int>? dynamicInserted) {
        if (dynamicInserted == null || dynamicInserted.Count < 1) {
            return;
        }

        List<KeyValuePair<int, int>> items = dynamicInserted.OrderBy(x => x.Key).Reverse().ToList();
        foreach (KeyValuePair<int, int> item in items) {
            ClearItemNodeRange((ItemsControl) element, item.Key, item.Value);
        }

        dynamicInserted.Clear();
    }

    // logicalIndex is the index within the underlying list that a menu item's (in this case, ItemsControl) item source.
    // Absolute is the index within the ItemsControl.
    // This leaves us with a pickle, we have to figure out which dynamic parts to offset

    internal static void InsertItemNodesWithDynamicSupport(IAdvancedMenuOrItem item, int logicalIndex, IReadOnlyList<IContextObject> entries, ref Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion, ref Dictionary<int, int>? dynamicInserted) {
        // ClearDynamicItems(menu, ref dynamicInserted);
        int idx = GetAbsoluteOffset(logicalIndex, dynamicInserted, true);
        if (!item.IsOpen)
            Debug.Assert(idx == logicalIndex);
        
        foreach (IContextObject entry in entries) {
            if (entry is DynamicGroupPlaceholderContextObject) {
                OffsetDynamics_Logical(logicalIndex, 1, ref dynamicInsertion);
                (dynamicInsertion ??= new Dictionary<int, DynamicGroupPlaceholderContextObject>())[idx] = (DynamicGroupPlaceholderContextObject) entry;
                if (item.IsOpen) {
                    int items = GenerateDynamicItems(item, idx, (DynamicGroupPlaceholderContextObject) entry, ref dynamicInserted);
                    OffsetDynamics_Absolute(idx, items, ref dynamicInserted);
                    idx += items;
                    Debug.WriteLine($"Inserted dynamic items after dynamic insertion. Count = {items}");
                }
            }
            else {
                Control element = item.OwnerMenu!.CreateItem(entry);
                if (element is AdvancedMenuItem menuItem) {
                    menuItem.OnAdding(item.OwnerMenu, item, (BaseContextEntry) entry);
                    item.Items.Insert(idx++, menuItem);
                    menuItem.ApplyStyling();
                    menuItem.ApplyTemplate();
                    menuItem.OnAdded();
                }
                else if (element is IAdvancedEntryConnection connection) {
                    connection.OnAdding(item.OwnerMenu, item, entry);
                    item.Items.Insert(idx++, element);
                    element.ApplyStyling();
                    element.ApplyTemplate();
                    connection.OnAdded();
                }
                else {
                    item.Items.Insert(idx++, element);
                }
            }
        }
    }

    internal static void OffsetDynamics_Absolute(int logicalIndex, int absoluteOffset, ref Dictionary<int, int>? dynamicInserted) {
        if (dynamicInserted != null) {
            Dictionary<int, int> newDynamicInserted = new Dictionary<int, int>(dynamicInserted.Count);
            List<KeyValuePair<int, int>> items = dynamicInserted.OrderBy(x => x.Key).ToList();
            foreach (KeyValuePair<int, int> pair in items) {
                // The logical index is <= the insertion index
                if (logicalIndex < pair.Key) {
                    newDynamicInserted[pair.Key + absoluteOffset] = pair.Value;
                }
                // The logical index is less than the insertion index, therefore, no need to offset
                else {
                    newDynamicInserted[pair.Key] = pair.Value;
                }
            }

            dynamicInserted = newDynamicInserted;
        }
    }
    
    internal static void OffsetDynamics_Logical(int logicalIndex, int logicalOffset, ref Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion) {
        if (dynamicInsertion != null) {
            Dictionary<int, DynamicGroupPlaceholderContextObject> newDynamicInsertion = new Dictionary<int, DynamicGroupPlaceholderContextObject>(dynamicInsertion.Count);
            List<KeyValuePair<int, DynamicGroupPlaceholderContextObject>> items = dynamicInsertion.OrderBy(x => x.Key).ToList();
            foreach (KeyValuePair<int, DynamicGroupPlaceholderContextObject> pair in items) {
                // The logical index is <= the insertion index
                if (logicalIndex < pair.Key) {
                    newDynamicInsertion[pair.Key + logicalOffset] = pair.Value;
                }
                // The logical index is less than the insertion index, therefore, no need to offset
                else {
                    newDynamicInsertion[pair.Key] = pair.Value;
                }
            }

            dynamicInsertion = newDynamicInsertion;
        }
    }

    internal static void RemoveItemNodesWithDynamicSupport(IAdvancedMenu menu, ItemsControl ic, int logicalIndex, IReadOnlyList<IContextObject> entries, ref Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion, ref Dictionary<int, int>? dynamicInserted) {
        int trueOffset = GetAbsoluteOffset(logicalIndex, dynamicInserted, false);
        for (int unused = 0; unused < entries.Count; unused++) {
            if (dynamicInserted != null && dynamicInserted.TryGetValue(trueOffset, out int count)) {
                ClearItemNodeRange(ic, trueOffset, count);
                dynamicInserted.Remove(trueOffset);
            }
            else if (dynamicInsertion == null || !dynamicInsertion.ContainsKey(trueOffset)) {
                RemoveItemNode(menu, ic, trueOffset);
            }
            else {
                dynamicInsertion.Remove(trueOffset);
            }
        }
    }

    private static int GetAbsoluteOffset(int logicalIndex, Dictionary<int, int>? dynamicInserted, bool inclusive) {
        if (dynamicInserted != null && dynamicInserted.Count > 0) {
            int trueOffset = logicalIndex;
            foreach (KeyValuePair<int, int> inserted in dynamicInserted) {
                if (inclusive) {
                    if (logicalIndex >= inserted.Key) {
                        trueOffset += inserted.Value;
                    }
                }
                else {
                    if (logicalIndex > inserted.Key) {
                        trueOffset += inserted.Value;
                    }
                }
            }

            return trueOffset;
        }

        return logicalIndex;
    }
}