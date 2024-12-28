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
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Skia;
using FramePFX.Icons;
using FramePFX.Logging;
using FramePFX.Themes;
using SkiaSharp;

namespace FramePFX.BaseFrontEnd.Icons;

/// <summary>
/// A class that manages a set of registered icons throughout the application. This is used to simply icon usage
/// </summary>
public class IconManagerImpl : IconManager {
    // Try to find an existing icon with the same file path. Share pixel data, maybe using a wrapper, because icons are lazily loaded

    public IconManagerImpl() {
    }

    public override Icon RegisterIconByFilePath(string name, string filePath, bool lazilyLoad = true) {
        Bitmap? bmp = null;
        try {
            return this.RegisterCore(new BitmapIconImpl(name, bmp = new Bitmap(filePath)));
        }
        catch (Exception e) {
            AppLogger.Instance.WriteLine($"Exception while loading bitmap from file '{filePath}':\n{e}");
            bmp?.Dispose();
            return this.RegisterCore(new EmptyIcon(name));
        }
    }

    public override Icon RegisterIconUsingBitmap(string name, SKBitmap bitmap) {
        Bitmap? bmp = null;
        try {
            SKImageInfo info = bitmap.Info;
            PixelFormat? fmt = info.ColorType.ToAvalonia();
            bmp = new Bitmap(fmt ?? PixelFormat.Bgra8888, info.AlphaType.ToAlphaFormat(), bitmap.GetPixels(), new PixelSize(info.Width, info.Height), new Vector(96, 96), info.RowBytes);
            return this.RegisterCore(new BitmapIconImpl(name, bmp));
        }
        catch (Exception e) {
            AppLogger.Instance.WriteLine($"Exception while creating avalonia bitmap from skia bitmap:\n{e}");
            bmp?.Dispose();
            return this.RegisterCore(new EmptyIcon(name));
        }
    }

    public override Icon RegisterGeometryIcon(string name, IColourBrush? brush, IColourBrush? stroke, string[] geometry, double strokeThickness = 0.0) {
        return this.RegisterCore(new GeometryIconImpl(name, brush, stroke, strokeThickness, geometry));
    }

    private static ImmutableSolidColorBrush? ColourToBrush(SKColor? colour) {
        return colour is SKColor b ? new ImmutableSolidColorBrush(Color.FromArgb(b.Alpha, b.Red, b.Green, b.Blue)) : null;
    }
}