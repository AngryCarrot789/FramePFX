using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.WPF.Shortcuts.Converters {
    public class GlobalUpdateShortcutGestureConverter : IValueConverter, INotifyPropertyChanged {
        public static GlobalUpdateShortcutGestureConverter Instance { get; } = new GlobalUpdateShortcutGestureConverter();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Version { get; private set; }

        public static void BroadcastChange() {
            Instance.Version++;
            Instance.OnPropertyChanged(nameof(Version));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is string path) {
                return ShortcutIdToGestureConverter.ShortcutIdToGesture(path, null, out string gesture) ? gesture : DependencyProperty.UnsetValue;
            }

            throw new Exception("Parameter is not a shortcut string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}