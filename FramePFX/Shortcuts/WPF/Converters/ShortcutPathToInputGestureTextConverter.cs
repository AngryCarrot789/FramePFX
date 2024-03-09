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
using System.Globalization;
using System.Windows.Data;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.WPF.Converters
{
    public class ShortcutPathToInputGestureTextConverter : IValueConverter
    {
        public string NoSuchShortcutFormat { get; set; } = "<{0}>";

        public string ShortcutFormat { get; set; } = null;

        public static string ShortcutToInputGestureText(string path, string shortcutFormat = null, string noSuchShortcutFormat = null)
        {
            GroupedShortcut shortcut = ShortcutManager.Instance.FindShortcutByPath(path);
            if (shortcut == null)
            {
                return noSuchShortcutFormat == null ? path : string.Format(noSuchShortcutFormat, path);
            }

            string representation = shortcut.Shortcut.ToString();
            return shortcutFormat == null ? representation : string.Format(shortcutFormat, representation);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string path && !string.IsNullOrWhiteSpace(path))
            {
                return ShortcutToInputGestureText(path, this.ShortcutFormat, this.NoSuchShortcutFormat);
            }
            else
            {
                throw new Exception("Invalid shortcut path (converter parameter): " + parameter);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}