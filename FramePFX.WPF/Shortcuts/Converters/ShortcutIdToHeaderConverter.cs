using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.WPF.Shortcuts.Converters
{
    public class ShortcutIdToHeaderConverter : IValueConverter
    {
        public static ShortcutIdToHeaderConverter Instance { get; } = new ShortcutIdToHeaderConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                return ShortcutIdToHeader(path, null, out string gesture) ? gesture : DependencyProperty.UnsetValue;
            }

            throw new Exception("Value is not a shortcut string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static bool ShortcutIdToHeader(string path, string fallback, out string header)
        {
            GroupedShortcut shortcut = ShortcutManager.Instance.FindShortcutByPath(path);
            if (shortcut == null)
            {
                return (header = fallback) != null;
            }

            // This could probably go in the guinness world records
            header = shortcut.DisplayName ?? shortcut.Name ?? shortcut.FullPath ?? shortcut.ActionId ?? fallback ?? shortcut.Shortcut.ToString();
            return true;
        }
    }
}