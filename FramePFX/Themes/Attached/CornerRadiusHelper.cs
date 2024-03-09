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

namespace FramePFX.Themes.Attached
{
    public static class CornerRadiusHelper
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("Value", typeof(CornerRadius), typeof(CornerRadiusHelper), new PropertyMetadata(new CornerRadius(0)));

        public static void SetValue(DependencyObject element, CornerRadius value) => element.SetValue(ValueProperty, value);

        public static CornerRadius GetValue(DependencyObject element) => (CornerRadius) element.GetValue(ValueProperty);
    }
}