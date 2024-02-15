using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Commands;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.WPF.Converters {
    public class CommandIdToGestureConverter : IValueConverter {
        public static CommandIdToGestureConverter Instance { get; } = new CommandIdToGestureConverter();

        public string NoSuchActionText { get; set; } = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string id) {
                return CommandIdToGesture(id, this.NoSuchActionText, out string gesture) ? gesture : DependencyProperty.UnsetValue;
            }

            throw new Exception("Value is not a string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public static bool CommandIdToGesture(string id, string fallback, out string gesture) {
            if (CommandManager.Instance.GetCommandById(id) == null) {
                return (gesture = fallback) != null;
            }

            IEnumerable<GroupedShortcut> shortcuts = ShortcutManager.Instance.GetShortcutsByCommandId(id);
            if (shortcuts == null) {
                return (gesture = fallback) != null;
            }

            return (gesture = ShortcutIdToGestureConverter.ShortcutsToGesture(shortcuts, fallback)) != null;
        }
    }
}