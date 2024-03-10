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
using FramePFX.Interactivity.Contexts;

namespace FramePFX.AdvancedMenuService.RegularMenuService
{
    /// <summary>
    /// A menu item which can be placed in any menu or menu item that uses a context generator to generate its children
    /// </summary>
    public class AdvancedMenuItem : MenuItem, IAdvancedContainer
    {
        public static readonly DependencyProperty ContextGeneratorProperty = DependencyProperty.Register("ContextGenerator", typeof(IContextGenerator), typeof(AdvancedMenuItem), new PropertyMetadata(null, (d, e) => ((AdvancedMenuItem) d).OnContextGeneratorChanged((IContextGenerator) e.OldValue, (IContextGenerator) e.NewValue)));

        public IContextGenerator ContextGenerator
        {
            get => (IContextGenerator) this.GetValue(ContextGeneratorProperty);
            set => this.SetValue(ContextGeneratorProperty, value);
        }

        public IContextData Context { get; private set; }

        private readonly Dictionary<Type, Stack<FrameworkElement>> itemCache;

        public AdvancedMenuItem()
        {
            this.itemCache = new Dictionary<Type, Stack<FrameworkElement>>();
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
            this.SubmenuOpened += this.OnSubmenuOpened;
            this.SubmenuClosed += this.OnSubmenuClosed;
        }

        private void OnSubmenuOpened(object sender, RoutedEventArgs e)
        {
            this.Context = ContextCapturingMenu.GetCapturedContextData(this) ?? DataManager.GetFullContextData(this);
            this.GenerateChildren();
        }

        private void OnSubmenuClosed(object sender, RoutedEventArgs e)
        {
            MenuService.ClearItemNodes(this);
            this.Items.Add(null);
        }

        private void OnContextGeneratorChanged(IContextGenerator oldGen, IContextGenerator newGen)
        {
            if (this.IsLoaded)
                this.GenerateChildren();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Context = ContextCapturingMenu.GetCapturedContextData(this) ?? DataManager.GetFullContextData(this);
            this.GenerateChildren();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Context = null;
        }

        private void GenerateChildren()
        {
            if (this.Context == null)
                return;

            MenuService.ClearItemNodes(this);
            IContextGenerator generator = this.ContextGenerator;
            if (generator != null)
            {
                List<IContextEntry> list = new List<IContextEntry>();
                generator.Generate(list, this.Context);
                if (list.Count > 0)
                    MenuService.InsertItemNodes(this, this, list);
            }
        }

        public bool PushCachedItem(Type entryType, FrameworkElement item) => MenuService.PushCachedItem(this.itemCache, entryType, item, 32);

        public FrameworkElement PopCachedItem(Type entryType) => MenuService.PopCachedItem(this.itemCache, entryType);

        public FrameworkElement CreateChildItem(IContextEntry entry) => MenuService.CreateChildItem(this, entry);
    }
}