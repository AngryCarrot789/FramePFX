using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AvalonDock.Layout;
using FramePFX.Editor;
using FramePFX.Editor.Rendering;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History.ViewModels;
using FramePFX.Logger;
using FramePFX.Notifications;
using FramePFX.Notifications.Types;
using FramePFX.Utils;
using FramePFX.WPF.Editor.Timelines;
using FramePFX.WPF.Editor.Timelines.Controls;
using FramePFX.WPF.Notifications;
using FramePFX.WPF.Themes;
using FramePFX.WPF.Views;
using SkiaSharp;
using Timeline = FramePFX.Editor.Timelines.Timeline;

namespace FramePFX.WPF.Editor.MainWindow {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorMainWindow : WindowEx, IVideoEditor, INotificationHandler {
        private const string AnimationCompletedKey = "AnimationCompleted";
        private int number;
        private volatile int isRenderActive;
        private Task lastRenderTask = null;

        public NotificationPanelViewModel NotificationPanel { get; }

        public VideoEditorViewModel Editor => (VideoEditorViewModel) this.DataContext;

        public EditorMainWindow() {
            this.InitializeComponent();
            IoC.BroadcastShortcutActivity = (x) => {
                this.NotificationBarTextBlock.Text = x;
            };

            this.NotificationPanel = new NotificationPanelViewModel(this);
            this.DataContext = new VideoEditorViewModel(this);

            this.NotificationPanelPopup.DataContext = this.NotificationPanel;
            this.TimelineLayoutPane.PropertyChanged += this.TimelineLayoutPaneOnPropertyChanged;
            this.MyDockingManager.ActiveContentChanged += this.MyDockingManagerOnActiveContentChanged;
            this.TimelineLayoutPane.Children.CollectionChanged += (sender, e) => {
                VideoEditorViewModel editor = this.Editor;
                if (editor == null) {
                    return;
                }

                switch (e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        foreach (TimelineViewModel item in GetTimelinesFromLayoutAnchorable(e.NewItems.Cast<LayoutAnchorable>()))
                            editor.OnTimelineOpened(item);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (TimelineViewModel item in GetTimelinesFromLayoutAnchorable(e.OldItems.Cast<LayoutAnchorable>()))
                            editor.OnTimelineClosed(item);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (TimelineViewModel item in GetTimelinesFromLayoutAnchorable(e.OldItems.Cast<LayoutAnchorable>()))
                            editor.OnTimelineClosed(item);
                        foreach (TimelineViewModel item in GetTimelinesFromLayoutAnchorable(e.NewItems.Cast<LayoutAnchorable>()))
                            editor.OnTimelineOpened(item);
                        break;
                    case NotifyCollectionChangedAction.Move: break;
                    case NotifyCollectionChangedAction.Reset:
                        editor.OnTimelinesCleared();
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            };
        }

        private static IEnumerable<TimelineViewModel> GetTimelinesFromLayoutAnchorable(IEnumerable<LayoutAnchorable> enumerable) {
            return enumerable.Select(x => ((PreAnchoredTimelineControl) x.Content).DataContext as TimelineViewModel).Where(x => x != null);
        }

        private void MyDockingManagerOnActiveContentChanged(object sender, EventArgs e) {
            VideoEditorViewModel editor = this.Editor;
            if (editor != null && this.MyDockingManager.ActiveContent is PreAnchoredTimelineControl control) {
                if (control.DataContext is TimelineViewModel timeline) {
                    VideoEditorViewModel.OnSelectedTimelineChangedInternal(editor, timeline);
                }
            }
        }

        private void TimelineLayoutPaneOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.TimelineLayoutPane.SelectedContent)) {
                VideoEditorViewModel editor = this.Editor;
                if (editor == null) {
                    return;
                }

                LayoutContent selected = this.TimelineLayoutPane.SelectedContent;
                if (selected != null && selected.Content is PreAnchoredTimelineControl control) {
                    if (control.DataContext is TimelineViewModel timeline) {
                        VideoEditorViewModel.OnSelectedTimelineChangedInternal(editor, timeline);
                        return;
                    }

                    if (editor.ActiveProject != null) {
                        VideoEditorViewModel.OnSelectedTimelineChangedInternal(editor, editor.ActiveProject.Timeline);
                    }
                }
            }
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
            BaseViewModel.RemoveInternalData(notification, AnimationCompletedKey);
            int index = (notification.Panel ?? this.NotificationPanel).Notifications.IndexOf(notification);
            if (index == -1) {
                BaseViewModel.SetInternalData(notification, AnimationCompletedKey, BoolBox.True);
                return;
            }

            if (this.PopupNotificationList.ItemContainerGenerator.ContainerFromIndex(index) is NotificationControl control) {
                DoubleAnimation animation = new DoubleAnimation(1d, 0d, TimeSpan.FromSeconds(2), FillBehavior.Stop);
                animation.Completed += (sender, args) => {
                    onCompleteCallback?.Invoke(notification, BaseViewModel.GetInternalData<bool>(notification, AnimationCompletedKey));
                };

                control.BeginAnimation(OpacityProperty, animation);
            }
        }

        public void CancelNotificationFadeOutAnimation(NotificationViewModel notification) {
            if (BaseViewModel.GetInternalData<bool>(notification, AnimationCompletedKey)) {
                return;
            }

            BaseViewModel.SetInternalData(notification, AnimationCompletedKey, BoolBox.True);
            int index = (notification.Panel ?? this.NotificationPanel).Notifications.IndexOf(notification);
            if (index == -1) {
                return;
            }

            if (this.PopupNotificationList.ItemContainerGenerator.ContainerFromIndex(index) is NotificationControl control) {
                control.BeginAnimation(OpacityProperty, null);
            }
        }

        private IEnumerable<PreAnchoredTimelineControl> PreAnchoredTimelineControls => this.TimelineLayoutPane.Children.Select(x => (PreAnchoredTimelineControl) x.Content);

        public void CloseAllTimelinesExcept(TimelineViewModel timeline) {
            ObservableCollection<LayoutAnchorable> list = this.TimelineLayoutPane.Children;
            for (int i = list.Count - 1; i >= 0; i--) {
                if (((PreAnchoredTimelineControl) list[i].Content).DataContext != timeline) {
                    list.RemoveAt(i);
                }
            }
        }

        public void OpenAndSelectTimeline(TimelineViewModel timeline) {
            int i = 0;
            foreach (LayoutAnchorable anchorable in this.TimelineLayoutPane.Children) {
                if (ReferenceEquals(((PreAnchoredTimelineControl) anchorable.Content).DataContext, timeline)) {
                    this.TimelineLayoutPane.SelectedContentIndex = i;
                    if (anchorable.IsHidden) {
                        anchorable.Show();
                    }

                    return;
                }

                i++;
            }

            LayoutAnchorable timelineAnchorable = new LayoutAnchorable {
                Content = new PreAnchoredTimelineControl() {
                    DataContext = timeline
                },
                CanClose = true,
                CanDockAsTabbedDocument = true
            };

            BindingOperations.SetBinding(timelineAnchorable, LayoutContent.TitleProperty, new Binding(nameof(timeline.DisplayName)) {
                Source = timeline,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            this.TimelineLayoutPane.Children.Add(timelineAnchorable);
        }

        public bool GetTimelineControlForTimeline(TimelineViewModel timeline, out TimelineControl control) {
            foreach (LayoutAnchorable anchorable in this.TimelineLayoutPane.Children) {
                PreAnchoredTimelineControl preAnchorControl = (PreAnchoredTimelineControl) anchorable.Content;
                if (preAnchorControl.PART_TimelineControl == null) {
                    continue;
                }

                TimelineViewModel openedTimeline = (TimelineViewModel) preAnchorControl.DataContext;
                if (openedTimeline == timeline) {
                    control = preAnchorControl.PART_TimelineControl;
                    return true;
                }
            }

            control = null;
            return false;
        }

        public void OnFrameRateRatioChanged(TimelineViewModel timeline, double ratio) {
            if (this.GetTimelineControlForTimeline(timeline, out TimelineControl control)) {
                control.TimelineEditor?.OnFrameRateRatioChanged(ratio);
            }
        }

        public void OnExportBegin(bool prepare) {
        }

        public void OnExportEnd() {
        }

        public async Task RenderToViewPortAsync(Timeline timeline, bool scheduleRender) {
            if (Interlocked.CompareExchange(ref this.isRenderActive, 1, 0) != 0) {
                return;
            }

            if (scheduleRender) {
                // if a render is already scheduled and not completed, don't schedule render again.
                // RenderToViewPortAsync can be called from any thread, and isRenderActive provides
                // a guard against this changing
                if (this.lastRenderTask == null || this.lastRenderTask.IsCompleted) {
                    this.lastRenderTask = IoC.Application.Dispatcher.InvokeAsync(() => {
                        this.lastRenderTask = this.RenderTimelineInternal(timeline);
                    });
                }
            }
            else if (this.Dispatcher.CheckAccess()) {
                // could check this, but it risks a potential locked state with isRenderActive... maybe
                // if (this.lastRenderTask?.IsCompleted ?? false)
                //     return;
                await this.RenderTimelineInternal(timeline);
            }
            else {
                await await IoC.Application.Dispatcher.InvokeAsync(() => this.RenderTimelineInternal(timeline));
            }
        }

        private async Task RenderTimelineInternal(Timeline timeline) {
            VideoEditorViewModel editor = this.Editor;
            ProjectViewModel project = editor.ActiveProject;
            if (project == null || project.Model.IsSaving || project.Model.IsExporting) {
                this.isRenderActive = 0;
                return;
            }

            try {
                CancellationTokenSource source = new CancellationTokenSource(-1);
                long frame = timeline.PlayHeadFrame;
                if (this.ViewPortControl.ViewPortElement.BeginRender(out SKSurface surface)) {
                    try {
                        RenderContext context = new RenderContext(surface, surface.Canvas, this.ViewPortControl.ViewPortElement.FrameInfo);
                        context.SetRenderQuality(project.Model.RenderQuality);
                        context.ClearPixels();
                        try {
                            await timeline.RenderAsync(context, frame, source.Token);

                            // Was debugging why composition clip's weren't rendering correctly, and it was
                            // because they were scheduling their own render while another timeline was selected.
                            // Therefore, it's important the editor has final say over which specific timeline is drawn
                            // to the preview, based on the selected timeline
                            // AppLogger.WriteLine("Timeline rendered at: " + frame);
                        }
                        catch (TaskCanceledException) {
                            AppLogger.WriteLine("Render at " + nameof(this.RenderTimelineInternal) + " took longer than 3 second");
                        }
                        catch (Exception e) {
                            AppLogger.WriteLine("Exception rendering timeline: " + e.GetToString());
                            await editor.Playback.StopForRenderException();
                            await IoC.DialogService.ShowMessageAsync("Render error", $"An error occurred while rendering timeline. See the logs for more info");
                        }
                    }
                    finally {
                        this.ViewPortControl.ViewPortElement.EndRender();
                    }
                }
            }
            finally {
                this.isRenderActive = 0;
            }
        }

        protected override async Task<bool> CanCloseAsync() {
            try {
                if (!await this.Editor.PromptSaveAndCloseProjectAction()) {
                    return false;
                }
            }
            catch (Exception e) {
                await IoC.DialogService.ShowMessageExAsync("Failed to close project", "Exception while closing project", e.GetToString());
            }

            try {
                this.Editor.Dispose();
            }
            catch (Exception e) {
                await IoC.DialogService.ShowMessageExAsync("Failed to dispose", "Exception while disposing editor", e.GetToString());
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
                track.Height = Maths.Clamp(track.Height + e.VerticalChange, 24, 500);
            }
        }

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

            ThemeController.SetTheme(type);
        }
    }
}