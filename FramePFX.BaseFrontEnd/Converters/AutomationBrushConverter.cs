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

using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FramePFX.Icons;

namespace FramePFX.BaseFrontEnd.Converters;

public class AutomationBrushConverter : IMultiValueConverter {
    public Icon? NoAutomationIcon { get; set; } = null;
    public Icon? AutomationIcon { get; set; } = AutomationIcons.IconLED_Active;
    public Icon? OverrideIcon { get; set; } = AutomationIcons.IconLED_Override;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
        if (values[0] == AvaloniaProperty.UnsetValue || values[1] == AvaloniaProperty.UnsetValue) {
            return AvaloniaProperty.UnsetValue;
        }

        bool isAutomated = (bool) values[0]!;
        bool isOverride = (bool) values[1]!;
        if (isAutomated) {
            return isOverride ? this.OverrideIcon : this.AutomationIcon;
        }
        else {
            return this.NoAutomationIcon;
        }
    }
}