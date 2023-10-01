using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.WPF.Shortcuts.Converters
{
    public class ShortcutIdToToolTipConverter : IValueConverter
    {
        public static ShortcutIdToToolTipConverter Instance { get; } = new ShortcutIdToToolTipConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                return ShortcutIdToTooltip(path, null, out string gesture) ? gesture : DependencyProperty.UnsetValue;
            }

            throw new Exception("Value is not a shortcut string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static bool ShortcutIdToTooltip(string path, string fallback, out string tooltip)
        {
            GroupedShortcut shortcut = ShortcutManager.Instance?.FindShortcutByPath(path);
            if (shortcut == null)
            {
                return (tooltip = fallback) != null;
            }

            return (tooltip = shortcut.Description) != null;
        }
    }
}