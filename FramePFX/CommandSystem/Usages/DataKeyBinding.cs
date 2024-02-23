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

using System.Windows;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.CommandSystem.Usages {
    public class DataKeyBinding : Freezable {
        public static readonly DependencyProperty DataKeyProperty = DependencyProperty.Register("DataKey", typeof(DataKey), typeof(DataKeyBinding), new PropertyMetadata(null, OnKeyChanged));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DataKeyBinding), new PropertyMetadata(null, OnValueChanged));

        public DataKeyBindingCollection Collection { get; private set; }

        public DataKey DataKey {
            get => (DataKey) this.GetValue(DataKeyProperty);
            set => this.SetValue(DataKeyProperty, value);
        }

        public object Value {
            get => (object) this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((DataKeyBinding) d).Collection?.UpdateDataContext();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((DataKeyBinding) d).Collection?.UpdateDataContext();
        }

        private void OnPropertiesChanged() {

        }

        protected override Freezable CreateInstanceCore() => new DataKeyBinding();

        public void Detatch() {

        }

        public void Attach(DataKeyBindingCollection collection) {
            this.Collection = collection;
        }
    }
}