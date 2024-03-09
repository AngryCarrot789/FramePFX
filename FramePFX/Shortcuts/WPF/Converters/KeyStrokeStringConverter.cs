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
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using FramePFX.Utils;

namespace FramePFX.Shortcuts.WPF.Converters
{
    public class KeyStrokeStringConverter : IMultiValueConverter
    {
        public static KeyStrokeStringConverter Instance { get; } = new KeyStrokeStringConverter();

        public static string ToStringFunction(int keyCode, int modifiers, bool release, bool appendKeyDown, bool appendKeyUp)
        {
            StringBuilder sb = new StringBuilder();
            string mods = ModsToString((ModifierKeys) modifiers);
            if (mods.Length > 0)
            {
                sb.Append(mods).Append('+');
            }

            sb.Append((Key) keyCode);
            if (release)
            {
                if (appendKeyUp)
                {
                    sb.Append(" (↑)");
                }
            }
            else if (appendKeyDown)
            {
                sb.Append(" (↓)");
            }

            return sb.ToString();
        }

        public bool AppendKeyDown { get; set; } = true;
        public bool AppendKeyUp { get; set; } = true;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 3)
            {
                throw new Exception("This converter requires 3 elements; keycode, modifiers, isRelease");
            }

            if (!(values[0] is int keyCode))
                throw new Exception("values[0] must be an int: keycode");
            if (!(values[1] is int modifiers))
                throw new Exception("values[1] must be an int: modifiers");
            if (!(values[2] is bool isRelease))
                throw new Exception("values[2] must be a bool: isRelease");

            return ToStringFunction(keyCode, modifiers, isRelease, this.AppendKeyDown, this.AppendKeyUp);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string ModsToString(ModifierKeys keys)
        {
            StringJoiner joiner = new StringJoiner("+");
            if ((keys & ModifierKeys.Control) != 0)
                joiner.Append("Ctrl");
            if ((keys & ModifierKeys.Alt) != 0)
                joiner.Append("Alt");
            if ((keys & ModifierKeys.Shift) != 0)
                joiner.Append("Shift");
            if ((keys & ModifierKeys.Windows) != 0)
                joiner.Append("Win");
            return joiner.ToString();
        }
    }
}