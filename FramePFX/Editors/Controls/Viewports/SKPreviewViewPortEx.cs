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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Controls.Viewports
{
    public class SKPreviewViewPortEx : FrameworkElement
    {
        public static readonly DependencyProperty UseNearestNeighbourRenderingProperty =
            DependencyProperty.Register(
                "UseNearestNeighbourRendering",
                typeof(bool),
                typeof(SKPreviewViewPortEx),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => RenderOptions.SetBitmapScalingMode(d, (bool) e.NewValue ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.Unspecified)));

        public bool UseNearestNeighbourRendering {
            get => (bool) this.GetValue(UseNearestNeighbourRenderingProperty);
            set => this.SetValue(UseNearestNeighbourRenderingProperty, value.Box());
        }

        /// <summary>Gets the current canvas size.</summary>
        /// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
        public SKSize CanvasSize { get; private set; }

        private SKImageInfo skImageInfo;
        private readonly bool designMode;
        private WriteableBitmap bitmap;
        private bool isRendering;

        public SKPreviewViewPortEx()
        {
            this.designMode = DesignerProperties.GetIsInDesignMode(this);
            this.UseNearestNeighbourRendering = !this.designMode; // true by default
        }

        public bool BeginRenderWithSurface(SKImageInfo frameInfo)
        {
            PresentationSource source;
            if (this.isRendering || this.designMode || (source = PresentationSource.FromVisual(this)) == null)
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
            WriteableBitmap bmp = this.bitmap;
            if (bmp == null || frameInfo.Width != bmp.PixelWidth || frameInfo.Height != bmp.PixelHeight)
            {
                this.bitmap = new WriteableBitmap(
                    frameInfo.Width, scaledSize.Height,
                    unscaledSize.Width == scaledSize.Width ? 96d : (96d * scaleX),
                    unscaledSize.Height == scaledSize.Height ? 96d : (96d * scaleY),
                    PixelFormats.Pbgra32, null);
            }

            this.isRendering = true;
            this.OnBeginRender();
            return true;
        }

        public void EndRenderWithSurface(SKSurface srcSurface)
        {
            SKImageInfo imgInfo = this.skImageInfo;
            srcSurface.Flush(true, true);
            this.bitmap.Lock();

            int dstPxW = this.bitmap.PixelWidth;
            int dstPxH = this.bitmap.PixelHeight;
            IntPtr srcPtr = srcSurface.PeekPixels().GetPixels();
            IntPtr dstPtr = this.bitmap.BackBuffer;
            if (imgInfo.Width == dstPxW && imgInfo.Height == dstPxH)
            {
                BitBltSkia(srcPtr, dstPtr, imgInfo);
            }
            else
            {
                using (SKPixmap srcPixmal = new SKPixmap(imgInfo, srcPtr))
                {
                    SKImageInfo dstInfo = new SKImageInfo(dstPxW, dstPxH, imgInfo.ColorType, imgInfo.AlphaType, imgInfo.ColorSpace);
                    using (SKPixmap dstPixmap = new SKPixmap(dstInfo, dstPtr))
                    {
                        srcPixmal.ScalePixels(dstPixmap, SKFilterQuality.High);
                    }
                }

                // using (SKSurface tmpDstSurface = SKSurface.Create(new SKImageInfo(dstPxW, dstPxH, imgInfo.ColorType, imgInfo.AlphaType, imgInfo.ColorSpace))) {
                //     srcSurface.Draw(tmpDstSurface.Canvas, 0, 0, null);
                //
                //     IntPtr srcPtr = tmpDstSurface.PeekPixels().GetPixels();
                //     IntPtr dstPtr = this.bitmap.BackBuffer;
                //     BitBltSkia(srcPtr, dstPtr, imgInfo);
                // }
            }

            this.OnPreEndRender();
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, dstPxW, dstPxH));
            this.bitmap.Unlock();
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

        protected override void OnRender(DrawingContext dc)
        {
            WriteableBitmap bmp = this.bitmap;
            if (bmp != null)
            {
                dc.DrawImage(bmp, new Rect(0d, 0d, this.ActualWidth, this.ActualHeight));
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.InvalidateVisual();
        }

        private SKSizeI CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, PresentationSource source)
        {
            unscaledSize = SKSizeI.Empty;
            scaleX = 1f;
            scaleY = 1f;
            Size size = this.RenderSize;
            if (IsPositive(size.Width) && IsPositive(size.Height))
            {
                unscaledSize = new SKSizeI((int) size.Width, (int) size.Height);
                Matrix transformToDevice = source.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
                scaleX = transformToDevice.M11;
                scaleY = transformToDevice.M22;
                return new SKSizeI((int) (size.Width * scaleX), (int) (size.Height * scaleY));
            }

            return SKSizeI.Empty;
        }

        private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}