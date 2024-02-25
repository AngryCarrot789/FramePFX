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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Editors.ResourceManaging.Autoloading.Controls {
    /// <summary>
    /// Interaction logic for ResourceLoaderDialog.xaml
    /// </summary>
    public partial class ResourceLoaderDialog : WindowEx {
        public static readonly DependencyProperty ResourceLoaderProperty = DependencyProperty.Register("ResourceLoader", typeof(ResourceLoader), typeof(ResourceLoaderDialog), new PropertyMetadata(null, (d, e) => ((ResourceLoaderDialog) d).OnResourceLoaderChanged((ResourceLoader) e.OldValue, (ResourceLoader) e.NewValue)));

        public ResourceLoader ResourceLoader {
            get => (ResourceLoader) this.GetValue(ResourceLoaderProperty);
            set => this.SetValue(ResourceLoaderProperty, value);
        }

        private readonly List<InvalidResourceEntryControl> controls;
        private bool isRemovingAllEntries;

        public ResourceLoaderDialog() {
            this.InitializeComponent();
            this.controls = new List<InvalidResourceEntryControl>();
            this.PART_ListBox.SelectionChanged += this.OnSelectedItemChanged;
            this.CalculateOwnerAndSetCentered();
        }

        private static void LoadResources(IEnumerable<BaseResource> resources, ResourceLoader loader) {
            foreach (BaseResource obj in resources) {
                if (obj is ResourceFolder folder) {
                    LoadResources(folder.Items, loader);
                }
                else {
                    ResourceItem item = (ResourceItem) obj;
                    if (!item.IsOnline) {
                        item.TryAutoEnable(loader);
                    }
                }
            }
        }

        public static bool TryLoadResources(params BaseResource[] resources) {
            return TryLoadResources(resources.ToList());
        }

        public static bool TryLoadResources(IEnumerable<BaseResource> resources) {
            ResourceLoader loader = new ResourceLoader();
            LoadResources(resources, loader);
            return ShowLoaderDialog(loader);
        }

        public static bool ShowLoaderDialog(ResourceLoader loader) {
            if (loader.Entries.Count < 1) {
                return true;
            }

            ResourceLoaderDialog dialog = new ResourceLoaderDialog();
            dialog.ResourceLoader = loader;
            return dialog.ShowDialog() == true;
        }

        private void OnResourceLoaderChanged(ResourceLoader oldLoader, ResourceLoader newLoader) {
            if (oldLoader != null) {
                oldLoader.EntryAdded -= this.OnEntryAdded;
                oldLoader.EntryRemoved -= this.OnEntryRemoved;

                for (int i = this.PART_ListBox.Items.Count - 1; i >= 0; i--) {
                    this.RemoveItemAt(i);
                }
            }

            if (newLoader != null) {
                newLoader.EntryAdded += this.OnEntryAdded;
                newLoader.EntryRemoved += this.OnEntryRemoved;

                int i = 0;
                foreach (InvalidResourceEntry entry in newLoader.Entries) {
                    this.InsertItemAt(i++, entry);
                }

                if (i > 0) {
                    this.PART_ListBox.SelectedIndex = 0;
                }
            }
        }

        private void OnEntryAdded(ResourceLoader loader, InvalidResourceEntry entry, int index) => this.InsertItemAt(index, entry);

        private void OnEntryRemoved(ResourceLoader loader, InvalidResourceEntry entry, int index) {
            this.RemoveItemAt(index);
            if (loader.Entries.Count < 1 && !this.isRemovingAllEntries) {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void InsertItemAt(int index, InvalidResourceEntry entry) {
            InvalidResourceEntryControl control = InvalidResourceEntryControl.NewInstance(entry.GetType());
            this.controls.Insert(index, control);
            control.AttachToEntry(entry);
            this.PART_ListBox.Items.Insert(index, new ListBoxItem() {Content = entry.DisplayName});
            entry.DisplayNameChanged += this.OnEntryDisplayNameChanged;
        }

        private void RemoveItemAt(int index) {
            InvalidResourceEntry entry = this.controls[index].Entry;
            entry.DisplayNameChanged -= this.OnEntryDisplayNameChanged;
            this.PART_ContentPresenter.Content = null;
            this.controls[index].DetatchFromEntry();
            this.controls.RemoveAt(index);
            this.PART_ListBox.Items.RemoveAt(index);
        }

        private void OnSelectedItemChanged(object sender, SelectionChangedEventArgs e) {
            if (this.PART_ListBox.Items.Count < 1) {
                return;
            }

            this.PART_ContentPresenter.Content = this.PART_ListBox.SelectedIndex == -1 ? null : this.controls[this.PART_ListBox.SelectedIndex];
        }

        private void OnEntryDisplayNameChanged(InvalidResourceEntry entry) {
            int index = this.ResourceLoader.Entries.IndexOf(entry);
            if (index != -1) {
                ((ListBoxItem) this.PART_ListBox.Items[index]).Content = entry.DisplayName;
            }
        }

        private void OfflineAll_Clicked(object sender, RoutedEventArgs e) {
            ResourceLoader loader = this.ResourceLoader;
            if (loader != null) {
                this.isRemovingAllEntries = true;
                for (int i = loader.Entries.Count - 1; i >= 0; i--) {
                    loader.RemoveEntryAt(i);
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        private void OfflineSelected_Click(object sender, RoutedEventArgs e) {
            int index;
            if (this.PART_ListBox.Items.Count > 0 && (index = this.PART_ListBox.SelectedIndex) != -1) {
                this.ResourceLoader.RemoveEntryAt(index);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            this.ResourceLoader = null;
        }
    }
}