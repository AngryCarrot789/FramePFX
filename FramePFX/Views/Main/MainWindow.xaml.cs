using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FramePFX.Project;
using FramePFX.Render;
using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Utils;
using OpenTK.Graphics.OpenGL;

namespace FramePFX {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly OGLMainViewPortImpl oglMainViewPort;
        public PlaybackViewportViewModel Editor => this.DataContext as PlaybackViewportViewModel;

        public MainWindow() {
            this.InitializeComponent();
            this.oglMainViewPort = new OGLMainViewPortImpl(this, this.GLViewport, 1920, 1080);
            PlaybackViewportViewModel editor = new PlaybackViewportViewModel {
                ViewportHandle = this.oglMainViewPort
            };

            this.DataContext = editor;
            this.Closed += this.OnClosed;
        }

        private void OnClosed(object sender, EventArgs e) {
            this.oglMainViewPort.Stop();

            if (this.Editor is PlaybackViewportViewModel editor) {
                editor.isPlaybackThreadRunning = false;
            }
        }

        public class OGLMainViewPortImpl : IAutoRenderTarget {
            private readonly MainWindow window;
            private readonly Image image;
            private readonly object locker = new object();
            private int width;
            private int height;
            private volatile bool isUpdatingViewPort;
            private volatile bool isReadyToRender;
            private volatile WriteableBitmap bitmap;
            private volatile IntPtr backBuffer;
            private OpenTKRenderThread openTk;

            public long last_tick_time;
            public long interval_ticks;
            public readonly DispatcherTimer timer;
            public volatile bool hasFreshFrame;

            public WriteableBitmap CurrentFrame => this.bitmap;
            public IntPtr CurrentFramePtr => this.backBuffer;

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

            public OGLMainViewPortImpl(MainWindow window, Image image, int w, int h) {
                this.width = w;
                this.height = h;
                this.window = window;
                this.image = image;
                this.openTk = new OpenTKRenderThread {
                    Width = w, Height = h,
                    MainViewPort = this
                };
                this.timer = new DispatcherTimer(DispatcherPriority.Render) {
                    Interval = TimeSpan.FromMilliseconds(1)
                };
                this.timer.Tick += this.Timer_Tick;
                this.timer.Start();
                image.Loaded += this.ImageOnLoaded;
                this.UpdateViewportSize(w, h);
            }

            private void Timer_Tick(object sender, EventArgs e) {
                if (this.isReadyToRender && this.openTk.IsGLEnabled) {
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

            private void ImageOnLoaded(object sender, RoutedEventArgs e) {
                this.RecreateBitmap();
                this.openTk.Width = this.width;
                this.openTk.Height = this.height;
                this.openTk.Start();

                Task.Run(async () => {
                    while (true) {
                        this.wpf_averager.PushValue(this.interval_ticks);
                        if (this.wpf_averager.NextIndex % this.wpf_averager.Count == 0) {
                            await this.window.Dispatcher.InvokeAsync(() => {
                                double wpfItv = this.wpf_averager.GetAverage() / TimeSpan.TicksPerMillisecond;
                                double oglItv = 1d / this.openTk.AverageDelta;
                                double pbkItv = 1000d / ((PlaybackViewportViewModel) this.window.DataContext).playbackAverageIntervalMS.GetAverage();

                                this.window.FPS_WPF.Text = Math.Round(IntervalToFPS(wpfItv), 2).ToString();
                                this.window.FPS_OGL.Text = Math.Round(oglItv, 2).ToString();
                                this.window.PLAYBACK_FPS.Text = Math.Round(pbkItv, 2).ToString();
                            });
                        }

                        await Task.Delay(10);
                    }
                });
            }

            public static double IntervalToFPS(double itv_ms) {
                return 1000d / itv_ms;
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

            private void RecreateBitmap() {
                this.bitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Rgb24, null);
                this.image.Source = this.bitmap;
                this.backBuffer = this.bitmap.BackBuffer;
                this.openTk.UpdateViewportSie(this.width, this.height);
                this.isUpdatingViewPort = false;
                this.isReadyToRender = true;
            }

            public void Setup() {

            }

            public void Render() {
                if (!this.hasFreshFrame) {
                    ProjectViewModel project = IoC.ActiveProject;
                    if (project == null) {
                        return;
                    }

                    this.openTk.oglContext.Framebuffer.Use();
                    GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

                    long playHead = project.Timeline.PlayHeadFrame;
                    // foreach (LayerViewModel layer in timeline.Layers) {
                    //     foreach (ClipContainerViewModel clip in layer.Clips) {
                    //         if (clip.IntersectsFrameAt(playHead)) {
                    //
                    //         }
                    //     }
                    // }

                    // TODO: change this to support layer opacity. And also move to shaders because this glVertex3f old stuff it no good
                    foreach (ClipContainerViewModel clip in project.Timeline.GetClipsOnPlayHead()) {
                        IClipContainerHandle handle = clip.ContainerHandle;
                        if (handle == null) {
                            continue;
                        }

                        if (handle.ClipHandle is IClipRenderTarget target) {
                            target.Render(this, playHead);
                        }
                        // TODO: add audio... somehow. I have no idea how to do audio lololol
                        // else if (handle.ClipHandle is IAudioRenderTarget) {
                        //
                        // }
                    }

                    // this.bitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Rgb24, null);
                    this.openTk.DrawViewportIntoBitmap(this.backBuffer, this.width, this.height);
                    // this.bitmap.Freeze();
                    this.hasFreshFrame = true;
                }
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
