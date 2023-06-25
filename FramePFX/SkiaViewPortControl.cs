using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace FramePFX {
    public sealed class SkiaViewPortControl : FrameworkElement {
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

        public SKImageInfo FrameInfo => this.skImageInfo;

        public SkiaViewPortControl() => this.designMode = DesignerProperties.GetIsInDesignMode(this);

        public bool BeginRender(out SKSurface surface) {
            PresentationSource source;
            if (this.targetSurface != null || this.designMode || (source = PresentationSource.FromVisual(this)) == null) {
                surface = null;
                return false;
            }

            SKSizeI pixelSize = this.CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY, source);
            SKSizeI size2 = this.IgnorePixelScaling ? unscaledSize : pixelSize;
            this.CanvasSize = size2;
            if (pixelSize.Width <= 0 || pixelSize.Height <= 0) {
                surface = null;
                return false;
            }

            SKImageInfo frameInfo = new SKImageInfo(pixelSize.Width, pixelSize.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            this.skImageInfo = frameInfo;
            if (this.bitmap == null || frameInfo.Width != this.bitmap.PixelWidth || frameInfo.Height != this.bitmap.PixelHeight)
                this.bitmap = new WriteableBitmap(frameInfo.Width, pixelSize.Height, 96.0 * scaleX, 96.0 * scaleY, PixelFormats.Pbgra32, null);
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
            SKImageInfo info = this.skImageInfo;
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, info.Height));
            this.bitmap.Unlock();
            this.Dispatcher.Invoke(this.InvalidateVisual);
            this.targetSurface.Dispose();
            this.targetSurface = null;
        }

        protected override void OnRender(DrawingContext dc) {
            WriteableBitmap bmp = this.bitmap;
            if (bmp != null) {
                dc.DrawImage(bmp, new Rect(0d, 0d, this.ActualWidth, this.ActualHeight));
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            this.InvalidateVisual();
        }

        private SKSizeI CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY, PresentationSource source) {
            unscaledSize = SKSizeI.Empty;
            scaleX = 1f;
            scaleY = 1f;
            double actualWidth = this.ActualWidth;
            double actualHeight = this.ActualHeight;
            if (IsPositive(actualWidth) && IsPositive(actualHeight)) {
                unscaledSize = new SKSizeI((int) actualWidth, (int) actualHeight);
                Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
                scaleX = (float) transformToDevice.M11;
                scaleY = (float) transformToDevice.M22;
                return new SKSizeI((int) (actualWidth * scaleX), (int) (actualHeight * scaleY));
            }

            return SKSizeI.Empty;
        }

        private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}