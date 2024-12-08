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
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using FramePFX.PropertyEditing;

namespace FramePFX.Avalonia.PropertyEditing;

public class GroupTypeToStyleConverter : IValueConverter
{
    public ControlTheme PrimaryExpander { get; set; }
    public ControlTheme SecondaryExpander { get; set; }
    public ControlTheme NoExpanderStyle { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == AvaloniaProperty.UnsetValue || !(value is GroupType groupType))
        {
            return AvaloniaProperty.UnsetValue;
        }

        switch (groupType)
        {
            case GroupType.PrimaryExpander: return this.PrimaryExpander;
            case GroupType.SecondaryExpander: return this.SecondaryExpander;
            case GroupType.NoExpander: return this.NoExpanderStyle;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}