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

namespace FramePFX.Converters {
    public class VolumeToDbConverter : IValueConverter {
        public static VolumeToDbConverter Instance { get; } = new VolumeToDbConverter();

        public int? RoundedPlaces { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double input;
            switch (value) {
                case float f:
                    input = f;
                    break;
                case double d:
                    input = d;
                    break;
                default: return DependencyProperty.UnsetValue;
            }

            double val = AudioUtils.VolumeToDb(input);
            if (this.RoundedPlaces is int round)
                val = Math.Round(val, round);
            return value is float ? (float) val : val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is float f) {
                return AudioUtils.DbToVolume(f);
            }
            else if (value is double d) {
                return AudioUtils.DbToVolume(d);
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}