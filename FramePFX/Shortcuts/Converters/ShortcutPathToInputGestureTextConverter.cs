using System;
using System.Globalization;
using System.Windows.Data;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Converters {
    public class ShortcutPathToInputGestureTextConverter : IValueConverter {
        public string NoSuchShortcutFormat { get; set; } = "<{0}>";

        public string ShortcutFormat { get; set; } = null;

        public static string ShortcutToInputGestureText(string path, string shortcutFormat = null, string noSuchShortcutFormat = null) {
            GroupedShortcut shortcut = ShortcutManager.Instance.FindShortcutByPath(path);
            if (shortcut == null) {
                return noSuchShortcutFormat == null ? path : string.Format(noSuchShortcutFormat, path);
            }

            string representation = shortcut.Shortcut.ToString();
            return shortcutFormat == null ? representation : string.Format(shortcutFormat, representation);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is string path && !string.IsNullOrWhiteSpace(path)) {
                return ShortcutToInputGestureText(path, this.ShortcutFormat, this.NoSuchShortcutFormat);
            }
            else {
                throw new Exception("Invalid shortcut path (converter parameter): " + parameter);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}