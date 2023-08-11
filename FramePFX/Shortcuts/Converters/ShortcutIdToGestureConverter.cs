using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Core.Utils;

namespace FramePFX.Shortcuts.Converters {
    public class ShortcutIdToGestureConverter : IValueConverter {
        public static ShortcutIdToGestureConverter Instance { get; } = new ShortcutIdToGestureConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string path) {
                return ShortcutIdToGesture(path, null, out string gesture) ? gesture : DependencyProperty.UnsetValue;
            }
            else if (value is IEnumerable<string> paths) {
                return ShortcutIdToGesture(paths, null, out string gesture) ? gesture : DependencyProperty.UnsetValue;
            }

            throw new Exception("Value is not a shortcut string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public static bool ShortcutIdToGesture(string path, string fallback, out string gesture) {
            GroupedShortcut shortcut = ShortcutManager.Instance?.FindShortcutByPath(path);
            if (shortcut == null) {
                return (gesture = fallback) != null;
            }

            gesture = shortcut.Shortcut.ToString();
            return true;
        }

        public static bool ShortcutIdToGesture(IEnumerable<string> paths, string fallback, out string gesture) {
            List<GroupedShortcut> shortcut = ShortcutManager.Instance?.FindShortcutsByPaths(paths).ToList();
            if (shortcut == null || shortcut.Count < 1) {
                return (gesture = fallback) != null;
            }

            return (gesture = ShortcutsToGesture(shortcut, null)) != null;
        }

        public static string ShortcutsToGesture(IEnumerable<GroupedShortcut> shortcuts, string fallback) {
            return shortcuts.Select(ToString).JoinString(", ", " or ", fallback);
        }

        public static string ToString(GroupedShortcut shortcut) {
            return string.Join(", ", shortcut.Shortcut.InputStrokes.Select(ToString));
        }

        public static string ToString(IInputStroke stroke) {
            if (stroke is MouseStroke ms) {
                return MouseStrokeStringConverter.ToStringFunction(ms.MouseButton, ms.Modifiers, ms.ClickCount);
            }
            else if (stroke is KeyStroke ks) {
                return KeyStrokeStringConverter.ToStringFunction(ks.KeyCode, ks.Modifiers, ks.IsRelease, false, true);
            }
            else {
                return stroke.ToString();
            }
        }
    }
}