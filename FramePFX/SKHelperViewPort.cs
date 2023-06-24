using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace FramePFX {
    public class SKHelperViewPort : FrameworkElement {
        private readonly bool designMode;
        private WriteableBitmap bitmap;
        private bool ignorePixelScaling;

        /// <summary>Gets the current canvas size.</summary>
        /// <value />
        /// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
        public SKSize CanvasSize { get; private set; }

        /// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
        /// <value />
        /// <remarks>By default, when false, the canvas is resized to 1 canvas pixel per display pixel. When true, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.</remarks>
        public bool IgnorePixelScaling {
            get => this.ignorePixelScaling;
            set {
                this.ignorePixelScaling = value;
                this.InvalidateVisual();
            }
        }

        private volatile SKSurface targetSurface;
        private SKImageInfo skImageInfo;
        private SKSizeI size;

        public SKImageInfo FrameInfo => this.skImageInfo;

        public SKHelperViewPort() => this.designMode = DesignerProperties.GetIsInDesignMode(this);

        public bool BeginRender(out SKSurface surface) {
            if (this.targetSurface != null || this.designMode) { // || PresentationSource.FromVisual(this) == null
                surface = null;
                return false;
            }

            SKSizeI size1 = this.CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY);
            SKSizeI size2 = this.IgnorePixelScaling ? unscaledSize : size1;
            this.CanvasSize = size2;
            if (size1.Width <= 0 || size1.Height <= 0) {
                surface = null;
                return false;
            }

            SKImageInfo frameInfo = new SKImageInfo(size1.Width, size1.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            this.skImageInfo = frameInfo;
            this.size = size1;
            if (this.bitmap == null || frameInfo.Width != this.bitmap.PixelWidth || frameInfo.Height != this.bitmap.PixelHeight)
                this.bitmap = new WriteableBitmap(frameInfo.Width, size1.Height, 96.0 * scaleX, 96.0 * scaleY, PixelFormats.Pbgra32, null);
            this.bitmap.Lock();

            this.targetSurface = surface = SKSurface.Create(frameInfo, this.bitmap.BackBuffer, this.bitmap.BackBufferStride);
            if (this.IgnorePixelScaling) {
                SKCanvas canvas = surface.Canvas;
                canvas.Scale(scaleX, scaleY);
                canvas.Save();
            }

            return true;
        }

        public void EndRender() {
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.skImageInfo.Width, this.size.Height));
            this.bitmap.Unlock();
            this.Dispatcher.Invoke(this.InvalidateVisual);
            this.targetSurface.Dispose();
            this.targetSurface = null;
        }

        /// <param name="dc">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        /// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.</summary>
        /// <remarks />
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            dc.DrawImage(this.bitmap, new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight));
        }

        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        /// <summary>Raises the SizeChanged event, using the specified information as part of the eventual event data.</summary>
        /// <remarks />
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            this.InvalidateVisual();
        }

        private SKSizeI CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY) {
            unscaledSize = SKSizeI.Empty;
            scaleX = 1f;
            scaleY = 1f;
            double actualWidth = this.ActualWidth;
            double actualHeight = this.ActualHeight;
            if (IsPositive(actualWidth) && IsPositive(actualHeight)) {
                unscaledSize = new SKSizeI((int) actualWidth, (int) actualHeight);
                Matrix transformToDevice = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
                scaleX = (float) transformToDevice.M11;
                scaleY = (float) transformToDevice.M22;
                return new SKSizeI((int) (actualWidth * scaleX), (int) (actualHeight * scaleY));
            }

            return SKSizeI.Empty;
        }

        private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}