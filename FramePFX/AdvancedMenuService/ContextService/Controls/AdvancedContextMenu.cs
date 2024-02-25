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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.AdvancedMenuService.ContextService.Controls {
    public class AdvancedContextMenu : ContextMenu, IAdvancedMenu {
        public static readonly DependencyProperty ContextGeneratorProperty =
            DependencyProperty.RegisterAttached(
                "ContextGenerator",
                typeof(IContextGenerator),
                typeof(AdvancedContextMenu),
                new PropertyMetadata(null, OnContextGeneratorPropertyChanged));

        private static readonly ContextMenuEventHandler MenuOpenHandlerForGenerable = OnContextMenuOpeningForGenerable;
        private static readonly ContextMenuEventHandler MenuCloseHandlerForGenerable = OnContextMenuClosingForGenerable;

        internal IContextData ContextOnMenuOpen;

        private readonly Dictionary<Type, Stack<FrameworkElement>> itemCache;

        public AdvancedContextMenu() {
            this.itemCache = new Dictionary<Type, Stack<FrameworkElement>>();
        }

        public bool PushCachedItem(Type entryType, FrameworkElement item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (entryType == null)
                throw new ArgumentNullException(nameof(entryType));

            if (!this.itemCache.TryGetValue(entryType, out Stack<FrameworkElement> stack)) {
                this.itemCache[entryType] = stack = new Stack<FrameworkElement>();
            }
            else if (stack.Count == 16) {
                return false;
            }

            stack.Push(item);
            return true;
        }

        public FrameworkElement PopCachedItem(Type entryType) {
            if (entryType == null)
                throw new ArgumentNullException(nameof(entryType));

            if (this.itemCache.TryGetValue(entryType, out Stack<FrameworkElement> stack) && stack.Count > 0) {
                return stack.Pop();
            }

            return null;
        }

        public FrameworkElement CreateChildItem(IContextEntry entry) {
            FrameworkElement element = this.PopCachedItem(entry.GetType());
            if (element == null) {
                switch (entry) {
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

        private static void OnContextGeneratorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue == e.NewValue) {
                return;
            }

            ContextMenuService.RemoveContextMenuOpeningHandler(d, MenuOpenHandlerForGenerable);
            ContextMenuService.RemoveContextMenuClosingHandler(d, MenuCloseHandlerForGenerable);
            if (e.NewValue != null) {
                GetOrCreateContextMenu(d);
                ContextMenuService.AddContextMenuOpeningHandler(d, MenuOpenHandlerForGenerable);
                ContextMenuService.AddContextMenuClosingHandler(d, MenuCloseHandlerForGenerable);
            }
        }

        internal static void InsertItemNodes(AdvancedContextMenu menu, ItemsControl parent, List<IContextEntry> entries) {
            ItemCollection items = parent.Items;
            foreach (IContextEntry entry in CleanEntries(entries)) {
                FrameworkElement element = menu.CreateChildItem(entry);
                AdvancedContextMenuItem parentNode = parent as AdvancedContextMenuItem;
                if (element is AdvancedContextMenuItem menuItem) {
                    menuItem.OnAdding(menu, parentNode, (BaseContextEntry) entry);
                    items.Add(menuItem);
                    menuItem.OnAdded();
                }
                else {
                    items.Add(element);
                }
            }
        }

        internal static void ClearItemNodes(ItemsControl control) {
            ItemCollection list = control.Items;
            AdvancedContextMenu menu;
            switch (control) {
                case AdvancedContextMenu a:
                    menu = a;
                    break;
                case AdvancedContextMenuItem b:
                    menu = b.Menu;
                    break;
                default:
                    menu = null;
                    break;
            }

            for (int i = list.Count - 1; i >= 0; i--) {
                FrameworkElement item = (FrameworkElement) list[i];
                if (item is AdvancedContextMenuItem menuItem) {
                    Type type = menuItem.Entry.GetType();
                    menuItem.OnRemoving();
                    list.RemoveAt(i);
                    menuItem.OnRemoved();
                    menu?.PushCachedItem(type, item);
                }
                else {
                    list.RemoveAt(i);
                    if (menu != null && item is Separator) {
                        menu.PushCachedItem(typeof(SeparatorEntry), item);
                    }
                }
            }
        }

        public static void OnContextMenuOpeningForGenerable(object sender, ContextMenuEventArgs e) {
            if (sender is DependencyObject sourceObject && e.OriginalSource is DependencyObject targetObject) {
                AdvancedContextMenu menu = GetOrCreateContextMenu(sourceObject);
                if (menu.Items.Count > 0) {
                    // assume that they right clicked again while the menu was opened
                    return;
                }

                IContextGenerator generator = GetContextGenerator(sourceObject);
                if (generator != null) {
                    List<IContextEntry> list = new List<IContextEntry>();
                    IContextData context = DataManager.GetFullContextData(sourceObject);
                    generator.Generate(list, context);
                    if (list.Count < 1) {
                        e.Handled = true;
                        return;
                    }

                    menu.ContextOnMenuOpen = context;
                    InsertItemNodes(menu, menu, list);
                }
            }
        }

        public static void OnContextMenuClosingForGenerable(object sender, ContextMenuEventArgs e) {
            if (sender is DependencyObject obj && ContextMenuService.GetContextMenu(obj) is ContextMenu menu) {
                menu.Dispatcher.Invoke(() => ClearItemNodes(menu), DispatcherPriority.DataBind);
            }
        }

        public static IEnumerable<IContextEntry> CleanEntries(List<IContextEntry> entries) {
            IContextEntry lastEntry = null;
            for (int i = 0, end = entries.Count - 1; i <= end; i++) {
                IContextEntry entry = entries[i];
                if (!(entry is SeparatorEntry) || (i != 0 && i != end && !(lastEntry is SeparatorEntry))) {
                    yield return entry;
                }

                lastEntry = entry;
            }
        }

        private static AdvancedContextMenu GetOrCreateContextMenu(DependencyObject targetElement) {
            ContextMenu menu = ContextMenuService.GetContextMenu(targetElement);
            if (!(menu is AdvancedContextMenu advancedMenu)) {
                ContextMenuService.SetContextMenu(targetElement, advancedMenu = new AdvancedContextMenu());
                advancedMenu.StaysOpen = true;
            }

            return advancedMenu;
        }

        public static void SetContextGenerator(DependencyObject element, IContextGenerator value) => element.SetValue(ContextGeneratorProperty, value);

        public static IContextGenerator GetContextGenerator(DependencyObject element) => (IContextGenerator) element.GetValue(ContextGeneratorProperty);
    }
}