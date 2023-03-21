using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Render;

namespace FramePFX.Controls {
    public class ViewportControl : Image, OGLViewPortContext {
        private readonly object locker = new object();
        private int width;
        private int height;
        private volatile bool isUpdatingViewPort;
        private volatile bool isReadyToRender;
        private volatile WriteableBitmap bitmap;
        private volatile IntPtr backBuffer;
        private volatile bool hasFreshFrame;

        public WriteableBitmap CurrentFrame => this.bitmap;
        public IntPtr CurrentFramePtr => this.backBuffer;

        public int ViewportWidth {
            get => this.width;
            set => this.UpdateViewport(value, this.height);
        }

        public int ViewportHeight {
            get => this.height;
            set => this.UpdateViewport(this.width, value);
        }

        public bool IsOGLReady {
            get => this.isReadyToRender && !this.isUpdatingViewPort;
        }

        public bool HasFreshFrame {
            get => this.hasFreshFrame;
            set => this.hasFreshFrame = value;
        }

        public ViewportControl() {
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.CreateBitmap();
        }

        public void UpdateImageForRenderedBitmap() {
            if (this.hasFreshFrame) {
                this.bitmap.Lock();
                this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.width, this.height));
                this.bitmap.Unlock();
                this.hasFreshFrame = false;
            }
        }

        public void UpdateViewport(int w, int h) {
            lock (this.locker) {
                if ((w = Math.Max(w, 1)) == this.width && (h = Math.Max(h, 1)) == this.height) {
                    return;
                }

                this.width = w;
                this.height = h;
                if (this.isUpdatingViewPort) {
                    return;
                }

                this.isReadyToRender = false;
                this.isUpdatingViewPort = true;
                this.Dispatcher.Invoke(() => {
                    lock (this.locker) {
                        this.CreateBitmap();
                    }
                });
            }
        }

        private void CreateBitmap() {
            this.bitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Rgb24, null);
            this.Source = this.bitmap;
            this.backBuffer = this.bitmap.BackBuffer;
            this.isUpdatingViewPort = false;
            this.isReadyToRender = true;
        }
    }
}