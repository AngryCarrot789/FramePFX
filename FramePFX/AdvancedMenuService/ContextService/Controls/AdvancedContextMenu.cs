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

namespace FramePFX.AdvancedMenuService.ContextService.Controls
{
    /// <summary>
    /// A context menu that uses dynamic item generation, based on available context
    /// </summary>
    public class AdvancedContextMenu : ContextMenu, IAdvancedContainer
    {
        private static readonly ContextMenuEventHandler MenuOpenHandlerForGenerable = OnContextMenuOpeningForGenerable;
        private static readonly ContextMenuEventHandler MenuCloseHandlerForGenerable = OnContextMenuClosingForGenerable;

        public static readonly DependencyProperty ContextGeneratorProperty =
            DependencyProperty.RegisterAttached(
                "ContextGenerator",
                typeof(IContextGenerator),
                typeof(AdvancedContextMenu),
                new PropertyMetadata(null, OnContextGeneratorPropertyChanged));


        private readonly Dictionary<Type, Stack<FrameworkElement>> itemCache;

        public IContextData Context { get; internal set; }

        public AdvancedContextMenu()
        {
            this.itemCache = new Dictionary<Type, Stack<FrameworkElement>>();
        }

        public bool PushCachedItem(Type entryType, FrameworkElement item) => MenuService.PushCachedItem(this.itemCache, entryType, item);

        public FrameworkElement PopCachedItem(Type entryType) => MenuService.PopCachedItem(this.itemCache, entryType);

        public FrameworkElement CreateChildItem(IContextEntry entry) => MenuService.CreateChildItem(this, entry);

        private static void OnContextGeneratorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContextMenuService.RemoveContextMenuOpeningHandler(d, MenuOpenHandlerForGenerable);
            ContextMenuService.RemoveContextMenuClosingHandler(d, MenuCloseHandlerForGenerable);
            if (e.NewValue != null)
            {
                GetOrCreateContextMenu(d);
                ContextMenuService.AddContextMenuOpeningHandler(d, MenuOpenHandlerForGenerable);
                ContextMenuService.AddContextMenuClosingHandler(d, MenuCloseHandlerForGenerable);
            }
        }

        public static void OnContextMenuOpeningForGenerable(object sender, ContextMenuEventArgs e)
        {
            if (!(sender is DependencyObject sourceObject))
                return;

            AdvancedContextMenu menu = GetOrCreateContextMenu(sourceObject);
            if (menu.Items.Count > 0)
                return; // assume that they right clicked again while the menu was opened

            IContextGenerator generator = GetContextGenerator(sourceObject);
            if (generator == null)
                return;

            List<IContextEntry> list = new List<IContextEntry>();
            IContextData context = DataManager.GetFullContextData(sourceObject);
            generator.Generate(list, context);
            if (list.Count < 1)
            {
                // Handle the event so that the context menu does not show.
                // With zero items, it will only be a tiny rectangle for the
                // border, so there's no point in showing it
                e.Handled = true;
                return;
            }

            menu.Context = context;
            MenuService.InsertItemNodes(menu, menu, list);
        }

        public static void OnContextMenuClosingForGenerable(object sender, ContextMenuEventArgs e)
        {
            if (sender is DependencyObject obj && ContextMenuService.GetContextMenu(obj) is ContextMenu menu)
            {
                menu.Dispatcher.Invoke(() => MenuService.ClearItemNodes(menu), DispatcherPriority.DataBind);
            }
        }

        private static AdvancedContextMenu GetOrCreateContextMenu(DependencyObject targetElement)
        {
            ContextMenu menu = ContextMenuService.GetContextMenu(targetElement);
            if (!(menu is AdvancedContextMenu advancedMenu))
            {
                ContextMenuService.SetContextMenu(targetElement, advancedMenu = new AdvancedContextMenu());
                // advancedMenu.StaysOpen = true;
            }

            return advancedMenu;
        }

        public static void SetContextGenerator(DependencyObject element, IContextGenerator value) => element.SetValue(ContextGeneratorProperty, value);

        public static IContextGenerator GetContextGenerator(DependencyObject element) => (IContextGenerator) element.GetValue(ContextGeneratorProperty);
    }
}