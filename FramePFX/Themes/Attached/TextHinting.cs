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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FramePFX.Themes.Attached
{
    public static class TextHinting
    {
        public static readonly DependencyProperty ShowWhenFocusedProperty =
            DependencyProperty.RegisterAttached(
                "ShowWhenFocused",
                typeof(bool),
                typeof(TextHinting),
                new FrameworkPropertyMetadata(false));

        public static void SetShowWhenFocused(Control control, bool value)
        {
            if (control is TextBoxBase || control is PasswordBox)
            {
                control.SetValue(ShowWhenFocusedProperty, value);
            }

            throw new ArgumentException("Control was not a textbox", nameof(control));
        }

        public static bool GetShowWhenFocused(Control control)
        {
            if (control is TextBoxBase || control is PasswordBox)
            {
                return (bool) control.GetValue(ShowWhenFocusedProperty);
            }

            throw new ArgumentException("Control was not a textbox", nameof(control));
        }
    }
}