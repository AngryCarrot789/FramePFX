using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Commands;
using FramePFX.Editor.Exporting;
using FramePFX.Editor.Notifications;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.FileBrowser;
using FramePFX.History.ViewModels;
using FramePFX.Logger;
using FramePFX.Notifications;
using FramePFX.Notifications.Types;
using FramePFX.PropertyEditing;
using FramePFX.RBC;
using FramePFX.TaskSystem;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels {
    /// <summary>
    /// A view model that represents a video editor
    /// </summary>
    public class VideoEditorViewModel : BaseViewModel, IDisposable {
        private readonly ObservableCollection<TimelineViewModel> activeTimelines;
        private ProjectViewModel activeProject;
        private bool isClosingProject;
        private bool isEditorEnabled;
        private bool areAutomationShortcutsEnabled;

        public bool IsEditorEnabled {
            get => this.isEditorEnabled;
            set {
                if (this.isEditorEnabled == value)
                    return;
                this.RaisePropertyChanged(ref this.isEditorEnabled, value);
            }
        }

        /// <summary>
        /// The project that is currently being edited in this editor. May be null if no project is loaded
        /// </summary>
        public ProjectViewModel ActiveProject => this.activeProject;

        /// <summary>
        /// A collection of timelines currently active in the UI
        /// </summary>
        public ReadOnlyObservableCollection<TimelineViewModel> ActiveTimelines { get; }

        /// <summary>
        /// Gets or sets the timeline that is currently active in the UI.
        /// Updating this may cause a render to be triggered asynchronously
        /// </summary>
        public TimelineViewModel SelectedTimeline { get; private set; }

        public bool IsProjectSaving {
            get => this.Model.IsProjectSaving;
            private set {
                this.Model.IsProjectSaving = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsClosingProject {
            get => this.isClosingProject;
            set => this.RaisePropertyChanged(ref this.isClosingProject, value);
        }

        public bool AreAutomationShortcutsEnabled {
            get => this.areAutomationShortcutsEnabled;
            set => this.RaisePropertyChanged(ref this.areAutomationShortcutsEnabled, value);
        }

        public EditorPlaybackViewModel Playback { get; }

        public FileExplorerViewModel FileExplorer { get; }

        public EffectProviderListViewModel EffectsProviderList { get; }

        public VideoEditor Model { get; }

        public IVideoEditor View { get; }

        public AsyncRelayCommand NewProjectCommand { get; }

        public AsyncRelayCommand OpenProjectCommand { get; }

        public AsyncRelayCommand ExportCommand { get; }

        private SavingProjectNotification saveNotification;

        private readonly Dictionary<TaskAction, TaskNotification> taskNotifications;
        private readonly Dictionary<TimelineViewModel, Task> timelineRenderTaskMap;

        public VideoEditorViewModel(IVideoEditor view) {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.Model = new VideoEditor();
            this.activeTimelines = new ObservableCollection<TimelineViewModel>();
            this.ActiveTimelines = new ReadOnlyObservableCollection<TimelineViewModel>(this.activeTimelines);
            this.timelineRenderTaskMap = new Dictionary<TimelineViewModel, Task>();
            this.Playback = new EditorPlaybackViewModel(this);
            this.Playback.ProjectModified += this.OnProjectModified;
            this.Playback.Model.OnStepFrame = () => this.SelectedTimeline?.OnStepFrameCallback();
            this.NewProjectCommand = new AsyncRelayCommand(this.NewProjectAction);
            this.OpenProjectCommand = new AsyncRelayCommand(this.OpenProjectAction);
            this.ExportCommand = new AsyncRelayCommand(this.ExportAction, () => this.ActiveProject != null);
            this.FileExplorer = new FileExplorerViewModel();
            this.EffectsProviderList = new EffectProviderListViewModel();
            this.taskNotifications = new Dictionary<TaskAction, TaskNotification>();
            TaskManager.Instance.TaskStarted += this.OnTaskStarted;
            TaskManager.Instance.TaskFinished += this.OnTaskFinished;
        }

        private void OnTaskStarted(TaskManager manager, TaskAction task) {
            this.taskNotifications[task] = new TaskNotification(task);
            this.View.NotificationPanel.PushNotification(this.taskNotifications[task]);
        }

        private void OnTaskFinished(TaskManager manager, TaskAction task) {
            if (this.taskNotifications.TryGetValue(task, out TaskNotification n)) {
                n.StartAutoHideTask(TimeSpan.FromSeconds(3));
            }
        }

        public static void OnSelectedTimelineChangedInternal(VideoEditorViewModel editor, TimelineViewModel timeline, bool? scheduleRender = true) {
            if (timeline == null) {
                timeline = editor.ActiveProject?.Timeline;
            }

            editor.SelectedTimeline = timeline;
            editor.RaisePropertyChanged(nameof(editor.SelectedTimeline));

            if (timeline != null) {
                editor.Model.ActiveTimeline = timeline.Model;
                PFXPropertyEditorRegistry.Instance.OnClipSelectionChanged(timeline.GetSelectedClips().ToList());
                PFXPropertyEditorRegistry.Instance.OnTrackSelectionChanged(timeline.SelectedTracks.ToList());
                PFXPropertyEditorRegistry.Instance.Root.CleanSeparators();
                timeline.RefreshAutomationAndPlayhead();
                if (scheduleRender.HasValue) {
                    editor.DoDrawRenderFrame(timeline.Model, scheduleRender.Value);
                }
            }
            else {
                editor.Model.ActiveTimeline = null;
                PFXPropertyEditorRegistry.Instance.ClipInfo.ClearHierarchyState();
                PFXPropertyEditorRegistry.Instance.TrackInfo.ClearHierarchyState();
            }
        }

        public void OnTimelineClosed(TimelineViewModel timeline) {
            this.activeTimelines.Remove(timeline);
            if (this.SelectedTimeline == timeline) {
                OnSelectedTimelineChangedInternal(this, null);
            }
        }

        public void OnTimelineOpened(TimelineViewModel timeline) {
            if (!this.activeTimelines.Contains(timeline)) {
                this.activeTimelines.Add(timeline);
            }
        }

        public void OnTimelinesCleared() {
            this.activeTimelines.Clear();
            OnSelectedTimelineChangedInternal(this, null, null);
        }

        public void OpenAndSelectTimeline(TimelineViewModel timeline) {
            if (this.SelectedTimeline != null) {
                this.Playback.StopPlaybackForChangingTimeline();
            }

            if (timeline != null || (timeline = this.ActiveProject?.Timeline) != null) {
                this.View.OpenAndSelectTimeline(timeline);
            }

            if (this.SelectedTimeline != timeline) {
                OnSelectedTimelineChangedInternal(this, timeline);
            }
        }

        public async Task ExportAction() {
            if (this.ActiveProject == null) {
                return;
            }

            if (this.Playback.IsPlaying) {
                await this.Playback.PauseAction();
            }

            TimelineViewModel timeline = this.ActiveProject.Timeline;
            FrameSpan span = timeline.Model.GetUsedFrameSpan();
            ExportSetupViewModel setup = new ExportSetupViewModel(this.ActiveProject) {
                RenderSpan = span
            };

            this.IsEditorEnabled = false;
            try {
                await IoC.GetService<IExportViewService>().ShowExportDialogAsync(setup);
            }
            finally {
                this.IsEditorEnabled = true;
            }

            await timeline.UpdateAndRenderTimelineToEditor(false);
        }

        private void OnProjectModified(object sender, string property) {
            this.activeProject?.OnProjectModified();
        }

        private async Task NewProjectAction() {
            if (this.ActiveProject != null && !await this.PromptAndSaveProjectAction()) {
                return;
            }

            ProjectViewModel project = new ProjectViewModel(new Project());
            project.Settings.Resolution = new Rect2i(1280, 720);
            if (this.ActiveProject != null) {
                await this.CloseProjectAction();
            }

            await this.LoadProject(project);
            this.ActiveProject.SetHasUnsavedChanges(false);
        }

        public async Task OpenProjectAction() {
            string[] result = await IoC.FilePicker.OpenFiles(Filters.ProjectTypeAndAllFiles, null, "Select a project file to open");
            if (result != null && (this.ActiveProject == null || await this.PromptAndSaveProjectAction())) {
                await this.CloseProjectAction();
                await this.OpenProjectAtAction(result[0]);
                if (this.SelectedTimeline != null) {
                    await this.SelectedTimeline.UpdateAndRenderTimelineToEditor(false);
                }
            }
        }

        public async Task OpenProjectAtAction(string filePath, bool forceCloseProject = false) {
            if (this.ActiveProject != null) {
                if (forceCloseProject) {
                    await this.CloseProjectAction();
                }
                else {
                    throw new Exception("Another project is already open, it should be closed before opening another");
                }
            }

            AppLogger.WriteLine("Reading packed RBE project from file: " + filePath);
            RBEDictionary dictionary;
            try {
                dictionary = RBEUtils.ReadFromFilePacked(filePath) as RBEDictionary;
            }
            catch (Exception e) {
                AppLogger.WriteLine("Failed to read packed RBE data");
                AppLogger.WriteLine(e.GetToString());
                await IoC.DialogService.ShowMessageAsync("Read error", "Failed to read project from file. See logs for more info");
                return;
            }

            if (dictionary == null) {
                await IoC.DialogService.ShowMessageAsync("Invalid project", "The project contains invalid data (non RBEDictionary)");
                return;
            }

            Project projectModel = new Project();
            ProjectViewModel pvm;

            try {
                projectModel.ReadFromRBE(dictionary, filePath);
                pvm = new ProjectViewModel(projectModel);
            }
            catch (Exception e) {
                await IoC.DialogService.ShowMessageExAsync("Project load error", "Failed to load project", e.GetToString());
                return;
            }

            if (this.ActiveProject != null) {
                try {
                    await this.CloseProjectAction();
                }
                catch (Exception e) {
                    await IoC.DialogService.ShowMessageExAsync("Exception", "Failed to close previous project. This error can be ignored", e.GetToString());
                }
            }

            await this.LoadProject(pvm);
            pvm.HasSavedOnce = true;

            ResourceCheckerViewModel checker = new ResourceCheckerViewModel() {
                Caption = "Project contains resources that could not be loaded (e.g. missing files)"
            };

            if (!await ResourceCheckerViewModel.LoadProjectResources(checker, pvm, true)) {
                AppLogger.WriteLine("Project load cancelled due to invalid resources. Unloading newly loaded project...");
                await this.CloseProjectAction();
                return;
            }

            this.ActiveProject.SetHasUnsavedChanges(false);
            AppLogger.WriteLine("Loaded project at " + pvm.ProjectFilePath);
        }

        /// <summary>
        /// Unloads and disposes the previous project (if one is loaded), then loads the new given project (or does nothing else if it's null)
        /// </summary>
        /// <param name="project"></param>
        public async Task LoadProject(ProjectViewModel project) {
            if (ReferenceEquals(this.activeProject, project)) {
                return;
            }

            this.Model.IsProjectChanging = true;
            PFXPropertyEditorRegistry.Instance.Root.ClearHierarchyState();
            await this.Playback.OnProjectChanging(project);
            if (this.activeProject != null) {
                this.View.CloseAllTimelinesExcept(this.activeProject.Timeline);
                this.Model.ClearTimelines();
                this.activeTimelines.Clear();
                this.activeProject.OnDisconnectFromEditor();
                this.Model.SetProject(null);
                try {
                    if (this.activeProject.Model.IsLoaded) {
                        this.activeProject.Model.OnUnloaded();
                    }

                    this.activeProject.Dispose();
                }
                catch (Exception e) {
                    AppLogger.WriteLine("Exception while disposing project: " + e.GetToString());
                    this.View.NotificationPanel.PushNotification(new MessageNotification("Error", "An error occurred while unloading project. See logs for more info"));
                }
            }

            this.activeProject = project;
            if (project != null) {
                if (!project.Model.IsLoaded) {
                    project.Model.OnLoaded();
                }

                this.Model.SetProject(project.Model);
                this.activeProject.OnConnectToEditor(this);
                this.activeTimelines.Add(project.Timeline);
                OnSelectedTimelineChangedInternal(this, project.Timeline);
            }

            this.Model.IsProjectChanging = false;
            this.RaisePropertyChanged(nameof(this.ActiveProject));
            this.IsEditorEnabled = project != null;
            this.ExportCommand.RaiseCanExecuteChanged();
            await this.Playback.OnProjectChanged(project);
            await HistoryManagerViewModel.Instance.ResetAsync();
        }

        public void Dispose() {
            using (ErrorList stack = new ErrorList("Exception occurred while disposing video editor")) {
                if (this.ActiveProject != null) {
                    try {
                        this.ActiveProject.Dispose();
                    }
                    catch (Exception e) {
                        stack.Add(new Exception("Exception disposing active project", e));
                    }
                }

                try {
                    this.Playback.Dispose();
                }
                catch (Exception e) {
                    stack.Add(new Exception("Exception disposing playback", e));
                }
            }
        }

        /// <summary>
        /// Saves and closes the project
        /// </summary>
        /// <returns>True when the project was closed,</returns>
        public async Task<bool> PromptSaveAndCloseProjectAction() {
            if (this.ActiveProject == null) {
                throw new Exception("No active project");
            }

            if (!await this.PromptAndSaveProjectAction()) {
                return false;
            }

            await this.CloseProjectAction();
            return true;
        }

        public async Task<bool> PromptAndSaveProjectAction() {
            if (this.ActiveProject == null) {
                throw new Exception("No active project");
            }

            bool? result = await IoC.DialogService.ShowYesNoCancelDialogAsync("Save project", "Do you want to save the current project first?");
            if (result == true) {
                await this.ActiveProject.SaveActionAsync();
            }
            else if (result == null) {
                return false;
            }

            return true;
        }

        public async Task CloseProjectAction() {
            if (this.ActiveProject != null) {
                this.IsClosingProject = true;
                try {
                    await this.LoadProject(null);
                }
                finally {
                    this.IsClosingProject = false;
                }
            }
        }

        public async Task OnProjectSaving() {
            this.IsProjectSaving = true;
            await this.Playback.OnProjectSaving();
            this.saveNotification = new SavingProjectNotification();
            this.View.NotificationPanel.PushNotification(this.saveNotification, false);
            this.saveNotification.BeginSave();
        }

        public async Task OnProjectSaved(Exception e) {
            this.IsProjectSaving = false;
            await this.Playback.OnProjectSaved(e == null);
            if (e == null) {
                this.saveNotification.OnSaveComplete();
            }
            else {
                this.saveNotification.OnSaveFailed(e.Message + ". See logs for more info");
                AppLogger.WriteLine(e.GetToString());
            }

            this.saveNotification.Timeout = TimeSpan.FromSeconds(5);
            this.saveNotification.StartAutoHideTask();
            this.saveNotification = null;
        }

        public Task DoDrawRenderFrame(Timeline timeline, bool schedule = false) {
            return this.View.RenderToViewPortAsync(timeline, schedule);
        }

        public Task ScheduleUpdateAndRender(TimelineViewModel timeline, bool shouldScheduleRender = false) {
            if (this.timelineRenderTaskMap.TryGetValue(timeline, out Task task) && !task.IsCompleted) {
                return task;
            }

            task = IoC.Application.Dispatcher.InvokeAsync(() => this.UpdateAndRenderTimeline(timeline, shouldScheduleRender));
            this.timelineRenderTaskMap[timeline] = task;
            return task;
        }

        private async Task UpdateAndRenderTimeline(TimelineViewModel timeline, bool shouldScheduleRender) {
            if (this.SelectedTimeline != null && this.SelectedTimeline != timeline) {
                return;
            }

            AutomationEngine.UpdateTimeline(timeline.Model, timeline.PlayHeadFrame);
            try {
                await this.View.RenderToViewPortAsync(timeline.Model, shouldScheduleRender);
            }
            catch (TaskCanceledException) {
                // do nothing
            }

            timeline.RefreshAutomationAndPlayhead();
        }
    }

    public class TaskNotification : NotificationViewModel {
        public string Header {
            get => this.Task.Tracker.HeaderText;
            set => this.Task.Tracker.HeaderText = value;
        }

        public string Footer {
            get => this.Task.Tracker.FooterText;
            set => this.Task.Tracker.FooterText = value;
        }

        public TaskAction Task { get; }

        public TaskNotification(TaskAction task) {
            this.Task = task;
            task.Tracker.PropertyChanged += (sender, args) => {
                switch (args.PropertyName) {
                    case nameof(IProgressTracker.HeaderText):
                        this.RaisePropertyChanged(nameof(this.Header));
                        break;
                    case nameof(IProgressTracker.FooterText):
                        this.RaisePropertyChanged(nameof(this.Footer));
                        break;
                }
            };
        }
    }
}