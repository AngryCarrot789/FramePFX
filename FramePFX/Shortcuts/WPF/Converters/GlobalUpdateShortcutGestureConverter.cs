//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.Shortcuts.WPF.Converters {
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