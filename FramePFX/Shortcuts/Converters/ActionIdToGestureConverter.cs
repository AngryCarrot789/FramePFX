using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Core.Actions;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Converters
{
    public class ActionIdToGestureConverter : IValueConverter
    {
        public static ActionIdToGestureConverter Instance { get; } = new ActionIdToGestureConverter();

        public string NoSuchActionText { get; set; } = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string id)
            {
                return ActionIdToGesture(id, this.NoSuchActionText, out string gesture) ? gesture : DependencyProperty.UnsetValue;
            }

            throw new Exception("Value is not a string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static bool ActionIdToGesture(string id, string fallback, out string gesture)
        {
            if (ActionManager.Instance.GetAction(id) == null)
            {
                return (gesture = fallback) != null;
            }

            IEnumerable<GroupedShortcut> shortcuts = ShortcutManager.Instance.GetShortcutsByAction(id);
            if (shortcuts == null)
            {
                return (gesture = fallback) != null;
            }

            return (gesture = ShortcutIdToGestureConverter.ShortcutsToGesture(shortcuts, fallback)) != null;
        }
    }
}