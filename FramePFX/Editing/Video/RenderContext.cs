// 
// Copyright (c) 2026-2026 REghZy
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
using SkiaSharp;

namespace FramePFX.Editing.Video;

public readonly struct RenderContext {
    /// <summary>
    /// The image info associated with our <see cref="Surface"/>
    /// </summary>
    public SKImageInfo ImageInfo { get; }

    /// <summary>
    /// The bitmap that stores the pixels
    /// </summary>
    public SKBitmap Bitmap { get; }

    /// <summary>
    /// A pixmap, used by our <see cref="Surface"/>, which references the <see cref="Bitmap"/>
    /// </summary>
    public SKPixmap Pixmap { get; }

    /// <summary>
    /// The surface used to draw things
    /// </summary>
    public SKSurface Surface { get; }

    /// <summary>
    /// Our <see cref="Surface"/>'s canvas
    /// </summary>
    public SKCanvas Canvas { get; }

    /// <summary>
    /// A vector2 containing our <see cref="ImageInfo"/>'s width and height
    /// </summary>
    public Vector2 FrameSize => new Vector2(this.ImageInfo.Width, this.ImageInfo.Height);

    /// <summary>
    /// Returns a rect that contains the effective rendering area, based on the canvas'
    /// <see cref="SKCanvas.TotalMatrix"/> and the size of our <see cref="ImageInfo"/>.
    /// <para>
    /// Pixels could exceed this area of course, however they really shouldn't since they
    /// would basically be drawing offscreen
    /// </para>
    /// </summary>
    public SKRect RenderArea => this.TranslateRect(new SKRect(0, 0, this.ImageInfo.Width, this.ImageInfo.Height));

    public RenderQuality Quality { get; }

    public RenderContext(SKImageInfo imageInfo, SKSurface surface, SKBitmap bitmap, SKPixmap pixmap, RenderQuality quality) {
        this.ImageInfo = imageInfo;
        this.Surface = surface;
        this.Canvas = surface.Canvas;
        this.Bitmap = bitmap;
        this.Pixmap = pixmap;
        this.Quality = quality;
    }

    /// <summary>
    /// Translates the given rectangle based on our canvas' <see cref="SKCanvas.TotalMatrix"/>
    /// </summary>
    /// <param name="inputRect">The input rect to be translated</param>
    /// <returns>The final rect which represents the effective drawing area</returns>
    public SKRect TranslateRect(SKRect inputRect) {
        return this.Canvas.TotalMatrix.MapRect(inputRect);
    }
}