using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FramePFX.Render;

namespace FramePFX.Controls {
    public class OGLViewPortControl : Image, IOGLViewPort {
        public static readonly DependencyProperty ViewportWidthProperty =
            DependencyProperty.Register(
                "ViewportWidth",
                typeof(int),
                typeof(OGLViewPortControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnViewportWidthPropertyChanged));

        public static readonly DependencyProperty ViewportHeightProperty =
            DependencyProperty.Register(
                "ViewportHeight",
                typeof(int),
                typeof(OGLViewPortControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnViewportHeightPropertyChanged));

        private readonly object locker = new object();
        private volatile bool isUpdatingViewPort;
        private volatile bool isReadyToRender;
        private volatile WriteableBitmap bitmap;
        private volatile IntPtr backBuffer;
        private volatile bool hasFreshFrame;

        private volatile int targetWidth;
        private volatile int targetHeight;
        private volatile bool isUpdatingDependencyProperties;
        private volatile bool isLoaded;

        public IOGLContext Context => OpenGLMainThread.GlobalContext;

        public bool IsReadyForRender {
            get => this.isReadyToRender && !this.isUpdatingViewPort;
        }

        public bool HasFreshFrame {
            get => this.hasFreshFrame;
            set => this.hasFreshFrame = value;
        }

        public int ViewportWidth {
            get => (int) this.GetValue(ViewportWidthProperty);
            set => this.SetValue(ViewportWidthProperty, value);
        }

        public int ViewportHeight {
            get => (int) this.GetValue(ViewportHeightProperty);
            set => this.SetValue(ViewportHeightProperty, value);
        }

        private static HashSet<OGLViewPortControl> TICKING_CONTROLS = new HashSet<OGLViewPortControl>();
        private static DispatcherTimer TICKING_TIMER;

        public OGLViewPortControl() {
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.ViewportControl_Unloaded;
        }

        static OGLViewPortControl() {
            Application.Current.Dispatcher.Invoke(() => {
                TICKING_TIMER = new DispatcherTimer(DispatcherPriority.Render) {
                    Interval = TimeSpan.FromMilliseconds(1)
                };

                TICKING_TIMER.Tick += OnGlobalTimerTick;
                TICKING_TIMER.Start();
            });
        }

        private static void OnGlobalTimerTick(object sender, EventArgs e) {
            if (TICKING_CONTROLS.Count == 0) {
                return;
            }

            lock (TICKING_CONTROLS) {
                foreach (OGLViewPortControl ports in TICKING_CONTROLS) {
                    ports.TickRenderMainThread();
                }
            }
        }

        private void TickRenderMainThread() {
            if (this.isReadyToRender && (this.Context?.IsReady ?? false)) {
                this.UpdateImageForRenderedBitmap();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.isLoaded = true;
            if (this.targetWidth < 1) {
                this.targetWidth = 1;
            }

            if (this.targetHeight < 1) {
                this.targetHeight = 1;
            }

            this.CreateBitmap();
            lock (TICKING_CONTROLS) {
                TICKING_CONTROLS.Add(this);
            }
        }

        private void ViewportControl_Unloaded(object sender, RoutedEventArgs e) {
            this.isReadyToRender = false;
            this.isUpdatingViewPort = false;
            this.isLoaded = false;
            this.backBuffer = IntPtr.Zero;
            this.bitmap = null;
            this.Source = null;
            lock (TICKING_CONTROLS) {
                TICKING_CONTROLS.Remove(this);
            }
        }

        private static void OnViewportWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue && d is OGLViewPortControl viewport && !viewport.isUpdatingDependencyProperties) {
                viewport.UpdateViewportSize(Math.Max(1, (int) e.NewValue), viewport.ViewportHeight, false);
            }
        }

        private static void OnViewportHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue && d is OGLViewPortControl viewport && !viewport.isUpdatingDependencyProperties) {
                viewport.UpdateViewportSize(viewport.ViewportWidth, Math.Max(1, (int) e.NewValue), false);
            }
        }

        public void UpdateImageForRenderedBitmap() {
            this.Dispatcher.Invoke(() => {
                if (this.hasFreshFrame) {
                    this.bitmap.Lock();
                    this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.ViewportWidth, this.ViewportHeight));
                    this.bitmap.Unlock();
                    this.hasFreshFrame = false;
                }
            });
        }

        public void UpdateViewportSize(int w, int h) {
            this.UpdateViewportSize(w, h, true);
        }

        public bool FlushFrame() {
            if (this.isReadyToRender && !this.isUpdatingViewPort) {
                return this.hasFreshFrame = this.Context.DrawViewportIntoBitmap(this.backBuffer, this.targetWidth, this.targetHeight);
            }
            else {
                return false;
            }
        }

        public void UpdateViewportSize(int w, int h, bool updateDependencProperties) {
            lock (this.locker) {
                this.targetWidth = Math.Max(1, w);
                this.targetHeight = Math.Max(1, h);
                if (this.isUpdatingViewPort || !this.isLoaded) {
                    return;
                }

                this.isReadyToRender = false;
                this.isUpdatingViewPort = true;
                this.Dispatcher.Invoke(() => {
                    lock (this.locker) {
                        this.CreateBitmap();
                        if (updateDependencProperties) {
                            this.isUpdatingDependencyProperties = true;
                            this.ViewportWidth = this.targetWidth;
                            this.ViewportHeight = this.targetHeight;
                            this.isUpdatingDependencyProperties = false;
                        }
                    }
                });
            }
        }

        private void CreateBitmap() {
            this.ValidateTargetSize();
            this.bitmap = new WriteableBitmap(this.targetWidth, this.targetHeight, 96, 96, PixelFormats.Rgb24, null);
            this.backBuffer = this.bitmap.BackBuffer;
            this.Source = this.bitmap;
            // this.backBuffer = this.bitmap.BackBuffer;
            this.isUpdatingViewPort = false;
            this.isReadyToRender = true;
        }

        private void ValidateTargetSize() {
            if (this.targetWidth < 1) {
                throw new Exception("Viewport Width is too small: " + this.targetWidth);
            }

            if (this.targetHeight < 1) {
                throw new Exception("Viewport Height is too small: " + this.targetHeight);
            }
        }
    }
}