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

namespace FramePFX.AttachedProperties
{
    public static class TopmostFocus
    {
        private class ZIndexExchangeData
        {
            public int OldFocusZIndex { get; set; }
        }

        public static readonly DependencyProperty FocusedZIndexProperty =
            DependencyProperty.RegisterAttached(
                "FocusedZIndex",
                typeof(int),
                typeof(TopmostFocus),
                new PropertyMetadata(0, OnZIndexPropertyChanged));

        private static readonly DependencyPropertyKey PreviousDataPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "PreviousData",
                typeof(ZIndexExchangeData),
                typeof(TopmostFocus),
                new PropertyMetadata(null));

        public static void SetFocusedZIndex(UIElement element, int value) => element.SetValue(FocusedZIndexProperty, value);
        public static int GetFocusedZIndex(UIElement element) => (int) element.GetValue(FocusedZIndexProperty);

        private static ZIndexExchangeData GetPreviousData(UIElement element)
        {
            ZIndexExchangeData data = (ZIndexExchangeData) element.GetValue(PreviousDataPropertyKey.DependencyProperty);
            if (data == null)
                element.SetValue(PreviousDataPropertyKey, data = new ZIndexExchangeData());
            return data;
        }

        private static readonly RoutedEventHandler GotFocusHandler = ControlOnGotFocus;
        private static readonly RoutedEventHandler LostFocusHandler = ControlOnLostFocus;

        private static void OnZIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement control)
            {
                control.GotFocus -= GotFocusHandler;
                control.LostFocus -= LostFocusHandler;
                if (e.NewValue is int)
                {
                    control.GotFocus += GotFocusHandler;
                    control.LostFocus += LostFocusHandler;
                }
            }
        }

        private static void ControlOnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                ZIndexExchangeData prevdata = GetPreviousData(element);
                prevdata.OldFocusZIndex = Panel.GetZIndex(element);
                int newIndex = GetFocusedZIndex(element);
                Panel.SetZIndex(element, newIndex);
            }
        }

        private static void ControlOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element && element.GetValue(PreviousDataPropertyKey.DependencyProperty) is ZIndexExchangeData data)
            {
                Panel.SetZIndex(element, data.OldFocusZIndex);
            }
        }
    }
}