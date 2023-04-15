using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Converters {
    public class ShortcutPathToGestureConverter : IValueConverter {
        public static ShortcutPathToGestureConverter Instance { get; } = new ShortcutPathToGestureConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string path) {
                return PathToGesture(path, null, out string gesture) ? gesture : DependencyProperty.UnsetValue;
            }

            throw new Exception("Value is not a shortcut string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public static bool PathToGesture(string path, string fallback, out string gesture) {
            GroupedShortcut shortcut = WPFShortcutManager.Instance.FindShortcutByPath(path);
            if (shortcut == null) {
                return (gesture = fallback) != null;
            }

            gesture = shortcut.Shortcut.ToString();
            return true;
        }
    }
}