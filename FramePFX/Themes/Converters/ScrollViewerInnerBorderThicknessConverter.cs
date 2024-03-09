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

namespace FramePFX.Themes.Converters
{
    public class ScrollViewerInnerBorderThicknessConverter : IMultiValueConverter
    {
        public static ScrollViewerInnerBorderThicknessConverter Instance { get; } = new ScrollViewerInnerBorderThicknessConverter();

        public double Left { get; } = 0.0;
        public double Top { get; } = 0.0;
        public double RightVisible { get; } = 1.0;
        public double RightNotVisible { get; } = 0.0;
        public double BottomVisible { get; } = 1.0;
        public double BottomNotVisible { get; } = 0.0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
            {
                throw new Exception("Need 2 values for this converter: bottom and right scroll bar visibility values");
            }

            if (!(values[0] is Visibility bottomBar))
                throw new Exception("Bottom bar value is not a visibility: " + values[0]);
            if (!(values[1] is Visibility rightBar))
                throw new Exception("Right bar value is not a visibility: " + values[1]);

            return new Thickness(
                this.Left,
                this.Top,
                rightBar == Visibility.Visible ? this.RightVisible : this.RightNotVisible,
                bottomBar == Visibility.Visible ? this.BottomVisible : this.BottomNotVisible);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}