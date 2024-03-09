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
using System.Windows.Controls.Primitives;

namespace FramePFX.AttachedProperties
{
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, OnSelectAllOnFocusPropertyChanged));

        public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }

        public static bool GetSelectAllOnFocus(DependencyObject obj)
        {
            return (bool) obj.GetValue(SelectAllOnFocusProperty);
        }

        private static readonly RoutedEventHandler FocusHandler = BoxOnGotFocus;

        private static void OnSelectAllOnFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBoxBase tb)
            {
                tb.GotFocus -= FocusHandler;
                if ((bool) e.NewValue)
                {
                    tb.GotFocus += FocusHandler;
                }
            }
        }

        private static void BoxOnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBoxBase tb)
            {
                tb.Dispatcher.InvokeAsync(() =>
                {
                    if (GetSelectAllOnFocus(tb))
                    {
                        tb.SelectAll();
                    }
                });
            }
        }
    }
}