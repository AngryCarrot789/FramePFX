using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace FramePFX
{
    public class SKViewPort : FrameworkElement
    {
        private readonly bool designMode;
        private WriteableBitmap bitmap;
        private bool ignorePixelScaling;

        public SKViewPort() => this.designMode = DesignerProperties.GetIsInDesignMode(this);

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

        /// <summary>Occurs when the the canvas needs to be redrawn.</summary>
        /// <remarks />
        [Category("Appearance")]
        public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        /// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.</summary>
        /// <remarks />
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (this.designMode || this.Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null)
                return;
            SKSizeI size1 = this.CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY);
            SKSizeI size2 = this.IgnorePixelScaling ? unscaledSize : size1;
            this.CanvasSize = size2;
            if (size1.Width <= 0 || size1.Height <= 0)
                return;
            SKImageInfo skImageInfo = new SKImageInfo(size1.Width, size1.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            if (this.bitmap == null || skImageInfo.Width != this.bitmap.PixelWidth || skImageInfo.Height != this.bitmap.PixelHeight)
                this.bitmap = new WriteableBitmap(skImageInfo.Width, size1.Height, 96.0 * scaleX, 96.0 * scaleY, PixelFormats.Pbgra32, null);
            this.bitmap.Lock();
            using (SKSurface surface = SKSurface.Create(skImageInfo, this.bitmap.BackBuffer, this.bitmap.BackBufferStride))
            {
                if (this.IgnorePixelScaling)
                {
                    SKCanvas canvas = surface.Canvas;
                    canvas.Scale(scaleX, scaleY);
                    canvas.Save();
                }

                this.OnPaintSurface(new SKPaintSurfaceEventArgs(surface, skImageInfo.WithSize(size2), skImageInfo));
            }

            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, skImageInfo.Width, size1.Height));
            this.bitmap.Unlock();
            drawingContext.DrawImage(this.bitmap, new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight));
        }

        /// <param name="e">The event arguments that contain the drawing surface and information.</param>
        /// <summary>Implement this to draw on the canvas.</summary>
        /// <remarks />
        protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            this.PaintSurface?.Invoke(this, e);
        }

        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        /// <summary>Raises the SizeChanged event, using the specified information as part of the eventual event data.</summary>
        /// <remarks />
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.InvalidateVisual();
        }

        private SKSizeI CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY)
        {
            scaleX = 1f;
            scaleY = 1f;
            double actualWidth = this.ActualWidth;
            double actualHeight = this.ActualHeight;
            if (IsPositive(actualWidth) && IsPositive(actualHeight))
            {
                unscaledSize = new SKSizeI((int) actualWidth, (int) actualHeight);
                Matrix transformToDevice = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
                scaleX = (float) transformToDevice.M11;
                scaleY = (float) transformToDevice.M22;
                return new SKSizeI((int) (actualWidth * scaleX), (int) (actualHeight * scaleY));
            }

            return unscaledSize = SKSizeI.Empty;
        }

        private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}