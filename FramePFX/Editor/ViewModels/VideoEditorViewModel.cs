using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.Exporting;
using FramePFX.Editor.Notifications;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.FileBrowser;
using FramePFX.History.ViewModels;
using FramePFX.Notifications.Types;
using FramePFX.PropertyEditing;
using FramePFX.RBC;
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
        private TimelineViewModel activeTimeline;

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
        public TimelineViewModel ActiveTimeline {
            get => this.activeTimeline;
            set => this.SetActiveTimeline(value);
        }

        private void SetActiveTimeline(TimelineViewModel timeline) {
            if (timeline == this.activeTimeline)
                return;
            if (!this.activeTimelines.Contains(timeline))
                this.activeTimelines.Add(timeline);
            if (this.activeTimeline != null) {
                this.Playback.StopPlaybackForChangingTimeline();
            }

            this.RaisePropertyChanged(ref this.activeTimeline, timeline);
            if (timeline != null) {
                PFXPropertyEditorRegistry.Instance.OnClipSelectionChanged(timeline.GetSelectedClips().ToList());
                PFXPropertyEditorRegistry.Instance.OnTrackSelectionChanged(timeline.SelectedTracks.ToList());
                PFXPropertyEditorRegistry.Instance.Root.CleanSeparators();
                timeline.RefreshAutomationAndPlayhead();
                this.DoDrawRenderFrame(timeline, true);
            }
        }

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

        public VideoEditor Model { get; }

        public IVideoEditor View { get; }

        public AsyncRelayCommand NewProjectCommand { get; }

        public AsyncRelayCommand OpenProjectCommand { get; }

        public AsyncRelayCommand ExportCommand { get; }

        private SavingProjectNotification notification;

        public VideoEditorViewModel(IVideoEditor view) {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.Model = new VideoEditor();
            this.activeTimelines = new ObservableCollection<TimelineViewModel>();
            this.ActiveTimelines = new ReadOnlyObservableCollection<TimelineViewModel>(this.activeTimelines);
            this.Playback = new EditorPlaybackViewModel(this);
            this.Playback.ProjectModified += this.OnProjectModified;
            this.Playback.Model.OnStepFrame = () => this.ActiveTimeline?.OnStepFrameCallback();
            this.NewProjectCommand = new AsyncRelayCommand(this.NewProjectAction);
            this.OpenProjectCommand = new AsyncRelayCommand(this.OpenProjectAction);
            this.ExportCommand = new AsyncRelayCommand(this.ExportAction, () => this.ActiveProject != null);
            this.FileExplorer = new FileExplorerViewModel();
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
                RenderSpan = span.WithBegin(0)
            };

            this.IsEditorEnabled = false;
            try {
                await Services.GetService<IExportViewService>().ShowExportDialogAsync(setup);
            }
            finally {
                this.IsEditorEnabled = true;
            }

            await timeline.DoAutomationTickAndRenderToPlayback(false);
        }

        private void OnProjectModified(object sender, string property) {
            this.activeProject?.OnProjectModified();
        }

        private async Task NewProjectAction() {
            if (this.ActiveProject != null && !await this.PromptSaveProjectAction()) {
                return;
            }

            ProjectViewModel project = new ProjectViewModel(new Project());
            project.Settings.Resolution = new Resolution(1280, 720);
            if (this.ActiveProject != null) {
                await this.CloseProjectAction();
            }

            await this.SetProject(project);
            this.ActiveProject.SetHasUnsavedChanges(false);
        }

        public async Task OpenProjectAction() {
            string[] result = await Services.FilePicker.OpenFiles(Filters.ProjectTypeAndAllFiles, null, "Select a project file to open");
            if (result == null) {
                return;
            }

            if (this.ActiveProject != null && !await this.PromptSaveProjectAction()) {
                return;
            }

            string path = result[0];
            string parentFolder;
            try {
                parentFolder = Path.GetDirectoryName(path);
            }
            catch (ArgumentException) {
                await Services.DialogService.ShowMessageAsync("Invalid file", "The project file contains invalid characters");
                return;
            }

            RBEDictionary dictionary;

#if DEBUG
            dictionary = RBEUtils.ReadFromFilePacked(path) as RBEDictionary;
#else
            try {
                dictionary = RBEUtils.ReadFromFilePacked(path) as RBEDictionary;
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Read error", "Failed to read project from file", e.GetToString());
                return;
            }
#endif

            if (dictionary == null) {
                await Services.DialogService.ShowMessageAsync("Invalid project", "The project contains invalid data (non RBEDictionary)");
                return;
            }

            Project projectModel = new Project();
            ProjectViewModel pvm;

            try {
                projectModel.ReadFromRBE(dictionary, parentFolder);
                pvm = new ProjectViewModel(projectModel);
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Project load error", "Failed to load project", e.GetToString());
                return;
            }

            if (this.ActiveProject != null) {
                try {
                    await this.CloseProjectAction();
                }
                catch (Exception e) {
                    await Services.DialogService.ShowMessageExAsync("Exception", "Failed to close previous project. This error can be ignored", e.GetToString());
                }
            }

            await this.SetProject(pvm);
            pvm.HasSavedOnce = true;

            ResourceCheckerViewModel checker = new ResourceCheckerViewModel() {
                Caption = "Project contains resources that could not be loaded (e.g. missing files)"
            };

            if (!await ResourceCheckerViewModel.LoadProjectResources(checker, pvm, true)) {
                await this.SetProject(null);
                return;
            }

            this.ActiveProject.SetHasUnsavedChanges(false);
        }

        public async Task SetProject(ProjectViewModel project) {
            if (ReferenceEquals(this.activeProject, project)) {
                return;
            }

            this.Model.IsProjectChanging = true;
            PFXPropertyEditorRegistry.Instance.Root.ClearHierarchyState();
            await this.Playback.OnProjectChanging(project);
            if (this.activeProject != null) {
                this.activeTimeline = null;
                this.RaisePropertyChanged(nameof(this.ActiveTimeline));
                this.Model.ActiveTimeline = null;
                this.activeTimelines.Clear();
                this.Model.SetProject(null);
                this.activeProject.OnDisconnectFromEditor();
                try {
                    this.activeProject.Dispose();
                }
                catch (Exception e) {
                    AppLogger.WriteLine("Exception while disposing project: " + e.GetToString());
                    this.View.NotificationPanel.PushNotification(new MessageNotification("Error", "An error occurred while unloading project. See logs for more info"));
                }
            }

            this.activeProject = project;
            if (project != null) {
                this.Model.SetProject(project.Model);
                this.activeProject.OnConnectToEditor(this);
                this.activeTimelines.Add(project.Timeline);
                this.activeTimeline = project.Timeline;
                this.RaisePropertyChanged(nameof(this.ActiveTimeline));
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

            if (!await this.PromptSaveProjectAction()) {
                return false;
            }

            this.IsClosingProject = true;
            await this.CloseProjectAction();
            this.IsClosingProject = false;
            return true;
        }

        public async Task<bool> PromptSaveProjectAction() {
            if (this.ActiveProject == null) {
                throw new Exception("No active project");
            }

            bool? result = await Services.DialogService.ShowYesNoCancelDialogAsync("Save project", "Do you want to save the current project first?");
            if (result == true) {
                await this.ActiveProject.SaveActionAsync();
            }
            else if (result == null) {
                return false;
            }

            return true;
        }

        public async Task CloseProjectAction() {
            if (this.ActiveProject == null) {
                throw new Exception("No active project");
            }

            try {
                this.ActiveProject.Dispose();
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Close Project", "An exception occurred while closing project", e.GetToString());
            }

            await this.SetProject(null);
        }

        public async Task OnProjectSaving() {
            this.IsProjectSaving = true;
            await this.Playback.OnProjectSaving();
            this.notification = new SavingProjectNotification();
            this.View.NotificationPanel.PushNotification(this.notification, false);
            this.notification.BeginSave();
        }

        public async Task OnProjectSaved(Exception e) {
            this.IsProjectSaving = false;
            await this.Playback.OnProjectSaved(e == null);
            if (e == null) {
                this.notification.OnSaveComplete();
            }
            else {
                this.notification.OnSaveFailed(e.Message + ". See logs for more info");
                AppLogger.WriteLine(e.GetToString());
            }

            this.notification.Timeout = TimeSpan.FromSeconds(5);
            this.notification.StartAutoHideTask();
            this.notification = null;
        }

        public Task DoDrawRenderFrame(TimelineViewModel timeline, bool schedule = false) {
            return this.View.RenderTimelineAsync(timeline, schedule);
        }
    }
}