// 
// Copyright (c) 2024-2024 REghZy
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

namespace FramePFX.Avalonia.Themes.Converters;

public class GroupBoxGapToThicknessConverter : IValueConverter
{
    public static GroupBoxGapToThicknessConverter Instance { get; } = new GroupBoxGapToThicknessConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == AvaloniaProperty.UnsetValue)
            return value;

        if (!(value is double gap))
            throw new Exception("Expected double, got " + value);

        return new Thickness(0, 0, 0, gap);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == AvaloniaProperty.UnsetValue)
            return value;

        if (!(value is Thickness gap))
            throw new Exception("Expected Thickness, got " + value);

        return gap.Bottom;
    }
}