using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;

namespace FramePFX.WPF {
    public sealed class SKAsyncViewPort : FrameworkElement {
        public static readonly DependencyProperty UseNearestNeighbourRenderingProperty =
            DependencyProperty.Register(
                "UseNearestNeighbourRendering",
                typeof(bool),
                typeof(SKAsyncViewPort),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => RenderOptions.SetBitmapScalingMode(d, (bool) e.NewValue ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.Unspecified)));

        public static readonly DependencyProperty RenderGizmoClipProperty = DependencyProperty.Register("RenderGizmoClip", typeof(ClipViewModel), typeof(SKAsyncViewPort), new PropertyMetadata(null));

        public bool UseNearestNeighbourRendering {
            get => (bool) this.GetValue(UseNearestNeighbourRenderingProperty);
            set => this.SetValue(UseNearestNeighbourRenderingProperty, value);
        }

        public ClipViewModel RenderGizmoClip {
            get { return (ClipViewModel) this.GetValue(RenderGizmoClipProperty); }
            set { this.SetValue(RenderGizmoClipProperty, value); }
        }

        /// <summary>Gets the current canvas size.</summary>
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

        public SKImageInfo FrameInfo => this.skImageInfo;

        private volatile SKSurface targetSurface;
        private SKImageInfo skImageInfo;
        private readonly bool designMode;
        private WriteableBitmap bitmap;
        private bool ignorePixelScaling;

        // unused... for now
        private GameWindow gameWindow;
        private readonly GRGlInterface grgInterface;
        public readonly GRContext grContext;

        public SKAsyncViewPort() {
            this.designMode = DesignerProperties.GetIsInDesignMode(this);
            // this.gameWindow = new GameWindow(100, 100, GraphicsMode.Default, "ok!");
            // this.gameWindow.MakeCurrent();
            // this.grgInterface = GRGlInterface.Create();
            // this.grContext = GRContext.CreateGl(this.grgInterface, new GRContextOptions());
        }

        public bool BeginRender(out SKSurface surface) {
            PresentationSource source;
            if (this.targetSurface != null || this.designMode || (source = PresentationSource.FromVisual(this)) == null) {
                surface = null;
                return false;
            }

            SKSizeI pixelSize = this.CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, source);
            SKSizeI size = this.IgnorePixelScaling ? unscaledSize : pixelSize;
            this.CanvasSize = size;
            if (pixelSize.Width <= 0 || pixelSize.Height <= 0) {
                surface = null;
                return false;
            }

            SKImageInfo frameInfo = new SKImageInfo(pixelSize.Width, pixelSize.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            this.skImageInfo = frameInfo;
            if (this.bitmap == null || frameInfo.Width != this.bitmap.PixelWidth || frameInfo.Height != this.bitmap.PixelHeight) {
                this.bitmap = new WriteableBitmap(
                    frameInfo.Width, pixelSize.Height,
                    scaleX == 1d ? 96d : (96d * scaleX),
                    scaleY == 1d ? 96d : (96d * scaleY),
                    PixelFormats.Pbgra32, null);
                // this.gameWindow.Width = (int) this.bitmap.Width;
                // this.gameWindow.Height = (int) this.bitmap.Height;
            }

            this.bitmap.Lock();

            this.targetSurface = surface = SKSurface.Create(frameInfo, this.bitmap.BackBuffer, this.bitmap.BackBufferStride);
            // this.targetSurface = surface = SKSurface.Create((GRRecordingContext) this.grContext, true, frameInfo, 0, GRSurfaceOrigin.TopLeft);
            if (this.IgnorePixelScaling) {
                SKCanvas canvas = surface.Canvas;
                canvas.Scale((float) scaleX, (float) scaleY);
                canvas.Save();
            }

            return true;
        }

        public void EndRender() {
            SKImageInfo info = this.skImageInfo;
            // this.targetSurface.Flush();
            // // this.gameWindow.SwapBuffers();
            // GL.ReadBuffer(ReadBufferMode.Back);
            // GL.ReadPixels(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight, PixelFormat.Bgra, PixelType.UnsignedByte, this.bitmap.BackBuffer);
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, info.Height));
            this.bitmap.Unlock();
            // this.Dispatcher.Invoke(this.InvalidateVisual);
            this.targetSurface.Dispose();
            this.targetSurface = null;
            this.InvalidateVisual();
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

        private SKSizeI CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, PresentationSource source) {
            unscaledSize = SKSizeI.Empty;
            scaleX = 1f;
            scaleY = 1f;
            Size size = this.RenderSize;
            if (IsPositive(size.Width) && IsPositive(size.Height)) {
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