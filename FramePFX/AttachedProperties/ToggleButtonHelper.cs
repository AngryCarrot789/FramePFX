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

using System.Windows;
using System.Windows.Controls;
using FramePFX.Utils;

namespace FramePFX.AttachedProperties {
    public static class ToggleButtonHelper {
        public static readonly DependencyProperty IsDisabledWhenIsCheckedIsNullProperty = DependencyProperty.RegisterAttached(
            "IsDisabledWhenIsCheckedIsNull", typeof(bool), typeof(ToggleButtonHelper), new PropertyMetadata(BoolBox.False, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is CheckBox cb) {
                cb.Checked -= OnCheckChanged;
                cb.Unchecked -= OnCheckChanged;
                if (e.NewValue is bool b && b) {
                    cb.Checked += OnCheckChanged;
                    cb.Unchecked += OnCheckChanged;
                }
            }
        }

        public static void SetIsDisabledWhenIsCheckedIsNull(DependencyObject element, bool value) {
            element.SetValue(IsDisabledWhenIsCheckedIsNullProperty, value);
        }

        public static bool GetIsDisabledWhenIsCheckedIsNull(DependencyObject element) {
            return (bool) element.GetValue(IsDisabledWhenIsCheckedIsNullProperty);
        }

        private static void OnCheckChanged(object sender, RoutedEventArgs e) {
            if (sender is CheckBox cb && GetIsDisabledWhenIsCheckedIsNull(cb)) {
                cb.IsEnabled = cb.IsChecked != null;
            }
        }
    }
}