using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FramePFX.Core;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Notifications;
using FramePFX.Core.Notifications.Types;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using FramePFX.Editor.Properties.Pages;
using FramePFX.Notifications;
using FramePFX.Shortcuts;
using FramePFX.Views;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace FramePFX.Editor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorMainWindow : WindowEx, IVideoEditor, INotificationHandler {
        public VideoEditorViewModel Editor => (VideoEditorViewModel) this.DataContext;

        private readonly Action renderCallback;

        private long lastRefreshTime;
        private bool isRefreshing;
        private const long RefreshInterval = 5000;

        public NotificationPanelViewModel NotificationPanel { get; }

        public EditorMainWindow() {
            BindingErrorListener.Listen();
            this.InitializeComponent();
            // this.oglPort = new OGLMainViewPortImpl(this.GLViewport);
            IoC.BroadcastShortcutActivity = (x) => {
                this.NotificationBarTextBlock.Text = x;
            };

            this.NotificationPanel = new NotificationPanelViewModel(this);

            this.DataContext = new VideoEditorViewModel(this);
            this.renderCallback = () => {
                this.ViewPortElement.InvalidateVisual();
            };

            this.lastRefreshTime = Time.GetSystemMillis();

            this.NotificationPanelPopup.StaysOpen = true;
            this.NotificationPanelPopup.Placement = PlacementMode.Absolute;
            this.NotificationPanelPopup.PlacementTarget = this;
            this.NotificationPanelPopup.PlacementRectangle = System.Windows.Rect.Empty;
            this.NotificationPanelPopup.DataContext = this.NotificationPanel;
            this.RefreshPopupLocation();

            this.Width = 1257;
        }

        public void OnNotificationPushed(NotificationViewModel notification) {
            if (!this.NotificationPanelPopup.IsOpen)
                this.NotificationPanelPopup.IsOpen = true;
            this.Dispatcher.InvokeAsync(this.RefreshPopupLocation, DispatcherPriority.Render);
        }

        public void OnNotificationRemoved(NotificationViewModel notification) {
            if (this.NotificationPanel.Notifications.Count < 1) {
                this.NotificationPanelPopup.IsOpen = false;
            }

            this.Dispatcher.InvokeAsync(this.RefreshPopupLocation, DispatcherPriority.Loaded);
        }

        public void BeginNotificationFadeOutAnimation(NotificationViewModel notification, Action<NotificationViewModel> onCompleteCallback = null) {
            NotificationList list = VisualTreeUtils.FindVisualChild<NotificationList>(this.NotificationPanelPopup.Child);
            if (list == null) {
                return;
            }

            int index = (notification.Panel ?? this.NotificationPanel).Notifications.IndexOf(notification);
            if (index == -1) {
                throw new Exception("Item not present in panel");
            }

            if (!(list.ItemContainerGenerator.ContainerFromIndex(index) is NotificationControl control)) {
                return;
            }

            DoubleAnimation animation = new DoubleAnimation(1d, 0d, TimeSpan.FromSeconds(2), FillBehavior.Stop);
            animation.Completed += (sender, args) => {
                if (onCompleteCallback != null) {
                    if (!BaseViewModel.GetInternalData<bool>(notification, "IsCancelled"))
                        onCompleteCallback(notification);
                }
            };

            control.BeginAnimation(OpacityProperty, animation);
        }

        public void CancelNotificationFadeOutAnimation(NotificationViewModel notification) {
            NotificationList list = VisualTreeUtils.FindVisualChild<NotificationList>(this.NotificationPanelPopup.Child);
            if (list == null) {
                return;
            }

            int index = (notification.Panel ?? this.NotificationPanel).Notifications.IndexOf(notification);
            if (index == -1) {
                throw new Exception("Item not present in panel");
            }

            if (!(list.ItemContainerGenerator.ContainerFromIndex(index) is NotificationControl control)) {
                return;
            }

            BaseViewModel.SetInternalData(notification, "IsCancelled", BoolBox.True);
            control.BeginAnimation(OpacityProperty, null);
        }

        protected override void OnLocationChanged(EventArgs e) {
            base.OnLocationChanged(e);
            this.RefreshPopupLocation();
        }

        public void RefreshPopupLocation() {
            Popup popup = this.NotificationPanelPopup;
            if (popup == null || !popup.IsOpen) {
                return;
            }

            if (!(popup.Child is FrameworkElement element)) {
                return;
            }

            // winpos = X 1663
            // popup pos = X 1620
            // popup wid = 300

            popup.VerticalOffset = this.Top + this.ActualHeight - element.ActualHeight;
            popup.HorizontalOffset = this.Left + this.ActualWidth - element.ActualWidth;
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);
            if (this.isRefreshing || this.Editor.IsClosingProject) {
                return;
            }

            long time = Time.GetSystemMillis();
            if ((time - this.lastRefreshTime) >= RefreshInterval) {
                this.isRefreshing = true;
                this.RefreshAction();
            }
        }

        private async void RefreshAction() {
            if (this.Editor.ActiveProject is ProjectViewModel vm) {
                await ResourceCheckerViewModel.ProcessProjectForInvalidResources(vm, false);
            }

            this.isRefreshing = false;
            this.lastRefreshTime = Time.GetSystemMillis();
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

        public void PushNotificationMessage(string message) {
            this.NotificationBarTextBlock.Text = message;
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
                if (PropertyPageRegistry.Instance.GenerateControl(type, clip, out FrameworkElement control)) {
                    control.DataContext = clip;
                    controls.Add(control);
                }
            }

            if (controls.Count < 1) {
                return;
            }

            // this.ClipPropertyPanelList.Items.Add(controls[0]);
            foreach (FrameworkElement t in controls) {
                this.ClipPropertyPanelList.Items.Add(t);
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

            RenderContext context = new SkiaSharpRenderContext(editor.Model, e.Surface, e.Surface.Canvas, e.RawInfo);
            context.Canvas.Clear(SKColors.Black);
            project.Timeline.Model.Render(context);

            // context.Canvas.Translate(100,100);
            // context.Canvas.Scale(100f);
            // context.Canvas.DrawVertices(SKVertexMode.Triangles, new SKPoint[] {
            //     new SKPoint(0, 0),
            //     new SKPoint(1, 1),
            //     new SKPoint(0, 1)
            // }, new SKColor[] {
            //     SKColors.Red,
            //     SKColors.Green,
            //     SKColors.Blue,
            // }, new SKPaint() {Color = SKColors.Aqua});

            this.isRenderScheduled = 0;
        }

        protected override async Task<bool> OnClosingAsync() {
            try {
                if (!await this.Editor.PromptSaveAndCloseProjectAction()) {
                    return false;
                }
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

        // Dragging a top thumb to resize between 2 layers is a bit iffy... so nah
        // private void OnTopThumbDrag(object sender, DragDeltaEventArgs e) {
        //     if ((sender as Thumb)?.DataContext is LayerViewModel layer) {
        //         double layerHeight = layer.Height - e.VerticalChange;
        //         if (layerHeight < layer.MinHeight || layerHeight > layer.MaxHeight) {
        //             if (layer.Timeline.GetPrevious(layer) is LayerViewModel behind1) {
        //                 double behindHeight = behind1.Height + e.VerticalChange;
        //                 if (behindHeight < behind1.MinHeight || behindHeight > behind1.MaxHeight)
        //                     return;
        //                 behind1.Height = behindHeight;
        //             }
        //         }
        //         else if (layer.Timeline.GetPrevious(layer) is LayerViewModel behind2) {
        //             double behindHeight = behind2.Height + e.VerticalChange;
        //             if (behindHeight < behind2.MinHeight || behindHeight > behind2.MaxHeight) {
        //                 return;
        //             }
        //             layer.Height = layerHeight;
        //             behind2.Height = behindHeight;
        //         }
        //     }
        // }

        private void OnBottomThumbDrag(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is LayerViewModel layer) {
                layer.Height = Maths.Clamp(layer.Height + e.VerticalChange, layer.MinHeight, layer.MaxHeight);
            }
        }

        private void OnFitContentToWindowClick(object sender, RoutedEventArgs e) {
            this.VPViewBox.FitContentToCenter();
        }

        private int number;

        private void MenuItem_OnClick(object sender, RoutedEventArgs e) {
            this.NotificationPanel.PushNotification(new MessageNotification("Header!!!", $"Some message here ({++this.number})", TimeSpan.FromSeconds(5)));
        }
    }
}
