﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace FramePFX.Editors.Controls.Viewports {
    /// <summary>
    /// A control that allows the use of SkiaSharp to draw into a <see cref="BitmapImage"/> which gets rendered as this control's content
    /// </summary>
    public class SKAsyncViewPort : FrameworkElement {
        public static readonly DependencyProperty UseNearestNeighbourRenderingProperty =
            DependencyProperty.Register(
                "UseNearestNeighbourRendering",
                typeof(bool),
                typeof(SKAsyncViewPort),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => RenderOptions.SetBitmapScalingMode(d, (bool)e.NewValue ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.Unspecified)));

        public bool UseNearestNeighbourRendering {
            get => (bool)this.GetValue(UseNearestNeighbourRenderingProperty);
            set => this.SetValue(UseNearestNeighbourRenderingProperty, value.Box());
        }

        /// <summary>Gets the current canvas size.</summary>
        /// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
        public SKSize CanvasSize { get; private set; }

        public SKImageInfo FrameInfo => this.skImageInfo;

        private volatile SKSurface targetSurface;
        private SKImageInfo skImageInfo;
        private readonly bool designMode;
        private WriteableBitmap bitmap;
        private bool isRendering;

        private readonly GameWindow gameWindow;
        private readonly GRGlInterface grgInterface;
        private readonly GRContext grContext;

        public SKAsyncViewPort() {
            this.designMode = DesignerProperties.GetIsInDesignMode(this);
            this.UseNearestNeighbourRendering = !this.designMode; // true by default
            if (!this.designMode) {
                this.gameWindow = new GameWindow(
                    1, 1,
                    new GraphicsMode(new ColorFormat(32), 16, 0, 2, ColorFormat.Empty, 1, false),
                    "FramePFX Off-Screen Rendering Window", // title, in case another app enumerates our windows
                    GameWindowFlags.Default, // default flags (don't care about window looks as it's not shown)
                    DisplayDevice.Default, // default display
                    1, 0, // OGL version
                    GraphicsContextFlags.Offscreen, // allow off-screen rendering optimisations hopefully?
                    null, // shared context
                    true); // is single thread
                this.gameWindow.MakeCurrent();
                this.grgInterface = GRGlInterface.Create();
                this.grContext = GRContext.CreateGl(this.grgInterface, new GRContextOptions());
            }
        }

        public bool BeginRender(out SKSurface surface) {
            PresentationSource source;
            if (this.isRendering || this.designMode || (source = PresentationSource.FromVisual(this)) == null) {
                surface = null;
                return false;
            }

            SKSizeI scaledSize = this.CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, source);
            this.CanvasSize = scaledSize;
            if (scaledSize.Width <= 0 || scaledSize.Height <= 0) {
                surface = null;
                return false;
            }

            SKImageInfo frameInfo = new SKImageInfo(scaledSize.Width, scaledSize.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            this.skImageInfo = frameInfo;
            WriteableBitmap bmp = this.bitmap;
            if (bmp == null || frameInfo.Width != bmp.PixelWidth || frameInfo.Height != bmp.PixelHeight) {
                this.bitmap = new WriteableBitmap(
                    frameInfo.Width, scaledSize.Height,
                    unscaledSize.Width == scaledSize.Width ? 96d : (96d * scaleX),
                    unscaledSize.Height == scaledSize.Height ? 96d : (96d * scaleY),
                    PixelFormats.Pbgra32, null);
                this.gameWindow.Width = frameInfo.Width;
                this.gameWindow.Height = frameInfo.Height;
                this.targetSurface?.Dispose();
                this.targetSurface = surface = SKSurface.Create((GRRecordingContext)this.grContext, true, frameInfo, 0, GRSurfaceOrigin.TopLeft);
            }
            else {
                surface = this.targetSurface;
            }

            this.isRendering = true;
            this.OnBeginRender();
            return true;
        }

        public void EndRender() {
            SKImageInfo info = this.skImageInfo;
            this.targetSurface.Flush(true, true);

            this.bitmap.Lock();
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, info.Width, info.Height, PixelFormat.Bgra, PixelType.UnsignedByte, this.bitmap.BackBuffer);

            this.OnPreEndRender();
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, info.Height));
            this.bitmap.Unlock();
            this.OnPostEndRender();
            this.isRendering = false;
            this.InvalidateVisual();
        }

        protected virtual void OnBeginRender() { }

        protected virtual void OnPreEndRender() { }

        protected virtual void OnPostEndRender() { }

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
                unscaledSize = new SKSizeI((int)size.Width, (int)size.Height);
                Matrix transformToDevice = source.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
                scaleX = transformToDevice.M11;
                scaleY = transformToDevice.M22;
                return new SKSizeI((int)(size.Width * scaleX), (int)(size.Height * scaleY));
            }

            return SKSizeI.Empty;
        }

        private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}
