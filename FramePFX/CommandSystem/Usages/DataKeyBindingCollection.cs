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
using System.Collections.Specialized;
using System.Windows;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.CommandSystem.Usages {
    public class DataKeyBindingCollection : FreezableCollection<DataKeyBinding> {
        private ContextData data;
        public DependencyObject Owner { get; private set; }

        public DataKeyBindingCollection() {
            ((INotifyCollectionChanged) this).CollectionChanged += this.OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    ((DataKeyBinding) e.NewItems[0]).Attach(this);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ((DataKeyBinding) e.OldItems[0]).Detatch();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (DataKeyBinding binding in e.OldItems) {
                        binding.Detatch();
                    }
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void OnAttach(DependencyObject owner) {
            this.Owner = owner;
            if (this.data != null)
                this.Owner?.SetValue(DataManager.ContextDataProperty, this.data);
        }

        public void UpdateDataContext() {
            this.data = new ContextData();
            foreach (DataKeyBinding binding in this) {
                if (binding.DataKey is DataKey key && binding.GetValue(DataKeyBinding.ValueProperty) is object obj) {
                    this.data.SetRaw(key.Id, obj);
                }
            }

            this.Owner?.SetValue(DataManager.ContextDataProperty, this.data);
        }

        public void OnDetatch() {
            this.Owner?.ClearValue(DataManager.ContextDataProperty);
            this.Owner = null;
        }
    }
}