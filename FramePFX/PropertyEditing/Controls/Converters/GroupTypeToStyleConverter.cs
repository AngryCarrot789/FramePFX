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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.PropertyEditing.Controls.Converters {
    public class GroupTypeToStyleConverter : IValueConverter {
        public Style PrimaryExpander { get; set; }
        public Style SecondaryExpander { get; set; }
        public Style NoExpanderStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == DependencyProperty.UnsetValue || !(value is GroupType groupType)) {
                return DependencyProperty.UnsetValue;
            }

            switch (groupType) {
                case GroupType.PrimaryExpander: return this.PrimaryExpander;
                case GroupType.SecondaryExpander: return this.SecondaryExpander;
                case GroupType.NoExpander: return this.NoExpanderStyle;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}