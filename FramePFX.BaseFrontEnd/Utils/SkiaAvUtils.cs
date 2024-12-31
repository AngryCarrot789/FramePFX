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

using Avalonia;
using Avalonia.Media;
using SkiaSharp;

namespace FramePFX.BaseFrontEnd.Utils;

/// <summary>
/// A helper class for converting between avalonia and skia objects
/// </summary>
public static class SkiaAvUtils { // SkAv seems like a weird name so SkiaAv it is
    public static SKColor AvToSkia(this Color c) => new(c.R, c.G, c.B, c.A);

    public static Color SkiaToAv(this SKColor c) => new(c.Alpha, c.Red, c.Green, c.Blue);

    internal static Matrix ToAvMatrix(this SKMatrix m)
    {
        return new Matrix(m.ScaleX, m.SkewY, m.Persp0, m.SkewX, m.ScaleY, m.Persp1, m.TransX, m.TransY, m.Persp2);
    }

    // Copy of a built in avalonia method for calculating the effective size of geometry
    internal static (Size size, Matrix transform) CalculateSizeAndTransform(Size availableSize, Rect shapeBounds, Stretch stretch) {
        Size size = new Size(shapeBounds.Right, shapeBounds.Bottom);
        Matrix translate = Matrix.Identity;
        double width = availableSize.Width;
        double height = availableSize.Height;
        double w = 0.0;
        double h = 0.0;
        if (stretch != Stretch.None) {
            size = shapeBounds.Size;
            translate = Matrix.CreateTranslation(-(Vector) shapeBounds.Position);
        }

        if (double.IsInfinity(availableSize.Width))
            width = size.Width;
        if (double.IsInfinity(availableSize.Height))
            height = size.Height;
        if (shapeBounds.Width > 0.0)
            w = width / size.Width;
        if (shapeBounds.Height > 0.0)
            h = height / size.Height;
        
        if (double.IsInfinity(availableSize.Width))
            w = h;
        if (double.IsInfinity(availableSize.Height))
            h = w;

        switch (stretch) {
            case Stretch.Fill:
                if (double.IsInfinity(availableSize.Width))
                    w = 1.0;
                if (double.IsInfinity(availableSize.Height))
                    h = 1.0;
            break;
            case Stretch.Uniform:       w = h = Math.Min(w, h); break;
            case Stretch.UniformToFill: w = h = Math.Max(w, h); break;
            default:                    w = h = 1.0; break;
        }

        Matrix transform = translate * Matrix.CreateScale(w, h);
        return (new Size(size.Width * w, size.Height * h), transform);
    }
}