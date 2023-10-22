﻿using System;
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
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History.ViewModels;
using FramePFX.Logger;
using FramePFX.Notifications;
using FramePFX.Notifications.Types;
using FramePFX.Rendering;
using FramePFX.Utils;
using FramePFX.WPF.Editor.Timeline;
using FramePFX.WPF.Editor.Timeline.Controls;
using FramePFX.WPF.Notifications;
using FramePFX.WPF.Themes;
using FramePFX.WPF.Views;
using SkiaSharp;

namespace FramePFX.WPF.Editor.MainWindow {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorMainWindow : WindowEx, IVideoEditor, INotificationHandler {
        public VideoEditorViewModel Editor => (VideoEditorViewModel) this.DataContext;

        private volatile int isRenderActive;

        public NotificationPanelViewModel NotificationPanel { get; }

        private readonly Func<TimelineViewModel, Task> doRenderActiveTimelineFunc;

        private int number;

        public EditorMainWindow() {
            this.InitializeComponent();
            Services.BroadcastShortcutActivity = (x) => {
                this.NotificationBarTextBlock.Text = x;
            };

            this.doRenderActiveTimelineFunc = this.RenderTimelineInternal;
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
                        foreach (TimelineViewModel item in e.NewItems.Cast<LayoutAnchorable>().Select(x => ((PreAnchoredTimelineControl) x.Content).DataContext as TimelineViewModel).Where(x => x != null))
                            editor.OnTimelineOpened(item);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (TimelineViewModel item in e.OldItems.Cast<LayoutAnchorable>().Select(x => ((PreAnchoredTimelineControl) x.Content).DataContext as TimelineViewModel).Where(x => x != null))
                            editor.OnTimelineClosed(item);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (TimelineViewModel item in e.OldItems.Cast<LayoutAnchorable>().Select(x => ((PreAnchoredTimelineControl) x.Content).DataContext as TimelineViewModel).Where(x => x != null))
                            editor.OnTimelineClosed(item);
                        foreach (TimelineViewModel item in e.NewItems.Cast<LayoutAnchorable>().Select(x => ((PreAnchoredTimelineControl) x.Content).DataContext as TimelineViewModel).Where(x => x != null))
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
            BaseViewModel.RemoveInternalData(notification, "AnimationCompleted");
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

        private IEnumerable<PreAnchoredTimelineControl> PreAnchoredTimelineControls => this.TimelineLayoutPane.Children.Select(x => (PreAnchoredTimelineControl) x.Content);

        public void CloseAllTimelinesExcept(TimelineViewModel timeline) {
            ObservableCollection<LayoutAnchorable> list = this.TimelineLayoutPane.Children;
            for (int i = list.Count - 1; i >= 0; i--) {
                if (((PreAnchoredTimelineControl) list[i].Content).DataContext == timeline) {
                    continue;
                }
                else {
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

        // old OpenGL specific code

        public void OnExportBegin(bool prepare) {
            // if (prepare)
            //     this.ViewPortElement.ClearContext();
            // else
            //     this.ViewPortElement.MakeContextCurrent();
        }

        public void OnExportEnd() {
            // this.ViewPortElement.ClearContext();
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

        private Task lastRenderTask = Task.CompletedTask;

        public async Task RenderToViewPortAsync(TimelineViewModel timeline, bool scheduleRender) {
            if (Interlocked.CompareExchange(ref this.isRenderActive, 1, 0) != 0) {
                return;
            }

            if (scheduleRender) {
                if (this.lastRenderTask != null) {
                    try {
                        await this.lastRenderTask;
                    }
                    catch (TaskCanceledException) {
                        // do nothing
                    }

                    this.lastRenderTask = null;
                }

                // does this even work properly??? we might not be awaiting the actual render task, but instead, the dispatcher task
                this.lastRenderTask = this.Dispatcher.BeginInvoke(DispatcherPriority.Send, this.doRenderActiveTimelineFunc, timeline).Task;
            }
            else if (this.Dispatcher.CheckAccess()) {
                // could check this, but it risks a potential locked state with isRenderActive... maybe
                // if (this.lastRenderTask?.IsCompleted ?? false)
                //     return;
                await this.RenderTimelineInternal(timeline);
            }
            else {
                await this.Dispatcher.BeginInvoke(DispatcherPriority.Send, this.doRenderActiveTimelineFunc, timeline).Task;
            }
        }

        private async Task RenderTimelineInternal(TimelineViewModel timeline) {
            VideoEditorViewModel editor = this.Editor;
            ProjectViewModel project = editor.ActiveProject;
            if (project == null || project.Model.IsSaving || project.Model.IsExporting) {
                Interlocked.Exchange(ref this.isRenderActive, 0);
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
                            await timeline.Model.RenderAsync(context, frame, source.Token);
                        }
                        catch (TaskCanceledException) {
                            AppLogger.WriteLine("Render at " + nameof(this.RenderTimelineInternal) + " took longer than 3 second");
                        }
                        catch (Exception e) {
                            AppLogger.WriteLine("Exception rendering timeline: " + e.GetToString());
                            await editor.Playback.StopForRenderException();
                            await Services.DialogService.ShowMessageAsync("Render error", $"An error occurred while rendering timeline. See the logs for more info");
                        }
                    }
                    finally {
                        this.ViewPortControl.ViewPortElement.EndRender();
                    }
                }
            }
            finally {
                Interlocked.Exchange(ref this.isRenderActive, 0);
            }
        }

        protected override async Task<bool> OnClosingAsync() {
            try {
                if (!await this.Editor.PromptSaveAndCloseProjectAction()) {
                    return false;
                }
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Failed to close project", "Exception while closing project", e.GetToString());
            }

            try {
                this.Editor.Dispose();
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Failed to dispose", "Exception while disposing editor", e.GetToString());
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

            ThemesController.SetTheme(type);
        }
    }
}