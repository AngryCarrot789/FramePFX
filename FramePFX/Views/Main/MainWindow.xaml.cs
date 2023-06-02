using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Core;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Editor;
using FramePFX.Editor.Timeline.ViewModels.Layer;
using SkiaSharp.Views.Desktop;

namespace FramePFX.Views.Main {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowEx, IVideoEditor {
        public VideoEditorViewModel Editor { get; }

        public MainWindow() {
            this.InitializeComponent();
            // this.oglPort = new OGLMainViewPortImpl(this.GLViewport);
            IoC.BroadcastShortcutActivity = (x) => {

            };

            this.DataContext = this.Editor = new VideoEditorViewModel(this, IoC.App);
        }

        private void OnPaintViewPortSurface(object sender, SKPaintSurfaceEventArgs e) {
            VideoEditorViewModel editor = this.Editor;
            if (editor.IsProjectSaving) {
                return;
            }

        }

        protected override async Task<bool> OnClosingAsync() {
            try {
                await this.Editor.CloseProjectAction();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Failed to close project", "Exception while closing project", e.GetToString());
            }

            try {
                this.Editor.Dispose();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Failed to dispose", "Exception while disposing editor", e.GetToString());
            }

            return true;
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
    }
}
