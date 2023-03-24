using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FramePFX.Render;
using FramePFX.Timeline.Layer;
using FramePFX.Utils;

namespace FramePFX.Views.Main {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public readonly OGLMainViewPortImpl oglPort;

        public VideoEditorViewModel Editor => this.DataContext as VideoEditorViewModel;

        public MainWindow() {
            this.InitializeComponent();
            this.oglPort = new OGLMainViewPortImpl(this.GLViewport);
            this.Closed += this.OnClosed;
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            Task.Run(async () => {
                while (true) {
                    this.oglPort.wpf_averager.PushValue(this.oglPort.interval_ticks);
                    if (this.oglPort.wpf_averager.NextIndex % this.oglPort.wpf_averager.Count == 0) {
                        await this.Dispatcher.InvokeAsync(() => {
                            double wpfItv = this.oglPort.wpf_averager.GetAverage() / TimeSpan.TicksPerMillisecond;
                            double oglItv = 1d / this.oglPort.openTk.AverageDelta;
                            double pbkItv = 1000d / ((VideoEditorViewModel) this.DataContext).Viewport.playbackAverageIntervalMS.GetAverage();
                            this.FPS_WPF.Text = Math.Round(1000d / wpfItv, 2).ToString();
                            this.FPS_OGL.Text = Math.Round(oglItv, 2).ToString();
                            this.PLAYBACK_FPS.Text = Math.Round(pbkItv, 2).ToString();
                        });
                    }

                    await Task.Delay(10);
                }
            });
        }

        private void OnClosed(object sender, EventArgs e) {
            this.oglPort.Stop();
            if (this.Editor is VideoEditorViewModel editor) {
                editor.Viewport.isPlaybackThreadRunning = false;
            }
        }

        public class OGLMainViewPortImpl : IOGLViewPort {
            private readonly Image image;
            private readonly object locker = new object();
            private int width;
            private int height;
            private volatile bool isUpdatingViewPort;
            private volatile bool isReadyToRender;
            private volatile WriteableBitmap bitmap;
            private volatile IntPtr backBuffer;
            public readonly OpenGLMainThread openTk;

            public long last_tick_time;
            public long interval_ticks;
            public readonly DispatcherTimer timer;
            public volatile bool hasFreshFrame;

            public IOGLContext Context => this.openTk.oglContext;

            public readonly NumberAverager wpf_averager = new NumberAverager(10);

            public bool HasFreshFrame {
                get => this.hasFreshFrame;
                set => this.hasFreshFrame = value;
            }

            public int ViewportWidth {
                get => this.width;
                set => this.UpdateViewportSize(value, this.height);
            }

            public int ViewportHeight {
                get => this.height;
                set => this.UpdateViewportSize(this.width, value);
            }

            public bool IsReadyForRender {
                get => this.isReadyToRender;
            }

            public OGLMainViewPortImpl(Image image) {
                this.image = image;
                this.width = 1;
                this.height = 1;
                this.openTk = OpenGLMainThread.Instance;
                this.timer = new DispatcherTimer(DispatcherPriority.Render) {
                    Interval = TimeSpan.FromMilliseconds(1)
                };

                this.timer.Tick += this.OnTickRender;
                this.timer.Start();
            }

            private void OnTickRender(object sender, EventArgs e) {
                if (this.isReadyToRender && this.Context.IsReady) {
                    long time = DateTime.Now.Ticks;
                    long diff = time - this.last_tick_time;
                    this.interval_ticks = diff;
                    this.last_tick_time = time;
                    this.UpdateImageForRenderedBitmap();
                }
            }

            public void UpdateImageForRenderedBitmap() {
                if (this.hasFreshFrame) {
                    this.bitmap.Lock();
                    this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.width, this.height));
                    this.bitmap.Unlock();
                    this.hasFreshFrame = false;
                }
            }

            public void UpdateViewportSize(int w, int h) {
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
                    this.image.Dispatcher.Invoke(() => {
                        lock (this.locker) {
                            this.RecreateBitmap();
                            // this.openTk.Start();
                        }
                    });
                }
            }

            public bool FlushFrame() {
                return this.hasFreshFrame = this.openTk.DrawViewportIntoBitmap(this.backBuffer, this.width, this.height);
            }

            private void RecreateBitmap() {
                this.bitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Rgb24, null);
                this.image.Source = this.bitmap;
                this.backBuffer = this.bitmap.BackBuffer;
                this.openTk.UpdateViewportSie(this.width, this.height);
                this.isUpdatingViewPort = false;
                this.isReadyToRender = true;
            }

            public void Stop() {
                lock (this.locker) {
                    this.isReadyToRender = false;
                    this.openTk.StopAndDispose();
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
