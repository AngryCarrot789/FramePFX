using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Core;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Editor;
using FramePFX.Editor.Timeline.ViewModels.Layer;
using FramePFX.Render.OGL;
using FramePFX.Views.Exceptions;
using SkiaSharp.Views.Desktop;

namespace FramePFX.Views.Main {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IVideoEditor {
        public PFXVideoEditor Editor => this.DataContext as PFXVideoEditor;

        public MainWindow() {
            this.InitializeComponent();
            // this.oglPort = new OGLMainViewPortImpl(this.GLViewport);
            this.Closed += this.OnClosed;
            this.Loaded += this.OnLoaded;
            IoC.BroadcastShortcutActivity = (x) => {

            };

            this.DataContext = new VideoEditorViewModel(this);
        }

        private void OnPaintViewPortSurface(object sender, SKPaintSurfaceEventArgs e) {

        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            // Task.Run(async () => {
            //     while (true) {
            //         this.oglPort.wpf_averager.PushValue(this.oglPort.interval_ticks);
            //         if (this.oglPort.wpf_averager.NextIndex % this.oglPort.wpf_averager.Count == 0) {
            //             await this.Dispatcher.InvokeAsync(() => {
            //                 double wpfItv = this.oglPort.wpf_averager.GetAverage() / TimeSpan.TicksPerMillisecond;
            //                 double oglItv = 1d / this.oglPort.openTk.AverageDelta;
            //                 double pbkItv = 1000d / ((VideoEditorViewModel) this.DataContext).Viewport.playbackAverageIntervalMS.GetAverage();
            //                 this.FPS_WPF.Text = Math.Round(1000d / wpfItv, 2).ToString();
            //                 this.FPS_OGL.Text = Math.Round(oglItv, 2).ToString();
            //                 this.PLAYBACK_FPS.Text = Math.Round(pbkItv, 2).ToString();
            //             });
            //         }
            //         await Task.Delay(10);
            //     }
            // });
        }

        private void OnClosed(object sender, EventArgs e) {
            OGLUtils.ShutdownMainThread();
            // this.oglPort.Stop();
            if (this.Editor is PFXVideoEditor editor) {
                editor.Playback.isPlaybackThreadRunning = false;
            }
        }

        private void ThumbTop(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is PFXTimelineLayer layer) {
                double layerHeight = layer.Height - e.VerticalChange;
                if (layerHeight < layer.MinHeight || layerHeight > layer.MaxHeight) {
                    if (layer.Timeline.GetPrevious(layer) is PFXTimelineLayer behind1) {
                        double behindHeight = behind1.Height + e.VerticalChange;
                        if (behindHeight < behind1.MinHeight || behindHeight > behind1.MaxHeight)
                            return;
                        behind1.Height = behindHeight;
                    }
                }
                else if (layer.Timeline.GetPrevious(layer) is PFXTimelineLayer behind2) {
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
            if ((sender as Thumb)?.DataContext is PFXTimelineLayer layer) {
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

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
        }
    }
}
