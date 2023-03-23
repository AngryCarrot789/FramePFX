using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Core.Render;
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

        private static void OnViewportWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue && d is OGLViewPortControl viewport && !viewport.isUpdatingProperties) {
                viewport.UpdateViewportSize((int) e.NewValue, viewport.ViewportHeight, false);
            }
        }

        private static void OnViewportHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue && d is OGLViewPortControl viewport && !viewport.isUpdatingProperties) {
                viewport.UpdateViewportSize(viewport.ViewportWidth, (int) e.NewValue, false);
            }
        }

        private readonly object locker = new object();
        private volatile bool isUpdatingViewPort;
        private volatile bool isReadyToRender;
        private volatile WriteableBitmap bitmap;
        private volatile bool hasFreshFrame;

        private volatile int targetWidth;
        private volatile int targetHeight;
        private volatile bool isUpdatingProperties;

        public IOGLContext Context => OpenTKRenderThread.GlobalContext;

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

        private volatile bool isLoaded;

        public OGLViewPortControl() {
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.ViewportControl_Unloaded;
        }

        private void ViewportControl_Unloaded(object sender, RoutedEventArgs e) {
            this.isLoaded = false;
            this.bitmap = null;
            this.Source = this.bitmap;
            this.isUpdatingViewPort = false;
            this.isReadyToRender = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            if (this.Width == 0)
                this.Width = this.ActualWidth;
            if (this.Height == 0)
                this.Height = this.ActualHeight;
            this.isLoaded = true;
            this.CreateBitmap();
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

        public void UpdateViewportSize(int w, int h, bool updateDependencProperties) {
            lock (this.locker) {
                this.targetWidth = w;
                this.targetHeight = h;
                if (this.isUpdatingViewPort || !this.isLoaded) {
                    return;
                }

                this.isReadyToRender = false;
                this.isUpdatingViewPort = true;
                this.Dispatcher.Invoke(() => {
                    lock (this.locker) {
                        this.CreateBitmap();
                        if (updateDependencProperties) {
                            this.isUpdatingProperties = true;
                            this.ViewportWidth = this.targetWidth;
                            this.ViewportHeight = this.targetHeight;
                            this.isUpdatingProperties = false;
                        }
                    }
                });
            }
        }

        private void CreateBitmap() {
            this.ValidateTargetSize();
            this.bitmap = new WriteableBitmap(this.targetWidth, this.targetHeight, 96, 96, PixelFormats.Rgb24, null);
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