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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using FramePFX.Shortcuts;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Shortcuts.Converters;

public class ShortcutIdToGestureConverter : IValueConverter
{
    public static ShortcutIdToGestureConverter Instance { get; } = new ShortcutIdToGestureConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            return ShortcutIdToGesture(path, null, out string gesture) ? gesture : AvaloniaProperty.UnsetValue;
        }
        else if (value is IEnumerable<string> paths)
        {
            return ShortcutIdToGesture(paths, null, out string gesture) ? gesture : AvaloniaProperty.UnsetValue;
        }

        throw new Exception("Value is not a shortcut string");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static bool ShortcutIdToGesture(string path, string fallback, out string gesture)
    {
        GroupedShortcut shortcut = ShortcutManager.Instance?.FindShortcutByPath(path);
        if (shortcut == null)
        {
            return (gesture = fallback) != null;
        }

        gesture = shortcut.Shortcut.ToString();
        return true;
    }

    public static bool ShortcutIdToGesture(IEnumerable<string> paths, string fallback, out string gesture)
    {
        List<GroupedShortcut> shortcut = ShortcutManager.Instance?.FindShortcutsByPaths(paths).ToList();
        if (shortcut == null || shortcut.Count < 1)
        {
            return (gesture = fallback) != null;
        }

        return (gesture = ShortcutsToGesture(shortcut, null)) != null;
    }

    public static string ShortcutsToGesture(IEnumerable<GroupedShortcut> shortcuts, string fallback, bool removeDupliateInputStrokes = true)
    {
        if (removeDupliateInputStrokes)
        {
            HashSet<string> strokes = new HashSet<string>();
            List<string> newList = new List<string>();
            foreach (GroupedShortcut sc in shortcuts)
            {
                string text = ToString(sc);
                if (!strokes.Contains(text))
                {
                    strokes.Add(text);
                    newList.Add(text);
                }
            }

            return newList.JoinString(", ", " or ", fallback);
        }
        else
        {
            return shortcuts.Select(ToString).JoinString(", ", " or ", fallback);
        }
    }

    public static string ToString(GroupedShortcut shortcut)
    {
        return string.Join("+", shortcut.Shortcut.InputStrokes.Select(ToString));
    }

    public static string ToString(IInputStroke stroke)
    {
        if (stroke is MouseStroke ms)
        {
            return MouseStrokeStringConverter.ToStringFunction(ms.MouseButton, ms.Modifiers, ms.ClickCount);
        }
        else if (stroke is KeyStroke ks)
        {
            return KeyStrokeStringConverter.ToStringFunction(ks.KeyCode, ks.Modifiers, ks.IsRelease, false, true);
        }
        else
        {
            return stroke.ToString();
        }
    }
}