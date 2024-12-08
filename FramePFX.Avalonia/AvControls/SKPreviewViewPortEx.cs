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

using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using SkiaSharp;

namespace FramePFX.Avalonia.AvControls;

public class SKPreviewViewPortEx : Control
{
    /// <summary>Gets the current canvas size.</summary>
    /// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
    public SKSize CanvasSize { get; private set; }

    private WriteableBitmap? bitmap;
    private SKImageInfo skImageInfo;
    private bool isRendering;

    public SKPreviewViewPortEx() {
    }

    public bool BeginRenderWithSurface(SKImageInfo frameInfo)
    {
        IRenderRoot? source;
        if (this.isRendering || (source = this.GetVisualRoot()) == null)
        {
            return false;
        }

        SKSizeI scaledSize = this.CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, source);
        this.CanvasSize = scaledSize;
        if (scaledSize.Width <= 0 || scaledSize.Height <= 0)
        {
            return false;
        }

        this.skImageInfo = frameInfo;
        WriteableBitmap? bmp = this.bitmap;
        if (bmp == null || frameInfo.Width != bmp.PixelSize.Width || frameInfo.Height != bmp.PixelSize.Height)
        {
            this.bitmap = new WriteableBitmap(
                new PixelSize(frameInfo.Width, frameInfo.Height),
                new Vector(
                    unscaledSize.Width == scaledSize.Width ? 96d : (96d * scaleX),
                    unscaledSize.Height == scaledSize.Height ? 96d : (96d * scaleY)));
        }

        this.isRendering = true;
        this.OnBeginRender();
        return true;
    }

    public void EndRenderWithSurface(SKSurface srcSurface)
    {
        SKImageInfo imgInfo = this.skImageInfo;
        srcSurface.Flush(true, true);
        ILockedFramebuffer lockKey = this.bitmap!.Lock();

        int dstPxW = this.bitmap.PixelSize.Width;
        int dstPxH = this.bitmap.PixelSize.Height;
        IntPtr srcPtr = srcSurface.PeekPixels().GetPixels();
        IntPtr dstPtr = lockKey.Address;
        if (imgInfo.Width == dstPxW && imgInfo.Height == dstPxH)
        {
            BitBltSkia(srcPtr, dstPtr, imgInfo);
        }
        else
        {
            using SKPixmap srcPixmal = new SKPixmap(imgInfo, srcPtr);
            SKImageInfo dstInfo = new SKImageInfo(dstPxW, dstPxH, imgInfo.ColorType, imgInfo.AlphaType, imgInfo.ColorSpace);
            using SKPixmap dstPixmap = new SKPixmap(dstInfo, dstPtr);
            srcPixmal.ScalePixels(dstPixmap, SKFilterQuality.High);
        }

        this.OnPreEndRender();
        lockKey.Dispose();
        this.OnPostEndRender();
        this.isRendering = false;
        this.InvalidateVisual();
    }

    private static void BitBltSkia(IntPtr srcPtr, IntPtr dstPtr, SKImageInfo imgInfo)
    {
        if (srcPtr != IntPtr.Zero && dstPtr != IntPtr.Zero)
        {
            unsafe
            {
                Unsafe.CopyBlock(dstPtr.ToPointer(), srcPtr.ToPointer(), (uint) imgInfo.BytesSize64);
            }
        }
    }

    protected virtual void OnBeginRender() { }

    protected virtual void OnPreEndRender() { }

    protected virtual void OnPostEndRender() { }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        WriteableBitmap? bmp = this.bitmap;
        if (bmp != null)
        {
            Rect bounds = this.Bounds;
            context.DrawImage(bmp, new Rect(0d, 0d, bounds.Width, bounds.Height));
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        this.InvalidateVisual();
    }

    private SKSizeI CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, IRenderRoot source)
    {
        unscaledSize = SKSizeI.Empty;
        scaleX = 1f;
        scaleY = 1f;

        Size bounds = this.Bounds.Size;
        if (IsPositive(bounds.Width) && IsPositive(bounds.Height))
        {
            unscaledSize = new SKSizeI((int) bounds.Width, (int) bounds.Height);
            double transformToDevice = source.RenderScaling;
            scaleX = transformToDevice;
            scaleY = transformToDevice;
            return new SKSizeI((int) (bounds.Width * scaleX), (int) (bounds.Height * scaleY));
        }

        return SKSizeI.Empty;
    }

    private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
}