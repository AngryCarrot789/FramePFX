using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Utils;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace FramePFX.Render {
    /// <summary>
    /// A control that can be rendered into via OpenGL
    /// </summary>
    public class OGLViewportControl : Image {
        public static readonly DependencyProperty ViewPortWidthProperty =
            DependencyProperty.Register(
                "ViewPortWidth",
                typeof(int),
                typeof(OGLViewportControl),
                new FrameworkPropertyMetadata(
                    1,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => { ((OGLViewportControl) d).OnViewPortWidthPropertyChanged((int) e.NewValue); }));

        public static readonly DependencyProperty ViewPortHeightProperty =
            DependencyProperty.Register(
                "ViewPortHeight",
                typeof(int),
                typeof(OGLViewportControl),
                new FrameworkPropertyMetadata(
                    1,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => { ((OGLViewportControl) d).OnViewPortHeightPropertyChanged((int) e.NewValue); }));

        private readonly OGLViewPortImpl viewPort;
        private volatile WriteableBitmap bitmap;
        private volatile IntPtr backBuffer;
        private volatile bool isRenderCallbackQueued;

        /// <summary>
        /// Whether this control is ready to be rendered into or not. When not ready, it
        /// typically means the bitmap is scheduled to be updated or hasn't been created
        /// </summary>
        public volatile bool isReady;
        protected readonly DispatcherCallback createBitmapCallback;
        public bool isLoaded;
        private readonly Action renderCallback;

        public IViewPort ViewPort { get => this.viewPort; }

        public System.Windows.Media.PixelFormat BitmapPixelFormat { get; set; }

        /// <summary>
        /// The buffer mode to use when reading the OpenGL buffer (during the bitmap-drawing render phase)
        /// </summary>
        public ReadBufferMode OglBufferMode { get; }

        /// <summary>
        /// The pixel format to use when reading pixels from OpenGL (during the bitmap-drawing render phase)
        /// </summary>
        public PixelFormat OglPixelFormat { get; }

        /// <summary>
        /// The pixel type to use when reading pixels from OpenGL (during the bitmap-drawing render phase)
        /// </summary>
        public PixelType OglPixelType { get; }

        public int ViewPortWidth {
            get => (int) this.GetValue(ViewPortWidthProperty);
            set => this.SetValue(ViewPortWidthProperty, value);
        }

        public int ViewPortHeight {
            get => (int) this.GetValue(ViewPortHeightProperty);
            set => this.SetValue(ViewPortHeightProperty, value);
        }

        public OGLViewportControl() {
            // this.BitmapPixelFormat = PixelFormats.Rgb24;
            // this.OglPixelFormat = PixelFormat.Rgb;
            this.BitmapPixelFormat = PixelFormats.Bgra32;
            this.OglPixelFormat = PixelFormat.Bgra;
            this.OglPixelType = PixelType.UnsignedByte;
            this.OglBufferMode = ReadBufferMode.Back;
            this.viewPort = new OGLViewPortImpl(this);
            this.createBitmapCallback = new DispatcherCallback(this.DoCreateBitmapCore, this.Dispatcher);
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
            this.renderCallback = this.DoRenderCore;
        }

        private void DoRenderCore() {
            if (this.isReady) {
                this.RenderBitmapToWPF();
            }

            this.isRenderCallbackQueued = false;
        }

        private void DoCreateBitmapCore() {
            lock (this) {
                this.viewPort.UpdateViewPortTask?.Wait();
                this.CreateBitmap(this.viewPort.Width, this.viewPort.Height);
                this.isReady = true;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.CreateBitmap(this.viewPort.Width, this.viewPort.Height);
            this.isLoaded = true;
            this.isReady = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            this.isLoaded = false;
            this.isReady = false;
        }

        // private void TimerOnTick(object sender, EventArgs e) {
        //     this.DoRenderCore();
        // }

        private void OnFreshFrameAvailable() { // can be called from any thread
            if (this.isRenderCallbackQueued || !this.isReady) {
                return;
            }

            this.isRenderCallbackQueued = true;
            this.Dispatcher.InvokeAsync(this.renderCallback, DispatcherPriority.Render);
        }

        public void RenderBitmapToWPF() { // only called on WPF/main thread
            this.bitmap.Lock();
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.viewPort.Width, this.viewPort.Height));
            this.bitmap.Unlock();
        }

        private void CreateBitmap(int width, int height) {
            this.backBuffer = IntPtr.Zero;
            this.bitmap = new WriteableBitmap(width, height, 96, 96, this.BitmapPixelFormat, null);
            this.Source = this.bitmap;
            this.bitmap.Lock();
            this.backBuffer = this.bitmap.BackBuffer;
            this.bitmap.Unlock();
        }

        private void OnViewPortWidthPropertyChanged(int w) {
            this.viewPort.SetResolution(w, this.viewPort.Height);
        }

        private void OnViewPortHeightPropertyChanged(int h) {
            this.viewPort.SetResolution(this.viewPort.Width, h);
        }

        private class OGLViewPortImpl : IViewPort {
            private readonly OGLViewportControl control;
            private volatile int vpWidth;
            private volatile int vpHeight;

            public int Width => this.vpWidth;

            public int Height => this.vpHeight;

            public bool IsReady => this.control.isReady && this.Context != null && this.Context.IsReady;

            public IRenderContext Context => OGLUtils.GlobalContext;

            public Task UpdateViewPortTask { get; private set; }

            private readonly Action updateViewPortCallback;

            public OGLViewPortImpl(OGLViewportControl control) {
                this.control = control;
                this.vpWidth = 1;
                this.vpHeight = 1;
                this.updateViewPortCallback = () => {
                    this.Context.UpdateViewport(this.vpWidth, this.vpHeight);
                };
            }

            public void SetResolution(int width, int height) {
                lock (this.control) {
                    if (width == this.vpWidth && height == this.vpHeight) {
                        return;
                    }

                    this.control.isReady = false;
                    this.vpWidth = width;
                    this.vpHeight = height;
                    if (this.Context == null) {
                        return;
                    }

                    this.UpdateViewPortTask = this.Context.OwningThread.InvokeAsync(this.updateViewPortCallback);
                    if (this.control.isLoaded) {
                        this.control.createBitmapCallback.InvokeAsync();
                    }
                }
            }

            public bool BeginRender(bool force = false) {
                return this.control.isReady && this.Context.BeginUse(force);
            }

            public void EndRender() {
                this.Context.EndUse();
            }

            // bitmap-drawing render phase
            public void FlushFrame() {
                if (this.control.isReady && this.Context.BeginUse(false)) {
                    GL.ReadBuffer(ReadBufferMode.Back);
                    GL.ReadPixels(0, 0, this.vpWidth, this.vpHeight, this.control.OglPixelFormat, this.control.OglPixelType, this.control.backBuffer);
                    this.Context.EndUse();
                    this.control.OnFreshFrameAvailable();
                }
            }
        }

        public static PixelFormat ConvertPixelFormatWpfToGl(System.Windows.Media.PixelFormat format) {
            switch (format.ToString()) {
                case "Bgr24": return PixelFormat.Bgr;
                case "Rgb24": return PixelFormat.Rgb;
                case "Bgr32": return PixelFormat.Bgra;
                case "Bgra32": return PixelFormat.Bgra;
                case "Cmyk32": return PixelFormat.CmykExt;
                default: throw new Exception("Unknown pixel format: " + format);
            }
        }
    }
}