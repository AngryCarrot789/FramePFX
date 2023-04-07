using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using FocusGroupHotkeys.Core.Shortcuts.ViewModels;

namespace FocusGroupHotkeys.Converters {
    public class MouseStrokeRepresentationConverter : IMultiValueConverter {
        public static string ToStringFunction(MouseStrokeViewModel stroke) {
            return ToStringFunction(stroke.MouseButton, stroke.Modifiers, stroke.ClickCount, stroke.WheelDelta);
        }

        public static string ToStringFunction(int mouseButton, int modifiers, int clickCount, int wheelDelta) {
            StringBuilder sb = new StringBuilder();
            string mods = KeyStrokeRepresentationConverter.ModsToString((ModifierKeys) modifiers);
            if (mods.Length > 0) {
                sb.Append(mods).Append('+');
            }

            string name;
            switch (mouseButton) {
                case 0: name = "Left Click"; break;
                case 1: name = "Middle Click"; break;
                case 2: name = "Right Click"; break;
                case 3: name = "X1 (NAV Back)"; break;
                case 4: name = "X2 (NAV Forward)"; break;
                case AppShortcutManager.BUTTON_WHEEL_UP: name = "Wheel Up"; break;
                case AppShortcutManager.BUTTON_WHEEL_DOWN: name = "Wheel Down"; break;
                default: throw new Exception("Invalid mouse button: " + mouseButton);
            }

            switch (clickCount) {
                case 2: name = "Double " + name; break;
                case 3: name = "Triple " + name; break;
                case 4: name = "Quad " + name; break;
                default: name += " (x" + clickCount + ")"; break;
            }

            return sb.Append(name).ToString();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 4) {
                throw new Exception("This converter requires 4 elements; mouseButton, modifiers, clickCount, wheelDelta");
            }

            if (!(values[0] is int mouseButton)) throw new Exception("values[0] must be an int: mouseButton");
            if (!(values[1] is int modifiers)) throw new Exception("values[1] must be an int: modifiers");
            if (!(values[2] is int clickCount)) throw new Exception("values[2] must be an int: clickCount");
            if (!(values[3] is int wheelDelta)) throw new Exception("values[3] must be an int: wheelDelta");

            return ToStringFunction(mouseButton, modifiers, clickCount, wheelDelta);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}