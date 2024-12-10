﻿// 
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
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.SceneGraph;
using Avalonia.VisualTree;
using FramePFX.Utils;
using SkiaSharp;
using Vector = Avalonia.Vector;

namespace FramePFX.Avalonia.AvControls;

public delegate void AsyncViewPortEndRenderEventHandler(SKAsyncViewPort sender, SKSurface surface);

/// <summary>
/// An event handler for a pre or post Avalonia render event
/// </summary>
/// <param name="sender">Sender skia async view port</param>
/// <param name="ctx">The drawing context</param>
/// <param name="size">The size of the drawing area (aka size of the view port)</param>
/// <param name="minatureOffset">
/// A special Point containing the decimal parts of the pixel-imperfect position of the drawing area.
/// Since the view port's X and Y positions aren't perfect integers, this value contains the
/// decimal parts, therefore, this point contains values no bigger than 1.0
/// </param>
public delegate void AsyncViewPortRenderEventHandler(SKAsyncViewPort sender, DrawingContext ctx, Size size, Point minatureOffset);

public class SKAsyncViewPort : Control
{
    private WriteableBitmap? bitmap;
    private SKSurface? targetSurface;
    private SKImageInfo skImageInfo;
    private bool ignorePixelScaling;
    private ILockedFramebuffer? lockKey;

    /// <summary>Gets the current canvas size.</summary>
    /// <value />
    /// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
    public SKSize CanvasSize { get; private set; }

    /// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
    /// <value />
    /// <remarks>By default, when false, the canvas is resized to 1 canvas pixel per display pixel. When true, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.</remarks>
    public bool IgnorePixelScaling
    {
        get => this.ignorePixelScaling;
        set
        {
            this.ignorePixelScaling = value;
            this.InvalidateVisual();
        }
    }

    public event AsyncViewPortEndRenderEventHandler? EndRenderExtension;
    public event AsyncViewPortRenderEventHandler? PreRenderExtension;
    public event AsyncViewPortRenderEventHandler? PostRenderExtension;

    public SKImageInfo FrameInfo => this.skImageInfo;

    public SKAsyncViewPort() {
    }

    public bool BeginRender(out SKSurface surface)
    {
        IRenderRoot? source;
        if (this.targetSurface != null || (source = this.GetVisualRoot()) == null)
        {
            surface = null;
            return false;
        }

        SKSizeI pixelSize = this.CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, source);
        this.CanvasSize = this.ignorePixelScaling ? unscaledSize : pixelSize;
        if (pixelSize.Width <= 0 || pixelSize.Height <= 0)
        {
            surface = null;
            return false;
        }

        SKImageInfo frameInfo = new SKImageInfo(pixelSize.Width, pixelSize.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        this.skImageInfo = frameInfo;

        WriteableBitmap? bmp = this.bitmap;
        if (bmp == null || frameInfo.Width != bmp.PixelSize.Width || frameInfo.Height != bmp.PixelSize.Height)
        {
            this.bitmap = bmp = new WriteableBitmap(new PixelSize(frameInfo.Width, frameInfo.Height), new Vector(scaleX * 96d, scaleY * 96d));
        }

        this.lockKey = bmp.Lock();
        this.targetSurface = surface = SKSurface.Create(frameInfo, this.lockKey.Address, this.lockKey.RowBytes);
        if (this.ignorePixelScaling)
        {
            SKCanvas canvas = surface.Canvas;
            canvas.Scale((float) scaleX, (float) scaleY);
            canvas.Save();
        }

        return true;
    }

    public void EndRender(bool invalidateVisual = true)
    {
        // SKImageInfo info = this.skImageInfo;
        // this.lockKey.AddDirtyRect(new Int32Rect(0, 0, info.Width, info.Height));
        this.EndRenderExtension?.Invoke(this, this.targetSurface!);
        this.lockKey!.Dispose();
        this.lockKey = null;
        if (invalidateVisual)
            this.InvalidateVisual();
        this.targetSurface!.Dispose();
        this.targetSurface = null;
    }

    public override void Render(DrawingContext context)
    {
        // Compositor.TryGetDefaultCompositor().
        base.Render(context);
        WriteableBitmap? bmp = this.bitmap;
        if (bmp != null)
        {
            Rect myBounds = this.Bounds, finalBounds = new Rect(0d, 0d, myBounds.Width, myBounds.Height);
            FreeMoveViewPortV2 container = ((FreeMoveViewPortV2?) this.Parent?.Parent)!;

            double scale = container.ZoomScale;
            double inverseScale = 1.0 / scale;

            Rect parentClip = container.Bounds;
            Point p1 = (this.TranslatePoint(new Point(), container) ?? default);
            Point p2 = this.TranslatePoint(new Point(myBounds.Width, myBounds.Height), container) ?? default;

            Rect mfb;
            double newX = Math.Max(-p1.X, 0);
            double newY = Math.Max(-p1.Y, 0);
            double newR = Math.Min(myBounds.Width * scale, -p1.X + p2.X);
            double newB = Math.Min(myBounds.Height * scale, -p1.Y + p2.Y);
            if (newR > newX && newB > newY)
            {
                double w1 = parentClip.Width - p1.X - newX;
                double h1 = parentClip.Height - p1.Y - newY;
                mfb = new Rect(newX, newY, Math.Min(w1, newR - newX), Math.Min(h1, newB - newY));
            }
            else
            {
                return;
            }

            // While this might be somewhat useful in some cases, it didn't seem to work well
            // for the selection outline when I tried to use it. It's almost like avalonia is doing some
            // internal rounding and there's also rounding error too, making it not that useful... or maybe
            // I didn't try hard enough :---)
            Point offset = new Point(myBounds.X - Maths.Floor(myBounds.X), myBounds.Y - Maths.Floor(myBounds.Y));
            
            this.PreRenderExtension?.Invoke(this, context, finalBounds.Size, offset);
            
            // Here's 3 primary ways of drawing the bitmap.
            // First is using mfb, which is the effective visible rectangle (My Final Bounds)
            // Since this control will be scaled to the zoom, we inverse it to get back to full size,
            // Then we do additional processing to specify the absolute visible source and dest rectangles
            using (context.PushTransform(Matrix.CreateScale(inverseScale, inverseScale)))
                ((IImage) bmp).Draw(context, mfb * inverseScale, mfb);
            
            // 2nd is draw the entire bitmap using our full rectangle size, and rely on scaling to scale it down.
            // I have a very strong feeling this is basically the exact same as above, except we just do
            // more processing in the above method, whereas this one does it for us... 
            // context.DrawImage(bmp, finalBounds);
            
            // 3rd way is hella expensive since we allocate an SKBitmap and Bitmap that wrap
            // bmp. But, during playback, the gizmos are exactly in the right spot, but dragging
            // things around still yields the issue with them rendering the previous position
            // context.Custom(new DrawBitmapOperation(bmp, finalBounds, mfb, inverseScale));
            
            this.PostRenderExtension?.Invoke(this, context, finalBounds.Size, offset);
        }
    }

    /// <summary>
    /// A method that disposes our bitmap to save on some memory
    /// </summary>
    public void DisposeBitmaps()
    {
        if (this.targetSurface != null)
            throw new InvalidOperationException("Currently rendering; cannot dispose");

        this.CanvasSize = default;
        this.skImageInfo = default;
        this.bitmap?.Dispose();
        this.bitmap = default;
    }

    // internal static Rect TransformToAncestor(Rect bounds, Visual from, Visual to)
    // {
    //     Point p1 = from.TranslatePoint(bounds.TopLeft, to) ?? new Point();
    //     Point p2 = from.TranslatePoint(bounds.BottomRight, to) ?? new Point();
    //     return new Rect(p1, p2);
    // }
    // private bool IsUserVisible(Control element, Control container)
    // {
    //     if (!element.IsVisible)
    //         return false;
    //     Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
    //     Rect rect = new Rect(0.0, 0.0, container.Bounds.Width, container.Bounds.Height);
    //     return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
    // }

    public bool BeginRenderWithSurface(SKImageInfo frameInfo)
    {
        IRenderRoot? source;
        if (this.targetSurface != null || (source = this.GetVisualRoot()) == null)
        {
            return false;
        }

        SKSizeI pixelSize = this.CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, source);
        this.CanvasSize = this.ignorePixelScaling ? unscaledSize : pixelSize;
        if (pixelSize.Width <= 0 || pixelSize.Height <= 0)
        {
            return false;
        }

        this.skImageInfo = frameInfo;
        WriteableBitmap? bmp = this.bitmap;
        if (bmp == null || frameInfo.Width != bmp.PixelSize.Width || frameInfo.Height != bmp.PixelSize.Height)
        {
            this.bitmap = new WriteableBitmap(
                new PixelSize(frameInfo.Width, frameInfo.Height),
                new Vector(unscaledSize.Width == pixelSize.Width ? 96d : (96d * scaleX), unscaledSize.Height == pixelSize.Height ? 96d : (96d * scaleY)),
                PixelFormat.Bgra8888);
        }

        return true;
    }

    public void EndRenderWithSurface(SKSurface surface)
    {
        this.EndRenderExtension?.Invoke(this, this.targetSurface!);
        surface.Flush(true, true);

        using (ILockedFramebuffer buffer = this.bitmap!.Lock())
        {
            SKImageInfo imgInfo = this.FrameInfo;
            if (imgInfo.Width == this.bitmap.PixelSize.Width && imgInfo.Height == this.bitmap.PixelSize.Height)
            {
                IntPtr srcPtr = surface.PeekPixels().GetPixels();
                IntPtr dstPtr = buffer.Address;
                if (srcPtr != IntPtr.Zero && dstPtr != IntPtr.Zero)
                {
                    unsafe
                    {
                        Unsafe.CopyBlock(dstPtr.ToPointer(), srcPtr.ToPointer(), (uint) imgInfo.BytesSize64);
                        // NativeMemory.Copy(srcPtr.ToPointer(), dstPtr.ToPointer(), (uint) imgInfo.BytesSize64);
                    }
                }
            }
        }

        this.InvalidateVisual();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        this.InvalidateVisual();
    }

    public new void InvalidateVisual() => base.InvalidateVisual();

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
    
    private class DrawBitmapOperation : ICustomDrawOperation
    {
        private readonly SKBitmap bitmap;
        private readonly Bitmap bmp;
        private readonly Rect bounds;
        private readonly Rect transformedVisibleBounds;
        private readonly double inverseScale;

        public Rect Bounds => this.bounds;
        
        public DrawBitmapOperation(WriteableBitmap bmp, Rect bounds, Rect transformedVisibleBounds, double inverseScale)
        {
            this.bitmap = new SKBitmap((int) bounds.Width, (int) bounds.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

            using (ILockedFramebuffer buffer = bmp.Lock())
            {
                IntPtr srcPtr = buffer.Address;
                IntPtr dstPtr = this.bitmap.GetPixels();
                if (srcPtr != IntPtr.Zero && dstPtr != IntPtr.Zero)
                {
                    unsafe
                    {
                        Unsafe.CopyBlock(dstPtr.ToPointer(), srcPtr.ToPointer(), (uint) (this.bitmap.RowBytes * this.bitmap.Height));
                    }
                }
            }

            this.bmp = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, this.bitmap.GetPixels(), new PixelSize((int) bounds.Width, (int) bounds.Height), new Vector(96, 96), this.bitmap.RowBytes);
            this.bounds = bounds;
            this.transformedVisibleBounds = transformedVisibleBounds;
            this.inverseScale = inverseScale;
        }

        public void Dispose()
        {
            this.bmp.Dispose();
            this.bitmap.Dispose();
        }

        public bool Equals(ICustomDrawOperation? other) => other == this;

        public bool HitTest(Point p) => this.bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            using (context.PushPreTransform(Matrix.CreateScale(inverseScale, inverseScale)))
                context.DrawBitmap(this.bmp, this.transformedVisibleBounds * inverseScale, this.transformedVisibleBounds);
        }
    }
}