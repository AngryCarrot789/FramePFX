using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FramePFX.Core.Utils;
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
        private volatile bool hasFreshFrame;
        private readonly DispatcherTimer timer;
        public volatile bool isReady;
        protected readonly RapidDispatchCallback createBitmapCallback;
        public bool isLoaded;

        public IViewPort ViewPort { get => this.viewPort; }

        public int ViewPortWidth {
            get => (int) this.GetValue(ViewPortWidthProperty);
            set => this.SetValue(ViewPortWidthProperty, value);
        }

        public int ViewPortHeight {
            get => (int) this.GetValue(ViewPortHeightProperty);
            set => this.SetValue(ViewPortHeightProperty, value);
        }

        public OGLViewportControl() {
            this.viewPort = new OGLViewPortImpl(this);
            this.createBitmapCallback = new RapidDispatchCallback();
            this.timer = new DispatcherTimer(DispatcherPriority.Render) {
                Interval = TimeSpan.FromMilliseconds(5)
            };

            this.timer.Tick += this.TimerOnTick;
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.isLoaded = true;
            this.CreateBitmap(this.viewPort.Width, this.viewPort.Height);
            this.timer.Start();
            this.isReady = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            this.isLoaded = false;
            this.isReady = false;
            this.timer.Stop();
        }

        private void TimerOnTick(object sender, EventArgs e) {
            if (this.hasFreshFrame) {
                this.RenderBitmapToWPF();
                this.hasFreshFrame = false;
            }
        }

        private void OnFreshFrameAvailable() { // can be called from any thread
            this.hasFreshFrame = true;
        }

        public void RenderBitmapToWPF() { // only called on WPF/main thread
            this.bitmap.Lock();
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.viewPort.Width, this.viewPort.Height));
            this.bitmap.Unlock();
        }

        private void CreateBitmap(int width, int height) {
            this.backBuffer = IntPtr.Zero;
            this.bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            this.Source = this.bitmap;
            this.bitmap.Lock();
            this.backBuffer = this.bitmap.BackBuffer;
            this.bitmap.Unlock();
        }

        private void OnViewPortWidthPropertyChanged(int w) {
            this.viewPort.SetSize(w, this.viewPort.Height);
        }

        private void OnViewPortHeightPropertyChanged(int h) {
            this.viewPort.SetSize(this.viewPort.Width, h);
        }

        private class OGLViewPortImpl : IViewPort {
            private readonly OGLViewportControl control;
            private volatile int vpWidth;
            private volatile int vpHeight;

            public int Width => this.vpWidth;

            public int Height => this.vpHeight;

            public bool IsReady => this.control.isReady && this.Context != null && this.Context.IsReady;

            public IRenderContext Context => OGLUtils.GlobalContext;

            private readonly Action updateViewPortCallback;

            public OGLViewPortImpl(OGLViewportControl control) {
                this.control = control;
                this.vpWidth = 1;
                this.vpHeight = 1;
                this.updateViewPortCallback = () => {
                    this.Context.UpdateViewport(this.vpWidth, this.vpHeight);
                };
            }

            public void SetSize(int width, int height) {
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

                    Task task = this.Context.OwningThread.InvokeAsync(this.updateViewPortCallback);
                    if (this.control.isLoaded) {
                        this.control.createBitmapCallback.Invoke(() => {
                            lock (this.control) {
                                task.Wait();
                                this.control.CreateBitmap(this.vpWidth, this.vpHeight);
                                this.control.isReady = true;
                            }
                        });
                    }
                }
            }

            public bool BeginRender(bool force = false) {
                return this.control.isReady && this.Context.BeginUse(force);
            }

            public void EndRender() {
                this.Context.EndUse();
            }

            public void FlushFrame() {
                if (this.Context.BeginUse(false)) {
                    GL.ReadBuffer(ReadBufferMode.Back);
                    GL.ReadPixels(0, 0, this.vpWidth, this.vpHeight, PixelFormat.Rgb, PixelType.UnsignedByte, this.control.backBuffer);
                    this.control.OnFreshFrameAvailable();
                    this.Context.EndUse();
                }
            }
        }
    }
}