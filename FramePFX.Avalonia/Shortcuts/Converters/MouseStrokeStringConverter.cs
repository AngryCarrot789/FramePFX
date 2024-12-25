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
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Input;
using FramePFX.Avalonia.Shortcuts.Avalonia;

namespace FramePFX.Avalonia.Shortcuts.Converters;

public class MouseStrokeStringConverter : IMultiValueConverter {
    public static MouseStrokeStringConverter Instance { get; } = new MouseStrokeStringConverter();

    public static string ToStringFunction(int mouseButton, int modifiers, int clickCount) {
        StringBuilder sb = new StringBuilder();
        string mods = KeyStrokeStringConverter.ModsToString((KeyModifiers) modifiers);
        if (mods.Length > 0) {
            sb.Append(mods).Append('+');
        }

        string name;
        switch (mouseButton) {
            case 0:                                         name = "Left Click"; break;
            case 1:                                         name = "Middle Click"; break;
            case 2:                                         name = "Right Click"; break;
            case 3:                                         name = "X1 (←)"; break;
            case 4:                                         name = "X2 (→)"; break;
            case AvaloniaShortcutManager.BUTTON_WHEEL_UP:   name = "Wheel Up"; break;
            case AvaloniaShortcutManager.BUTTON_WHEEL_DOWN: name = "Wheel Down"; break;
            default:                                        throw new Exception("Invalid mouse button: " + mouseButton);
        }

        switch (clickCount) {
            case 2: sb.Append("Double ").Append(name); break;
            case 3: sb.Append("Triple ").Append(name); break;
            case 4: sb.Append("Quad ").Append(name); break;
            default: {
                if (clickCount > 0) {
                    sb.Append(name).Append(" (x").Append(clickCount).Append(")");
                }
                else {
                    sb.Append(name);
                }

                break;
            }
        }

        return sb.ToString();
    }

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
        if (values == null || values.Count != 3 || values.Count != 4) {
            Debug.WriteLine($"This converter requires 4 elements; mouseButton, modifiers, clickCount, wheelDelta. Got: {values}");
            return AvaloniaProperty.UnsetValue;
        }

        if (!(values[0] is int mouseButton))
            throw new Exception("values[0] must be an int: mouseButton");
        if (!(values[1] is int modifiers))
            throw new Exception("values[1] must be an int: modifiers");
        if (!(values[2] is int clickCount))
            throw new Exception("values[2] must be an int: clickCount");

        return ToStringFunction(mouseButton, modifiers, clickCount);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}