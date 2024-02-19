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

namespace FramePFX.Editors.Controls.Timelines {
    public class AutomationBrushConverter : IMultiValueConverter {
        public Brush NoAutomationBrush { get; set; } = Brushes.Transparent;
        public Brush AutomationBrush { get; set; } = Brushes.Orange;
        public Brush OverrideBrush { get; set; } = Brushes.Gray;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue) {
                return DependencyProperty.UnsetValue;
            }

            bool isAutomated = (bool) values[0];
            bool isOverride = (bool) values[1];
            if (isAutomated) {
                return isOverride ? this.OverrideBrush : this.AutomationBrush;
            }
            else {
                return this.NoAutomationBrush;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}