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

public delegate void BitmapHandleChangedEventHandler(PNBitmap bitmap);

/// <summary>
/// A class that encapsulates a Skia bitmap
/// </summary>
public class PNBitmap {
    private IntPtr hColourData;
    private SKBitmap? skBitmap;
    private SKCanvas? skCanvas;
    private PixSize size;

    /// <summary>
    /// Returns true when this bitmap has been initialised and can therefore be drawn into. The size will be non-zero when true
    /// </summary>
    public bool IsInitialised => this.hColourData != IntPtr.Zero;

    /// <summary>
    /// Gets the handle to the raw pixels 
    /// </summary>
    public IntPtr ColourData => this.hColourData;

    /// <summary>
    /// Gets our Skia canvas
    /// </summary>
    public SKCanvas? Canvas => this.skCanvas;

    /// <summary>
    /// Gets our skia bitmap
    /// </summary>
    public SKBitmap? Bitmap => this.skBitmap;

    /// <summary>
    /// Gets the pixel size of this bitmap
    /// </summary>
    public PixSize Size => this.size;

    public event BitmapHandleChangedEventHandler? BitmapHandleChanged;

    public PNBitmap() {
    }

    /// <summary>
    /// Sets our colour data handle to the given pointer. Width and height must be non-negative
    /// </summary>
    /// <param name="hColourData"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void InitialiseBitmapUnsafe(IntPtr hColourData, int w, int h) {
        if (w < 0 || h < 0)
            throw new InvalidOperationException("Cannot set width or height to a negative value");

        this.skCanvas?.Dispose();
        this.skBitmap?.Dispose();
        if (hColourData == IntPtr.Zero || w == 0 || h == 0) {
            if (this.hColourData == IntPtr.Zero) {
                return;
            }

            this.hColourData = IntPtr.Zero;
            this.size = default;
        }
        else {
            this.hColourData = hColourData;
            this.size = new PixSize(w, h);

            // this.skBitmap = new SKBitmap(new SKImageInfo(w, h, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul));
            // this.skBitmap.SetPixels(this.hColourData);

            this.skBitmap = new SKBitmap();
            // Sets the SKBitmap back buffer as our colour data handle
            this.skBitmap.InstallPixels(new SKImageInfo(w, h, SKImageInfo.PlatformColorType, SKAlphaType.Premul), hColourData, w * 4, (address, context) => {
            });

            this.skCanvas = new SKCanvas(this.skBitmap);
        }

        this.BitmapHandleChanged?.Invoke(this);
    }

    public void InitialiseBitmap(PixSize resolution) {
        if (resolution.Width < 0 || resolution.Height < 0)
            throw new InvalidOperationException("Cannot set width or height to a negative value");

        this.skCanvas?.Dispose();
        this.skBitmap?.Dispose();
        if (resolution.Width == 0 || resolution.Height == 0) {
            if (this.hColourData == IntPtr.Zero) {
                return;
            }

            this.hColourData = IntPtr.Zero;
            this.size = default;
        }
        else {
            this.size = resolution;
            this.skBitmap = new SKBitmap(new SKImageInfo(resolution.Width, resolution.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            this.skCanvas = new SKCanvas(this.skBitmap);
            this.hColourData = this.skBitmap.GetPixels();
        }

        this.BitmapHandleChanged?.Invoke(this);
    }

    public unsafe int PixelAt(int x, int y) {
        return ((int*) this.hColourData)[this.size.Width * y + x];
    }

    // Endianness is flipped so it's technically ARGB
    public unsafe void SetPixelAt(int x, int y, int colourData) {
        if (x < 0 || y < 0 || x >= this.size.Width || y >= this.size.Height)
            return;

        ((int*) this.hColourData)[this.size.Width * y + x] = colourData;
    }

    // public unsafe void Fill(uint colour) {
    //     new Span<uint>((void*) this.hColourData, this.size.Width * this.size.Height).Fill(colour);
    // }
    public void Paste(PNBitmap bitmap) {
        if (bitmap.skBitmap != null)
            this.Canvas?.DrawBitmap(bitmap.skBitmap, new SKPoint(0, 0));
    }

    public void InitialiseBitmap(PNBitmap bitmap) {
        if (!bitmap.IsInitialised)
            return;

        this.InitialiseBitmap(bitmap.size);
        this.Paste(bitmap);
    }

    /// <summary>
    /// Initialise this PNB using the given bitmap as our backing bitmap
    /// </summary>
    /// <param name="bitmap">
    /// The bitmap to use. Once this method returns, ownership of this bitmap becomes that of the PNB
    /// </param>
    public void InitialiseUsingBitmap(SKBitmap? bitmap) {
        this.skCanvas?.Dispose();
        this.skBitmap?.Dispose();
        if (bitmap == null) {
            if (this.hColourData == IntPtr.Zero) {
                return;
            }

            this.hColourData = IntPtr.Zero;
            this.size = default;
        }
        else {
            SKImageInfo info = bitmap.Info;
            this.size = new PixSize(info.Width, info.Height);
            this.skBitmap = bitmap;
            this.skCanvas = new SKCanvas(this.skBitmap);
            this.hColourData = this.skBitmap.GetPixels();
        }

        this.BitmapHandleChanged?.Invoke(this);
    }
}