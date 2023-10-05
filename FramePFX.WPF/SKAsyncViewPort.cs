using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Utils;
using SkiaSharp;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;

namespace FramePFX.WPF
{
    public sealed class SKAsyncViewPort : FrameworkElement
    {
        public static readonly DependencyProperty UseNearestNeighbourRenderingProperty =
            DependencyProperty.Register(
                "UseNearestNeighbourRendering",
                typeof(bool),
                typeof(SKAsyncViewPort),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => RenderOptions.SetBitmapScalingMode(d, (bool) e.NewValue ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.Unspecified)));

        public bool UseNearestNeighbourRendering
        {
            get => (bool) this.GetValue(UseNearestNeighbourRenderingProperty);
            set => this.SetValue(UseNearestNeighbourRenderingProperty, value);
        }

        /// <summary>Gets the current canvas size.</summary>
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

        public SKImageInfo FrameInfo => this.skImageInfo;

        private volatile SKSurface targetSurface;
        private SKImageInfo skImageInfo;
        private readonly bool designMode;
        private WriteableBitmap bitmap;
        private bool ignorePixelScaling;
        private bool isRendering;

        // unused... for now
        // private GameWindow gameWindow;
        private readonly GRGlInterface grgInterface;
        public readonly GRContext grContext;

        /// <summary>
        /// A list of clips to draw with their outline
        /// </summary>
        public List<(VideoClip, SKRect)> OutlineList { get; set; }

        public SKAsyncViewPort()
        {
            this.designMode = DesignerProperties.GetIsInDesignMode(this);
            // this.gameWindow = new GameWindow(100, 100, GraphicsMode.Default, "ok!");
            // this.gameWindow.MakeCurrent();
            // this.grgInterface = GRGlInterface.Create();
            // this.grContext = GRContext.CreateGl(this.grgInterface, new GRContextOptions());
        }

        public bool BeginRender(out SKSurface surface)
        {
            PresentationSource source;
            if (this.isRendering || this.designMode || (source = PresentationSource.FromVisual(this)) == null)
            {
                surface = null;
                return false;
            }

            SKSizeI scaledSize = this.CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, source);
            SKSizeI size = this.IgnorePixelScaling ? unscaledSize : scaledSize;
            this.CanvasSize = size;
            if (scaledSize.Width <= 0 || scaledSize.Height <= 0)
            {
                surface = null;
                return false;
            }

            SKImageInfo frameInfo = new SKImageInfo(scaledSize.Width, scaledSize.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            this.skImageInfo = frameInfo;
            WriteableBitmap bitmap = this.bitmap;
            if (bitmap == null || frameInfo.Width != bitmap.PixelWidth || frameInfo.Height != bitmap.PixelHeight)
            {
                this.bitmap = bitmap = new WriteableBitmap(
                    frameInfo.Width, scaledSize.Height,
                    unscaledSize.Width == scaledSize.Width ? 96d : (96d * scaleX),
                    unscaledSize.Height == scaledSize.Height ? 96d : (96d * scaleY),
                    PixelFormats.Pbgra32, null);
                // bitmap.Lock();
                // this.targetSurface?.Dispose();
                // this.targetSurface = SKSurface.Create(frameInfo, bitmap.BackBuffer, bitmap.BackBufferStride);
                // bitmap.Unlock();
            }

            bitmap.Lock();
            this.targetSurface = SKSurface.Create(frameInfo, bitmap.BackBuffer, bitmap.BackBufferStride);
            bitmap.Unlock();

            surface = this.targetSurface;

            // this.targetSurface = surface = SKSurface.Create((GRRecordingContext) this.grContext, true, frameInfo, 0, GRSurfaceOrigin.TopLeft);
            if (this.IgnorePixelScaling)
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Scale((float) scaleX, (float) scaleY);
                canvas.Save();
            }

            this.isRendering = true;
            return true;
        }

        public void EndRender()
        {
            SKImageInfo info = this.skImageInfo;
            this.bitmap.Lock();
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, info.Height));
            this.bitmap.Unlock();
            this.targetSurface.Canvas.Restore();

            this.targetSurface.Dispose();
            this.targetSurface = null;

            this.isRendering = false;
            this.InvalidateVisual();
        }

        private readonly Pen OutlinePen = new Pen(Brushes.Orange, 2.5f);

        protected override void OnRender(DrawingContext dc)
        {
            WriteableBitmap bmp = this.bitmap;
            if (bmp != null)
            {
                dc.DrawImage(bmp, new Rect(0d, 0d, this.ActualWidth, this.ActualHeight));
            }

            if (this.OutlineList == null)
                return;

            const double thickness = 2.5d;
            const double half = thickness / 2d;
            foreach ((VideoClip clip, SKRect rect) in this.OutlineList)
            {
                dc.DrawRectangle(null, OutlinePen, new Rect(rect.Left - half, rect.Top - half, rect.Width + thickness, rect.Height + thickness));
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