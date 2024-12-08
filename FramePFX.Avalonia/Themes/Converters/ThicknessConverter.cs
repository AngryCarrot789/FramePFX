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

public abstract class ThicknessConverter : IValueConverter
{
    public object? UnsetValue { get; set; } = AvaloniaProperty.UnsetValue;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == AvaloniaProperty.UnsetValue)
            return this.UnsetValue;

        if (value is double d)
            return this.Convert(new Thickness(d), parameter, culture, true);

        if (value is Thickness t)
            return this.Convert(t, parameter, culture, false);

        throw new Exception($"Invalid value. Expected double or thickness, Got {value?.ToString() ?? "null"}");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    protected abstract Thickness Convert(Thickness value, object? targetType, CultureInfo parameter, bool isUniformDouble);
}

public class AddThicknessConverter : ThicknessConverter
{
    public Thickness Thickness { get; set; }

    /// <summary>
    /// A value added to all 4 parts
    /// </summary>
    public double Uniform { get; set; }

    protected override Thickness Convert(Thickness v, object? targetType, CultureInfo parameter, bool isUniformDouble)
    {
        return v + this.Thickness + new Thickness(this.Uniform);
    }
}

public class MultiplyThicknessConverter : ThicknessConverter
{
    public Thickness Thickness { get; set; } = new Thickness(1);

    protected override Thickness Convert(Thickness v, object? targetType, CultureInfo parameter, bool isUniformDouble)
    {
        Thickness t = this.Thickness;
        return new Thickness(v.Left * t.Left, v.Top * t.Top, v.Right * t.Right, v.Bottom * t.Bottom);
    }
}

public class SetThicknessConverter : ThicknessConverter
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double? Right { get; set; }
    public double? Bottom { get; set; }

    protected override Thickness Convert(Thickness v, object? targetType, CultureInfo parameter, bool isUniformDouble)
    {
        return new Thickness(this.Left ?? v.Left, this.Top ?? v.Top, this.Right ?? v.Right, this.Bottom ?? v.Bottom);
    }
}

public class RaisedBaseContentMarginConverter : ThicknessConverter
{
    public double RaisedBaseGap { get; set; } = 4.0;

    protected override Thickness Convert(Thickness v, object? targetType, CultureInfo parameter, bool isUniformDouble)
    {
        Thickness v2 = v * 2;
        double g = this.RaisedBaseGap;
        return new Thickness(v2.Left + g, v2.Top + g, v2.Right + g, v2.Bottom + g);
    }
}

public class RaisedBasePressedContentMarginConverter : ThicknessConverter
{
    public double RaisedBaseGap { get; set; } = 4.0;

    protected override Thickness Convert(Thickness v, object? targetType, CultureInfo parameter, bool isUniformDouble)
    {
        return v + new Thickness(this.RaisedBaseGap) + v + new Thickness(v.Left, v.Top, -v.Right, -v.Bottom);
    }
}