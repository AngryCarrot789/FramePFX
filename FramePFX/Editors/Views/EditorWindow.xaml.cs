//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Editors.Automation;
using FramePFX.Editors.ProjectProps;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Interactivity.Contexts;
using FramePFX.PropertyEditing;
using FramePFX.Tasks;
using FramePFX.Themes;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Editors.Views {
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : WindowEx {
        public static readonly DependencyProperty EditorProperty = DependencyProperty.Register("Editor", typeof(VideoEditor), typeof(EditorWindow), new PropertyMetadata(null, (o, e) => ((EditorWindow) o).OnEditorChanged((VideoEditor) e.OldValue, (VideoEditor) e.NewValue)));

        public VideoEditor Editor {
            get => (VideoEditor) this.GetValue(EditorProperty);
            set => this.SetValue(EditorProperty, value);
        }

        private readonly ContextData contextData;
        private readonly NumberAverager renderTimeAverager;

        private ActivityTask primaryActivity;

        public EditorWindow() {
            this.renderTimeAverager = new NumberAverager(5); // average 5 samples. Will take a second to catch up at 5 fps but meh
            DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.HostWindowKey, this).Clone());
            this.InitializeComponent();
            this.Loaded += this.EditorWindow_Loaded;

            TaskManager taskManager = IoC.TaskManager;
            taskManager.TaskStarted += this.OnTaskStarted;
            taskManager.TaskCompleted += this.OnTaskCompleted;
        }

        private void OnTaskStarted(TaskManager taskmanager, ActivityTask task, int index) {
            if (this.primaryActivity == null || this.primaryActivity.IsCompleted) {
                this.SetActivityTask(task);
            }
        }

        private void OnTaskCompleted(TaskManager taskmanager, ActivityTask task, int index) {
            if (task == this.primaryActivity) {
                // try to access next task
                task = taskmanager.ActiveTasks.Count > 0 ? taskmanager.ActiveTasks[0] : null;
                this.SetActivityTask(task);
            }
        }

        private void SetActivityTask(ActivityTask task) {
            IActivityProgress prog = null;
            if (this.primaryActivity != null) {
                prog = this.primaryActivity.Progress;
                prog.TextChanged -= this.OnPrimaryActivityTextChanged;
                prog.CompletionValueChanged -= this.OnPrimaryActionCompletionValueChanged;
                prog.IsIndeterminateChanged -= this.OnPrimaryActivityIndeterminateChanged;
                prog = null;
            }

            this.primaryActivity = task;
            if (task != null) {
                prog = task.Progress;
                prog.TextChanged += this.OnPrimaryActivityTextChanged;
                prog.CompletionValueChanged += this.OnPrimaryActionCompletionValueChanged;
                prog.IsIndeterminateChanged += this.OnPrimaryActivityIndeterminateChanged;
                this.PART_ActiveBackgroundTaskGrid.Visibility = Visibility.Visible;
            }
            else {
                this.PART_ActiveBackgroundTaskGrid.Visibility = Visibility.Collapsed;
            }

            this.OnPrimaryActivityTextChanged(prog);
            this.OnPrimaryActionCompletionValueChanged(prog);
            this.OnPrimaryActivityIndeterminateChanged(prog);
        }

        private void OnPrimaryActivityTextChanged(IActivityProgress tracker) {
            this.Dispatcher.Invoke(() => this.PART_TaskCaption.Text = tracker?.Text ?? "", DispatcherPriority.Loaded);
        }

        private void OnPrimaryActionCompletionValueChanged(IActivityProgress tracker) {
            this.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.Value = tracker?.TotalCompletion ?? 0.0, DispatcherPriority.Loaded);
        }

        private void OnPrimaryActivityIndeterminateChanged(IActivityProgress tracker) {
            this.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.IsIndeterminate = tracker?.IsIndeterminate ?? false, DispatcherPriority.Loaded);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override Task<bool> OnClosingAsync() {
            // Close the project (which also destroys it) so that we can safely close and destroy all
            // used objects (e.g. images, file locks, video files, etc.) even thought it may not be
            // strictly necessary, still seems like a good idea to do so
            if (this.Editor is VideoEditor editor) {
                if (editor.Project != null) {
                    editor.CloseProject();
                }

                this.Editor = null;
            }

            return Task.FromResult(true);
        }

        private void UpdateFrameRenderInterval(RenderManager manager) {
            this.renderTimeAverager.PushValue(manager.AverageVideoRenderTimeMillis);

            double averageMillis = this.renderTimeAverager.GetAverage();
            this.PART_AvgRenderTimeBlock.Text = $"{Math.Round(averageMillis, 2).ToString(),5} ms ({((int) Math.Round(1000.0 / averageMillis)).ToString(),3} FPS)";
        }

        private void EditorWindow_Loaded(object sender, RoutedEventArgs e) {
            this.ThePropertyEditor.ApplyTemplate();
            this.ThePropertyEditor.PropertyEditor = VideoEditorPropertyEditor.Instance;
            this.PART_ActiveBackgroundTaskGrid.Visibility = Visibility.Collapsed;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Key == Key.LeftAlt) {
                e.Handled = true;
            }
        }

        private void OnEditorChanged(VideoEditor oldEditor, VideoEditor newEditor) {
            if (oldEditor != null) {
                oldEditor.ProjectChanged -= this.OnEditorProjectChanged;
                oldEditor.Playback.PlaybackStateChanged -= this.OnEditorPlaybackStateChanged;
                Project oldProject = oldEditor.Project;
                if (oldProject != null) {
                    this.OnProjectChanged(oldProject, null);
                }

                this.PART_ViewPort.VideoEditor = null;
            }

            if (newEditor != null) {
                newEditor.ProjectChanged += this.OnEditorProjectChanged;
                newEditor.Playback.PlaybackStateChanged += this.OnEditorPlaybackStateChanged;
                this.PART_ViewPort.VideoEditor = newEditor;
            }

            Project project = newEditor?.Project;
            DataManager.SetContextData(this, this.contextData.Set(DataKeys.VideoEditorKey, newEditor).Set(DataKeys.ProjectKey, project).Clone());
            if (project != null) {
                this.OnProjectChanged(null, project);
            }

            this.UpdatePlayBackButtons(newEditor?.Playback);
        }

        private void OnEditorPlaybackStateChanged(PlaybackManager sender, PlayState state, long frame) {
            this.UpdatePlayBackButtons(sender);
        }

        private void UpdatePlayBackButtons(PlaybackManager manager) {
            // if (manager != null) {
            //     this.PlayBackButton_Play.IsEnabled = manager.CanSetPlayStateTo(PlayState.Play);
            //     this.PlayBackButton_Pause.IsEnabled = manager.CanSetPlayStateTo(PlayState.Pause);
            //     this.PlayBackButton_Stop.IsEnabled = manager.CanSetPlayStateTo(PlayState.Stop);
            // }
            // else {
            //     this.PlayBackButton_Play.IsEnabled = false;
            //     this.PlayBackButton_Pause.IsEnabled = false;
            //     this.PlayBackButton_Stop.IsEnabled = false;
            // }
        }

        private void OnEditorProjectChanged(VideoEditor editor, Project oldProject, Project newProject) {
            DataManager.SetContextData(this, this.contextData.Set(DataKeys.ProjectKey, newProject).Clone());
            this.OnProjectChanged(oldProject, newProject);
        }

        private void OnProjectChanged(Project oldProject, Project newProject) {
            if (oldProject != null) {
                oldProject.ActiveTimelineChanged -= this.OnActiveTimelineChanged;
                oldProject.ProjectFilePathChanged -= this.OnProjectFilePathChanged;
                oldProject.ProjectNameChanged -= this.OnProjectNameChanged;
                oldProject.IsModifiedChanged -= this.OnProjectModifiedChanged;
            }

            if (newProject != null) {
                newProject.ActiveTimelineChanged += this.OnActiveTimelineChanged;
                newProject.ProjectFilePathChanged += this.OnProjectFilePathChanged;
                newProject.ProjectNameChanged += this.OnProjectNameChanged;
                newProject.IsModifiedChanged += this.OnProjectModifiedChanged;
            }

            this.UpdateResourceManager(newProject?.ResourceManager);
            this.OnActiveTimelineChanged(oldProject?.ActiveTimeline, newProject?.ActiveTimeline);
            this.UpdateWindowTitle(newProject);
            this.UpdatePlayBackButtons(newProject?.Editor.Playback);

            IoC.Dispatcher.InvokeAsync(() => {
                this.VPViewBox.FitContentToCenter();
            }, DispatcherPriority.Background);
        }

        private void OnActiveTimelineChanged(Project project, Timeline oldTimeline, Timeline newTimeline) {
            this.OnActiveTimelineChanged(oldTimeline, newTimeline);
        }

        private void PART_CloseTimelineButton_OnClick(object sender, RoutedEventArgs e) {
            if (this.TheTimeline.Timeline is CompositionTimeline timeline && timeline.Project != null) {
                timeline.Project.ActiveTimeline = null;
            }
        }

        private void OnActiveTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            if (oldTimeline != null) {
                oldTimeline.RenderManager.FrameRendered -= this.UpdateFrameRenderInterval;
                if (oldTimeline is CompositionTimeline oldComposition) {
                    oldComposition.Resource.DisplayNameChanged -= this.OnCompositionTimelineDisplayNameChanged;
                }
            }

            if (newTimeline != null) {
                newTimeline.RenderManager.FrameRendered += this.UpdateFrameRenderInterval;
                if (newTimeline is CompositionTimeline newComposition) {
                    newComposition.Resource.DisplayNameChanged += this.OnCompositionTimelineDisplayNameChanged;
                }
            }

            this.TheTimeline.Timeline = newTimeline;
            this.UpdateTimelineName();

            if (newTimeline is Timeline timeline) {
                this.PART_CloseTimelineButton.IsEnabled = timeline is CompositionTimeline;
            }
            else {
                this.PART_CloseTimelineButton.IsEnabled = false;
            }

            if (this.Editor is VideoEditor editor)
                this.UpdatePlayBackButtons(editor.Playback);
        }

        private void OnCompositionTimelineDisplayNameChanged(IDisplayName sender, string oldName, string newName) {
            this.UpdateTimelineName();
        }

        private void UpdateTimelineName() {
            Timeline timeline = this.TheTimeline.Timeline;
            if (timeline == null) {
                this.PART_TimelineName.Text = "No timeline loaded";
            }
            else if (timeline is CompositionTimeline composition) {
                this.PART_TimelineName.Text = composition.Resource.DisplayName;
            }
            else {
                this.PART_TimelineName.Text = "Project Timeline";
            }
        }

        private void OnProjectModifiedChanged(Project project) {
            this.UpdateWindowTitle(project);
        }

        private void OnProjectFilePathChanged(Project project) {
            this.UpdateWindowTitle(project);
        }

        private void OnProjectNameChanged(Project project) {
            this.UpdateWindowTitle(project);
        }

        private void UpdateWindowTitle(Project project) {
            const string DefaultTitle = "Bootleg song vegas (FramePFX v1.0.2)";
            if (project == null) {
                this.Title = DefaultTitle;
            }
            else {
                StringBuilder sb = new StringBuilder().Append(DefaultTitle);
                if (!string.IsNullOrEmpty(project.ProjectFilePath)) {
                    sb.Append(" - ").Append(project.ProjectFilePath);
                }

                if (!string.IsNullOrWhiteSpace(project.ProjectName)) {
                    sb.Append(" [").Append(project.ProjectName).Append("]");
                }

                if (project.IsModified)
                    sb.Append("*");

                this.Title = sb.ToString();
            }
        }

        private void UpdateResourceManager(ResourceManager manager) {
            this.TheResourcePanel.ResourceManager = manager;
        }

        private void OnFitToContentClicked(object sender, RoutedEventArgs e) {
            this.VPViewBox.FitContentToCenter();
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

        private void EditProjectSettings_Click(object sender, RoutedEventArgs e) {
            VideoEditor editor = this.Editor;
            Project project = editor?.Project;
            if (project == null) {
                return;
            }

            Rational oldFps = project.Settings.FrameRate;

            ProjectPropertiesDialog.ShowNewDialog(project);

            Rational newFps = project.Settings.FrameRate;
            if (oldFps == newFps) {
                return;
            }

            MessageBoxResult convertFrameRateResult = IoC.MessageService.ShowMessage("Convert Project", "Do you want to convert the project to suit the new frame rate? (resize clips and automation, etc.)", MessageBoxButton.YesNo);
            if (convertFrameRateResult == MessageBoxResult.Yes) {
                double ratio = newFps.AsDouble / oldFps.AsDouble;
                AutomationEngine.ConvertProjectFrameRate(project, ratio);

                double amount = 1.0 / ratio;
                project.ActiveTimeline.SetZoom(amount, ZoomType.Direct);
                project.ActiveTimeline.PlayHeadPosition = (long) Math.Round(project.ActiveTimeline.PlayHeadPosition * ratio);
            }
        }
    }
}