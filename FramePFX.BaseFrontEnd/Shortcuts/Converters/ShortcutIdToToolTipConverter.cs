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

using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using FramePFX.Shortcuts;

namespace FramePFX.BaseFrontEnd.Shortcuts.Converters;

public class ShortcutIdToToolTipConverter : IValueConverter {
    public static ShortcutIdToToolTipConverter Instance { get; } = new ShortcutIdToToolTipConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is string path) {
            return ShortcutIdToTooltip(path, null, out string gesture) ? gesture : AvaloniaProperty.UnsetValue;
        }

        throw new Exception("Value is not a shortcut string");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    public static bool ShortcutIdToTooltip(string path, string fallback, out string tooltip) {
        GroupedShortcut shortcut = ShortcutManager.Instance?.FindShortcutByPath(path);
        if (shortcut == null) {
            return (tooltip = fallback) != null;
        }

        return (tooltip = shortcut.Description) != null;
    }
}