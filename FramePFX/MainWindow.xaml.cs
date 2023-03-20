using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FramePFX.Core;
using FramePFX.Render;
using FramePFX.Timeline;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.Layer.Clips;
using OpenTK.Graphics.OpenGL;

namespace FramePFX {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly OGLContextImpl ogl;

        public MainWindow() {
            this.InitializeComponent();
            IoC.Instance.Register<OGLContext>(this.ogl = new OGLContextImpl(this, this.GLViewport) {
                Width = 1920,
                Height = 1080
            });
            this.DataContext = new MainViewModel();

            this.Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e) {
            this.ogl.Stop();
        }

        public class OGLContextImpl : OGLContext, IRenderHandler {
            private readonly MainWindow window;
            private readonly Image image;
            private readonly object locker = new object();
            private int width;
            private int height;
            private volatile bool isUpdating;
            private volatile bool isReady;
            private volatile WriteableBitmap bitmap;
            private TKRenderThread openTk;

            public long lastTickTime;
            public long interval_ticks;
            public DispatcherTimer timer;

            public int Width {
                get => this.width;
                set => this.UpdateSize(value, this.height);
            }

            public int Height {
                get => this.height;
                set => this.UpdateSize(this.width, value);
            }

            public bool IsReady {
                get => this.isReady;
            }

            public OGLContextImpl(MainWindow window, Image image) {
                this.window = window;
                this.image = image;
                this.openTk = new TKRenderThread();
                this.openTk.RenderHandler = this;
                this.timer = new DispatcherTimer(DispatcherPriority.Render);
                this.timer.Interval = TimeSpan.FromMilliseconds(1);
                this.timer.Tick += this.Timer_Tick;
                this.timer.Start();

                image.Loaded += this.ImageOnLoaded;
            }

            private void Timer_Tick(object sender, EventArgs e) {
                if (this.isReady && this.openTk.IsGLEnabled) {
                    long time = DateTime.Now.Ticks;
                    long diff = time - this.lastTickTime;
                    this.interval_ticks = diff;
                    this.lastTickTime = time;
                    this.Render_WPF();
                }
            }

            public void Render_WPF() {
                // if (TKRenderThread.Instance.BitmapLock.TryLock(out CASLockType bitmapLockType)) {
                this.bitmap.Lock();
                int w = this.bitmap.PixelWidth;
                int h = this.bitmap.PixelHeight;
                if (this.openTk.DrawViewportIntoBitmap(this.bitmap.BackBuffer, w, h))
                    this.bitmap.AddDirtyRect(new Int32Rect(0, 0, w, h));
                this.bitmap.Unlock();
                // TKRenderThread.Instance.BitmapLock.Unlock(bitmapLockType);
                // }
            }

            private void ImageOnLoaded(object sender, RoutedEventArgs e) {
                this.RecreateBitmap();
                this.openTk.Width = this.width;
                this.openTk.Height = this.height;
                this.openTk.Start();
            }

            public void UpdateSize(int w, int h) {
                w = Math.Max(w, 1);
                h = Math.Max(h, 1);

                if (this.width == w && this.height == h) {
                    return;
                }

                // this.openTk.Pause();
                lock (this.locker) {
                    this.width = w;
                    this.height = h;
                    if (this.isUpdating) {
                        return;
                    }

                    this.isUpdating = true;
                    this.isReady = false;
                    this.image.Dispatcher.Invoke(() => {
                        lock (this.locker) {
                            this.RecreateBitmap();
                            // this.openTk.Start();
                        }
                    });
                }
            }

            private void RecreateBitmap() {
                this.bitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Rgb24, null);
                this.image.Source = this.bitmap;
                this.openTk.UpdateViewportSie(this.width, this.height);

                this.isUpdating = false;
                this.isReady = true;
            }

            public void Setup() {

            }

            public void Render() {
                if (TimelineViewModel.Instance != null && TimelineViewModel.Instance.IsRenderDirty) {
                    GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
                    foreach (ClipViewModel clip in TimelineViewModel.Instance.GetClipsIntersectingFrame(TimelineViewModel.Instance.PlayHeadFrame)) {
                        if (clip is VideoClipViewModel videoClip) {
                            videoClip.Render();
                        }
                    }

                    TimelineViewModel.Instance.IsRenderDirty = false;
                }
            }

            public void Tick(double interval) {

            }

            public void Stop() {
                lock (this.locker) {
                    this.isReady = false;
                    this.openTk.Stop();
                    this.openTk.Dispose();
                }
            }
        }

        private void ThumbTop(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is LayerViewModel layer) {
                double layerHeight = layer.Height - e.VerticalChange;
                if (layerHeight < layer.MinHeight || layerHeight > layer.MaxHeight) {
                    if (layer.Timeline.GetPrevious(layer) is LayerViewModel behind1) {
                        double behindHeight = behind1.Height + e.VerticalChange;
                        if (behindHeight < behind1.MinHeight || behindHeight > behind1.MaxHeight)
                            return;
                        behind1.Height = behindHeight;
                    }
                }
                else if (layer.Timeline.GetPrevious(layer) is LayerViewModel behind2) {
                    double behindHeight = behind2.Height + e.VerticalChange;
                    if (behindHeight < behind2.MinHeight || behindHeight > behind2.MaxHeight) {
                        return;
                    }

                    layer.Height = layerHeight;
                    behind2.Height = behindHeight;
                }
            }
        }

        private void ThumbBottom(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is LayerViewModel layer) {
                double layerHeight = layer.Height + e.VerticalChange;
                if (layerHeight < layer.MinHeight || layerHeight > layer.MaxHeight) {
                    return;
                }

                layer.Height = layerHeight;
            }
        }

        private void FrameworkElement_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            // Prevent the timeline scrolling when you select a clip
            e.Handled = true;
        }
    }
}
