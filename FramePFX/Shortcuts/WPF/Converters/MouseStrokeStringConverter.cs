using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace FramePFX.Shortcuts.WPF.Converters {
    public class MouseStrokeStringConverter : IMultiValueConverter {
        public static MouseStrokeStringConverter Instance { get; } = new MouseStrokeStringConverter();

        public static string ToStringFunction(int mouseButton, int modifiers, int clickCount) {
            StringBuilder sb = new StringBuilder();
            string mods = KeyStrokeStringConverter.ModsToString((ModifierKeys) modifiers);
            if (mods.Length > 0) {
                sb.Append(mods).Append('+');
            }

            string name;
            switch (mouseButton) {
                case 0:
                    name = "Left Click";
                    break;
                case 1:
                    name = "Middle Click";
                    break;
                case 2:
                    name = "Right Click";
                    break;
                case 3:
                    name = "X1 (←)";
                    break;
                case 4:
                    name = "X2 (→)";
                    break;
                case WPFShortcutManager.BUTTON_WHEEL_UP:
                    name = "Wheel Up";
                    break;
                case WPFShortcutManager.BUTTON_WHEEL_DOWN:
                    name = "Wheel Down";
                    break;
                default: throw new Exception("Invalid mouse button: " + mouseButton);
            }

            switch (clickCount) {
                case 2:
                    sb.Append("Double ").Append(name);
                    break;
                case 3:
                    sb.Append("Triple ").Append(name);
                    break;
                case 4:
                    sb.Append("Quad ").Append(name);
                    break;
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

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 3 || values.Length != 4) {
                Debug.WriteLine($"This converter requires 4 elements; mouseButton, modifiers, clickCount, wheelDelta. Got: {values}");
                return DependencyProperty.UnsetValue;
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
}