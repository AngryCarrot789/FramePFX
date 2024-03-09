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
using System.Windows.Media;

namespace FramePFX.Editors.Controls.Automation
{
    public class AutomationActivityBrushConverter : IMultiValueConverter
    {
        public Brush ActiveBrush { get; set; } = Brushes.OrangeRed;

        public Brush ForcedActive { get; set; } = Brushes.DodgerBlue;

        public Brush InactiveBrush { get; set; } = Brushes.DarkGray;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            {
                return DependencyProperty.UnsetValue;
            }

            bool isReady = (bool) values[0];
            bool selected = (bool) values[1];
            if (isReady)
            {
                return this.ActiveBrush; //selected ? this.ActiveBrush : this.ForcedActive;
            }
            else
            {
                return this.InactiveBrush;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}