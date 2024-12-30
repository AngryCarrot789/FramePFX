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

using System.Numerics;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using FramePFX.Icons;
using SkiaSharp;

namespace FramePFX.BaseFrontEnd.Icons;

public class BitmapIconImpl : AbstractAvaloniaIcon {
    public Bitmap Bitmap { get; }
    
    public BitmapIconImpl(string name, Bitmap bitmap) : base(name) {
        this.Bitmap = bitmap;
    }

    public override void Render(DrawingContext context, Rect size, SKMatrix transform) {
        using (context.PushTransform(Unsafe.As<SKMatrix, Matrix>(ref transform)))
            context.DrawImage(this.Bitmap, size);
    }

    public override (Size Size, SKMatrix Transform) Measure(Size availableSize, StretchMode stretchMode) {
        return (this.Bitmap.Size, SKMatrix.Identity);
    }
}