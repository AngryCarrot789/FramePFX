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

using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PFXToolKitUI.Avalonia.Themes.Converters;

public class AddToThicknessConverter : IValueConverter {
    public Thickness Thickness { get; set; }

    public double Uniform { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value == AvaloniaProperty.UnsetValue)
            return value;

        if (value is Thickness t)
            return new Thickness(
                t.Left + this.Thickness.Left + this.Uniform,
                t.Top + this.Thickness.Top + this.Uniform,
                t.Right + this.Thickness.Right + this.Uniform,
                t.Bottom + this.Thickness.Bottom + this.Uniform);

        throw new Exception("Invalid value: " + value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}