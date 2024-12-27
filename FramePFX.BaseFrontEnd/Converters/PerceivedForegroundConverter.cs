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
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkiaSharp;

namespace FramePFX.BaseFrontEnd.Converters;

public class PerceivedForegroundConverter : IValueConverter {
    public static PerceivedForegroundConverter Instance { get; } = new PerceivedForegroundConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is ISolidColorBrush brush) {
            return GetColour(brush.Color);
        }
        else if (value is SKColor skCb) {
            return GetColour(new Color(255, skCb.Red, skCb.Green, skCb.Blue));
        }
        else {
            throw new ArgumentException("Incompatible value", nameof(value));
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    public static Color GetColour(ISolidColorBrush brush) => GetColour(brush.Color);

    public static Color GetColour(Color c) => GetColour(c.R, c.G, c.B);

    public static Color GetColour(int r, int g, int b) {
        const double MulR = 0.28, MulG = 0.52, MulB = 0.125;
        int brightness = (int) Math.Sqrt(r * r * MulR + g * g * MulG + b * b * MulB);
        return brightness > 180 ? Colors.Black : Colors.White;
    }

    public static ImmutableSolidColorBrush GetBrush(ISolidColorBrush brush) => new ImmutableSolidColorBrush(GetColour(brush));
    public static ImmutableSolidColorBrush GetBrush(Color c) => new ImmutableSolidColorBrush(GetColour(c));
}