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

using SkiaSharp;

namespace FramePFX.Utils;

public class RenderUtils {
    private static readonly Random Rnd = new Random();

    private static readonly SKColor[] Colours = new SKColor[] {
        SKColors.Black,
        SKColors.Brown,
        SKColors.CadetBlue,
        SKColors.Chocolate,
        SKColors.Coral,
        SKColors.CornflowerBlue,
        SKColors.Crimson,
        SKColors.DarkBlue,
        SKColors.DarkCyan,
        SKColors.DarkGoldenrod,
        SKColors.DarkGray,
        SKColors.DarkGreen,
        SKColors.DarkKhaki,
        SKColors.DarkMagenta,
        SKColors.DarkOliveGreen,
        SKColors.DarkOrange,
        SKColors.DarkOrchid,
        SKColors.DarkRed,
        SKColors.DarkSalmon,
        SKColors.DarkSlateBlue,
        SKColors.DarkSlateGray,
        SKColors.DarkViolet,
        SKColors.DeepPink,
        SKColors.DeepSkyBlue,
        SKColors.DimGray,
        SKColors.DodgerBlue,
        SKColors.Firebrick,
        SKColors.ForestGreen,
        SKColors.Fuchsia,
        SKColors.Gray,
        SKColors.Green,
        SKColors.HotPink,
        SKColors.IndianRed,
        SKColors.Indigo,
        SKColors.Magenta,
        SKColors.Maroon,
        SKColors.MediumPurple,
        SKColors.MediumSlateBlue,
        SKColors.MediumVioletRed,
        SKColors.MidnightBlue,
        SKColors.Navy,
        SKColors.Olive,
        SKColors.OliveDrab,
        SKColors.Orange,
        SKColors.OrangeRed,
        SKColors.Orchid,
        SKColors.PaleVioletRed,
        SKColors.Peru,
        SKColors.Plum,
        SKColors.PowderBlue,
        SKColors.Purple,
        SKColors.RosyBrown,
        SKColors.RoyalBlue,
        SKColors.SaddleBrown,
        SKColors.SeaGreen,
        SKColors.Sienna,
        SKColors.SkyBlue,
        SKColors.SlateBlue,
        SKColors.SlateGray,
        SKColors.SteelBlue,
        SKColors.Teal,
        SKColors.Thistle,
        SKColors.Tomato,
    };

    public static SKColor RandomColour() {
        return Colours[Rnd.Next(Colours.Length)];
    }

    public static SKColor BlendAlpha(SKColor colour, double alpha) {
        return colour.WithAlpha(MultiplyByte255(colour.Alpha, alpha));
    }

    public static byte MultiplyByte255(byte a, double b) {
        return (byte) Maths.Clamp((int) Math.Round(a / 255d * b * 255d), 0, 255);
    }

    public static byte DoubleToByte255(double value) {
        return (byte) Maths.Clamp((int) Math.Round(value * 255d), 0, 255);
    }

    public static double Byte255ToDouble(byte value) {
        return Maths.Clamp(value / 255d, 0d, 1d);
    }

    public static sbyte DoubleToSByte127(double value) {
        return (sbyte) Maths.Clamp((int) Math.Round(value * 255d), sbyte.MinValue, sbyte.MaxValue);
    }
}