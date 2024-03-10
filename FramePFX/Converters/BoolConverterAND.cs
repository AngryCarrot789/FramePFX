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
using FramePFX.Utils;

namespace FramePFX.Converters
{
    public class BoolConverterAND : IMultiValueConverter
    {
        public bool EmptyArrayBool { get; set; } = false;

        public bool NonBoolBool
        {
            get => this.NonBoolValue is bool b && b;
            set => this.NonBoolValue = value.Box();
        }

        public object NonBoolValue { get; set; } = DependencyProperty.UnsetValue;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // if (values == null || values.Length != 3) {
            //     throw new Exception("Expected 3 elements, not " + (values != null ? values.Length.ToString() : "null"));
            // }
            // bool a = (bool) values[0]; // showOptionAlwaysUseResult
            // bool b = (bool) values[1]; // isAlwaysUseNextResult
            // bool c = (bool) values[2]; // showOptionAlwaysUseResultForCurrent
            // return (a && b && c).Box(); // box utils as optimisation
            if (values == null)
            {
                return this.EmptyArrayBool.Box();
            }

            foreach (object value in values)
            {
                if (value is bool boolean)
                {
                    if (!boolean)
                        return BoolBox.False;
                }
                else
                {
                    return this.NonBoolValue;
                }
            }

            return BoolBox.True;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}