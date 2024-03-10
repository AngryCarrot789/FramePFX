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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FramePFX.AdvancedMenuService.ContextService;
using FramePFX.AdvancedMenuService.ContextService.Controls;

namespace FramePFX.AdvancedMenuService
{
    public static class MenuService
    {
        public static IEnumerable<IContextEntry> CleanEntries(List<IContextEntry> entries)
        {
            IContextEntry lastEntry = null;
            for (int i = 0, end = entries.Count - 1; i <= end; i++)
            {
                IContextEntry entry = entries[i];
                if (!(entry is SeparatorEntry) || (i != 0 && i != end && !(lastEntry is SeparatorEntry)))
                {
                    yield return entry;
                }

                lastEntry = entry;
            }
        }

        internal static void InsertItemNodes(IAdvancedContainer container, ItemsControl parent, List<IContextEntry> entries)
        {
            ItemCollection items = parent.Items;
            foreach (IContextEntry entry in CleanEntries(entries))
            {
                FrameworkElement element = container.CreateChildItem(entry);
                if (element is AdvancedContextMenuItem menuItem)
                {
                    menuItem.OnAdding(container, parent, (BaseContextEntry) entry);
                    items.Add(menuItem);
                    menuItem.OnAdded();
                }
                else
                {
                    items.Add(element);
                }
            }
        }

        internal static void ClearItemNodes(ItemsControl control)
        {
            ItemCollection list = control.Items;
            IAdvancedContainer container;
            switch (control)
            {
                case AdvancedContextMenu a:
                    container = a;
                    break;
                case AdvancedContextMenuItem b:
                    container = b.Container;
                    break;
                default:
                    container = null;
                    break;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                FrameworkElement item = (FrameworkElement) list[i];
                if (item is AdvancedContextMenuItem menuItem)
                {
                    Type type = menuItem.Entry.GetType();
                    menuItem.OnRemoving();
                    list.RemoveAt(i);
                    menuItem.OnRemoved();
                    container?.PushCachedItem(type, item);
                }
                else
                {
                    list.RemoveAt(i);
                    if (container != null && item is Separator)
                        container.PushCachedItem(typeof(SeparatorEntry), item);
                }
            }
        }

        /// <summary>
        /// Default implementation for <see cref="IAdvancedContainer.PushCachedItem"/>
        /// </summary>
        public static bool PushCachedItem(Dictionary<Type, Stack<FrameworkElement>> itemCache, Type entryType, FrameworkElement item, int maxCache = 16)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (entryType == null)
                throw new ArgumentNullException(nameof(entryType));

            if (!itemCache.TryGetValue(entryType, out Stack<FrameworkElement> stack))
                itemCache[entryType] = stack = new Stack<FrameworkElement>();
            else if (stack.Count == maxCache)
                return false;

            stack.Push(item);
            return true;
        }

        /// <summary>
        /// Default implementation for <see cref="IAdvancedContainer.PopCachedItem"/>
        /// </summary>
        public static FrameworkElement PopCachedItem(Dictionary<Type, Stack<FrameworkElement>> itemCache, Type entryType)
        {
            if (entryType == null)
                throw new ArgumentNullException(nameof(entryType));

            if (itemCache.TryGetValue(entryType, out Stack<FrameworkElement> stack) && stack.Count > 0)
                return stack.Pop();

            return null;
        }

        /// <summary>
        /// Default implementation for <see cref="IAdvancedContainer.CreateChildItem"/>
        /// </summary>
        public static FrameworkElement CreateChildItem(IAdvancedContainer container, IContextEntry entry)
        {
            FrameworkElement element = container.PopCachedItem(entry.GetType());
            if (element == null)
            {
                switch (entry)
                {
                    case CommandContextEntry _:
                        element = new AdvancedContextCommandMenuItem();
                        break;
                    case EventContextEntry _:
                        element = new AdvancedContextEventMenuItem();
                        break;
                    case BaseContextEntry _:
                        element = new AdvancedContextMenuItem();
                        break;
                    case SeparatorEntry _:
                        element = new Separator();
                        break;
                    default: throw new Exception("Unknown item type: " + entry?.GetType());
                }
            }

            return element;
        }
    }
}