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
using System.Windows.Controls.Primitives;

namespace FramePFX.Themes.Attached
{
    public static class TextBoxAutoSelect
    {
        private static readonly RoutedEventHandler Handler = ControlOnLoaded;

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TextBoxAutoSelect), new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                control.Loaded += Handler;
            }
        }

        private static void ControlOnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Control control)
            {
                control.Focus();
                if (control is TextBoxBase textbox)
                {
                    textbox.SelectAll();
                }

                control.Loaded -= Handler;
            }
        }

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool) element.GetValue(IsEnabledProperty);
        }
    }
}