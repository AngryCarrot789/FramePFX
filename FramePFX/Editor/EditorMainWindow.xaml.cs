using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Core;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using FramePFX.Editor.Properties;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace FramePFX.Views.Main {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorMainWindow : WindowEx, IVideoEditor {
        public VideoEditorViewModel Editor => (VideoEditorViewModel) this.DataContext;

        private readonly Action renderCallback;

        public EditorMainWindow() {
            this.InitializeComponent();
            // this.oglPort = new OGLMainViewPortImpl(this.GLViewport);
            IoC.BroadcastShortcutActivity = (x) => {

            };

            this.DataContext = new VideoEditorViewModel(this, IoC.App);
            this.renderCallback = () => {
                this.ViewPortElement.InvalidateVisual();
            };
        }

        public void UpdateSelectionPropertyPages() {
            this.ClipPropertyPanelList.Items.Clear();
            if (this.Editor.ActiveProject is ProjectViewModel project) {
                List<ClipViewModel> list = project.Timeline.Layers.SelectMany(x => x.SelectedClips).ToList();
                if (list.Count != 1) {
                    return;
                }

                this.GeneratePropertyPages(list[0]);
            }
        }

        public void GeneratePropertyPages(ClipViewModel clip) {
            Type root = typeof(ClipViewModel);
            List<Type> types = new List<Type>();
            for (Type type = clip.GetType(); type != null && root.IsAssignableFrom(type); type = type.BaseType) {
                types.Add(type);
            }

            if (types.Count > 0) {
                types.Reverse();
                this.GeneratePropertyPages(types, clip);
            }
        }

        public void GeneratePropertyPages(List<Type> types, ClipViewModel clip) {
            List<FrameworkElement> controls = new List<FrameworkElement>(types.Count);
            foreach (Type type in types) {
                if (PropertyPageRegistry.GenerateControl(type, clip, out FrameworkElement control)) {
                    control.DataContext = clip;
                    controls.Add(control);
                }
            }

            if (controls.Count < 1) {
                return;
            }

            // this.ClipPropertyPanelList.Items.Add(controls[0]);
            for (int i = 0; i < controls.Count; i++) {
                this.ClipPropertyPanelList.Items.Add(controls[i]);
            }
        }

        private volatile int isRenderScheduled;

        public void RenderViewPort(bool scheduleRender) {
            if (Interlocked.CompareExchange(ref this.isRenderScheduled, 1, 0) != 0) {
                return;
            }

            if (scheduleRender) {
                this.Dispatcher.InvokeAsync(this.renderCallback);
            }
            else {
                this.Dispatcher.Invoke(this.renderCallback);
                this.isRenderScheduled = 0;
            }
        }

        private void OnPaintViewPortSurface(object sender, SKPaintSurfaceEventArgs e) {
            VideoEditorViewModel editor = this.Editor;
            ProjectViewModel project = editor.ActiveProject;
            if (project == null) {
                return;
            }

            if (editor.IsProjectSaving) {
                return;
            }

            RenderContext context = new RenderContext(editor.Model, e.Surface, e.Surface.Canvas, e.RawInfo);
            context.Canvas.Clear(SKColors.Black);
            context.Canvas.Save();
            project.Timeline.Model.Render(context);
            context.Canvas.Restore();
            this.isRenderScheduled = 0;
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

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.VPViewBox.FitContentToCenter();
        }
    }
}
