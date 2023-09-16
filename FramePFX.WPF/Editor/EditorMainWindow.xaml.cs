﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using FramePFX.Editor;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Effects;
using FramePFX.History.ViewModels;
using FramePFX.Notifications;
using FramePFX.Notifications.Types;
using FramePFX.PropertyEditing;
using FramePFX.Rendering;
using FramePFX.Utils;
using FramePFX.WPF.Notifications;
using FramePFX.WPF.Themes;
using FramePFX.WPF.Views;
using SkiaSharp;
using Time = FramePFX.Utils.Time;

namespace FramePFX.WPF.Editor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorMainWindow : WindowEx, IVideoEditor, INotificationHandler {
        public VideoEditorViewModel Editor => (VideoEditorViewModel) this.DataContext;

        private const long RefreshInterval = 5000;
        private long lastRefreshTime;
        private bool isRefreshing;
        private volatile int isRenderScheduled;
        private bool isPrintingErrors;

        public NotificationPanelViewModel NotificationPanel { get; }

        public EditorMainWindow() {
            this.InitializeComponent();
            // this.oglPort = new OGLMainViewPortImpl(this.GLViewport);
            IoC.BroadcastShortcutActivity = (x) => {
                this.NotificationBarTextBlock.Text = x;
            };

            this.NotificationPanel = new NotificationPanelViewModel(this);
            this.DataContext = new VideoEditorViewModel(this);
            this.lastRefreshTime = Time.GetSystemMillis();

            this.NotificationPanelPopup.DataContext = this.NotificationPanel;
            this.Width = 1257;
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);
            HistoryManagerViewModel.Instance.NotificationPanel = this.NotificationPanel;
        }

        public void OnNotificationPushed(NotificationViewModel notification) {
        }

        public void OnNotificationRemoved(NotificationViewModel notification) {
        }

        public void BeginNotificationFadeOutAnimation(NotificationViewModel notification, Action<NotificationViewModel, bool> onCompleteCallback = null) {
            BaseViewModel.ClearInternalData(notification, "AnimationCompleted");
            int index = (notification.Panel ?? this.NotificationPanel).Notifications.IndexOf(notification);
            if (index == -1) {
                BaseViewModel.SetInternalData(notification, "AnimationCompleted", BoolBox.True);
                return;
            }

            if (this.PopupNotificationList.ItemContainerGenerator.ContainerFromIndex(index) is NotificationControl control) {
                DoubleAnimation animation = new DoubleAnimation(1d, 0d, TimeSpan.FromSeconds(2), FillBehavior.Stop);
                animation.Completed += (sender, args) => {
                    onCompleteCallback?.Invoke(notification, BaseViewModel.GetInternalData<bool>(notification, "AnimationCompleted"));
                };

                control.BeginAnimation(OpacityProperty, animation);
            }
        }

        public void CancelNotificationFadeOutAnimation(NotificationViewModel notification) {
            if (BaseViewModel.GetInternalData<bool>(notification, "AnimationCompleted")) {
                return;
            }

            BaseViewModel.SetInternalData(notification, "AnimationCompleted", BoolBox.True);
            int index = (notification.Panel ?? this.NotificationPanel).Notifications.IndexOf(notification);
            if (index == -1) {
                return;
            }

            if (this.PopupNotificationList.ItemContainerGenerator.ContainerFromIndex(index) is NotificationControl control) {
                control.BeginAnimation(OpacityProperty, null);
            }
        }

        // protected override void OnActivated(EventArgs e) {
        //     base.OnActivated(e);
        //     if (this.isRefreshing || this.Editor.IsClosingProject) {
        //         return;
        //     }
        //     long time = Time.GetSystemMillis();
        //     if ((time - this.lastRefreshTime) >= RefreshInterval) {
        //         this.isRefreshing = true;
        //         this.RefreshAction();
        //     }
        // }

        // private async void RefreshAction() {
        //     if (this.Editor.ActiveProject is ProjectViewModel vm) {
        //         await ResourceCheckerViewModel.LoadProjectResources(vm, false);
        //     }
        //     this.isRefreshing = false;
        //     this.lastRefreshTime = Time.GetSystemMillis();
        // }

        public void UpdateClipSelection() {
            if (this.Editor.ActiveProject is ProjectViewModel project) {
                // TODO: maybe move this to a view model?
                List<ClipViewModel> clips = project.Timeline.Tracks.SelectMany(x => x.SelectedClips).ToList();
                PFXPropertyEditorRegistry.Instance.OnClipsSelected(clips);
            }
        }

        public void UpdateResourceSelection() {
            // TODO: maybe move this to a view model?
            ResourceManagerViewModel manager;
            if (this.Editor.ActiveProject is ProjectViewModel project && (manager = project.ResourceManager) != null) {
                PFXPropertyEditorRegistry.Instance.ResourceInfo.SetupHierarchyState(manager.SelectedItems.ToList());
            }
        }

        public void PushNotificationMessage(string message) {
            this.NotificationBarTextBlock.Text = message;
        }

        private Task lastRenderTask = Task.CompletedTask;

        public async Task Render(bool scheduleRender) {
            if (Interlocked.CompareExchange(ref this.isRenderScheduled, 1, 0) != 0) {
                return;
            }

            if (scheduleRender) {
                if (this.lastRenderTask != null) {
                    await this.lastRenderTask;
                    this.lastRenderTask = null;
                }

                this.lastRenderTask = this.Dispatcher.InvokeAsync(this.DoRenderCoreAsync).Task;
            }
            else if (this.Dispatcher.CheckAccess()) {
                await this.DoRenderCoreAsync();
            }
            else {
                // this.Dispatcher.Invoke(this.renderCallback);
                await await this.Dispatcher.InvokeAsync(this.DoRenderCoreAsync);
            }
        }

        private async Task DoRenderCoreAsync() {
            CancellationTokenSource source = new CancellationTokenSource(2000);
            try {
                await this.DoRenderCoreAsync(source.Token);
            }
            catch (TaskCanceledException) {
                AppLogger.WriteLine("Render at " + nameof(this.DoRenderCoreAsync) + " took longer than 1 second. Cancelling render");
            }
            finally {
                this.isRenderScheduled = 0;
            }
        }

        private async Task DoRenderCoreAsync(CancellationToken token) {
            VideoEditorViewModel editor = this.Editor;
            ProjectViewModel project = editor.ActiveProject;
            if (project == null || project.Model.IsSaving) {
                this.isRenderScheduled = 0;
                return;
            }

            long frame = project.Timeline.PlayHeadFrame;
            if (this.ViewPortElement.BeginRender(out SKSurface surface)) {
                try {
                    RenderContext context = new RenderContext(surface, surface.Canvas, this.ViewPortElement.FrameInfo);
                    context.Canvas.Clear(SKColors.Black);
                    try {
                        await project.Model.Timeline.RenderAsync(context, frame, token);
                    }
                    catch (TaskCanceledException) {
                        // do nothing
                    }

                    Dictionary<Clip, Exception> dictionary = project.Model.Timeline.ExceptionsLastRender;
                    if (dictionary.Count > 0) {
                        this.PrintRenderErrors(dictionary);
                        dictionary.Clear();
                    }
                }
                finally {
                    this.ViewPortElement.EndRender();
                }
            }

            this.isRenderScheduled = 0;
        }

        private async void PrintRenderErrors(Dictionary<Clip, Exception> dictionary) {
            if (this.isPrintingErrors) {
                return;
            }

            await this.Editor.Playback.StopAction();

            this.isPrintingErrors = true;
            StringBuilder sb = new StringBuilder(2048);
            foreach (KeyValuePair<Clip, Exception> entry in dictionary) {
                sb.Append($"{entry.Key.DisplayName ?? entry.Key.ToString()}: {entry.Value.GetToString()}\n");
            }

            await IoC.MessageDialogs.ShowMessageExAsync("Render error", $"An exception updating {dictionary.Count} clips", sb.ToString());
            this.isPrintingErrors = false;
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

        // Dragging a top thumb to resize between 2 tracks is a bit iffy... so nah
        // private void OnTopThumbDrag(object sender, DragDeltaEventArgs e) {
        //     if ((sender as Thumb)?.DataContext is TrackViewModel track) {
        //         double trackHeight = track.Height - e.VerticalChange;
        //         if (trackHeight < track.MinHeight || trackHeight > track.MaxHeight) {
        //             if (track.Timeline.GetPrevious(track) is trackViewModel behind1) {
        //                 double behindHeight = behind1.Height + e.VerticalChange;
        //                 if (behindHeight < behind1.MinHeight || behindHeight > behind1.MaxHeight)
        //                     return;
        //                 behind1.Height = behindHeight;
        //             }
        //         }
        //         else if (track.Timeline.GetPrevious(track) is trackViewModel behind2) {
        //             double behindHeight = behind2.Height + e.VerticalChange;
        //             if (behindHeight < behind2.MinHeight || behindHeight > behind2.MaxHeight) {
        //                 return;
        //             }
        //             track.Height = trackHeight;
        //             behind2.Height = behindHeight;
        //         }
        //     }
        // }

        private void OnBottomThumbDrag(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is TrackViewModel track) {
                track.Height = Maths.Clamp(track.Height + e.VerticalChange, track.MinHeight, track.MaxHeight);
            }
        }

        private void OnFitContentToWindowClick(object sender, RoutedEventArgs e) {
            this.VPViewBox.FitContentToCenter();
        }

        private int number;

        private void MenuItem_OnClick(object sender, RoutedEventArgs e) {
            this.NotificationPanel.PushNotification(new MessageNotification("Header!!!", $"Some message here ({++this.number})", TimeSpan.FromSeconds(5)));
        }

        private void ShowLogsClick(object sender, RoutedEventArgs e) {
            new AppLoggerWindow().Show();
        }

        private void SetThemeClick(object sender, RoutedEventArgs e) {
            ThemeType type;
            switch (((MenuItem) sender).Uid) {
                case "0":
                    type = ThemeType.DeepDark;
                    break;
                case "1":
                    type = ThemeType.SoftDark;
                    break;
                case "2":
                    type = ThemeType.DarkGreyTheme;
                    break;
                case "3":
                    type = ThemeType.GreyTheme;
                    break;
                case "4":
                    type = ThemeType.RedBlackTheme;
                    break;
                case "5":
                    type = ThemeType.LightTheme;
                    break;
                default: return;
            }

            ThemesController.SetTheme(type);
        }
    }
}