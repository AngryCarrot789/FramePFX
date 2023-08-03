using System;
using System.Threading.Tasks;
using FramePFX.Core.Editor.Exporting;
using FramePFX.Core.Editor.Notifications;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.History;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels {
    /// <summary>
    /// A view model that represents a video editor
    /// </summary>
    public class VideoEditorViewModel : BaseViewModel, IDisposable {
        private ProjectViewModel activeProject;
        private bool isClosingProject;
        private bool isRecordingKeyFrames;

        private bool isExporting;
        public bool IsExporting {
            get => this.isExporting;
            set => this.RaisePropertyChanged(ref this.isExporting, value);
        }

        /// <summary>
        /// The project that is currently being edited in this editor. May be null if no project is loaded
        /// </summary>
        public ProjectViewModel ActiveProject {
            get => this.activeProject;
            private set {
                if (ReferenceEquals(this.activeProject, value))
                    return;
                if (this.activeProject != null) {
                    this.activeProject.Editor = null;
                }

                if (value != null) {
                    this.Model.ActiveProject = value.Model;
                    value.Editor = this; // this also sets the project model's editor
                }
                else {
                    this.Model.ActiveProject = null;
                }

                this.RaisePropertyChanged(ref this.activeProject, value);
                this.ExportCommand.RaiseCanExecuteChanged();
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

        /// <summary>
        /// Whether or not to add new key frames when a parameter is modified during playback. Default is false
        /// </summary>
        public bool IsRecordingKeyFrames {
            get => this.isRecordingKeyFrames;
            set => this.RaisePropertyChanged(ref this.isRecordingKeyFrames, value);
        }

        private bool areAutomationShortcutsEnabled;
        public bool AreAutomationShortcutsEnabled {
            get => this.areAutomationShortcutsEnabled;
            set => this.RaisePropertyChanged(ref this.areAutomationShortcutsEnabled, value);
        }

        public EditorPlaybackViewModel Playback { get; }

        public VideoEditor Model { get; }

        public IVideoEditor View { get; }

        public AsyncRelayCommand NewProjectCommand { get; }

        public HistoryManagerViewModel HistoryManager { get; }

        public AsyncRelayCommand OpenProjectCommand { get; }

        public AsyncRelayCommand ExportCommand { get; }

        private SavingProjectNotification notification;

        public VideoEditorViewModel(IVideoEditor view) {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.Model = new VideoEditor();
            this.HistoryManager = new HistoryManagerViewModel(view.NotificationPanel, new HistoryManager());
            this.Playback = new EditorPlaybackViewModel(this);
            this.Playback.ProjectModified += this.OnProjectModified;
            this.Playback.Model.OnStepFrame = () => this.ActiveProject?.Timeline.OnStepFrameCallback();
            this.NewProjectCommand = new AsyncRelayCommand(this.NewProjectAction);
            this.OpenProjectCommand = new AsyncRelayCommand(this.OpenProjectAction);
            this.ExportCommand = new AsyncRelayCommand(this.ExportAction, () => this.ActiveProject != null);
        }

        public async Task ExportAction() {
            if (this.ActiveProject == null) {
                return;
            }

            long begin = 0L, endIndex = 0L;
            this.ActiveProject.Timeline.Model.GetUsedFrameSpan(ref begin, ref endIndex);
            ExportSetupViewModel setup = new ExportSetupViewModel(this.ActiveProject.Model) {
                RenderSpan = new FrameSpan(0, endIndex)
            };

            this.IsExporting = true;
            try {
                await IoC.Provide<IExportViewService>().ShowExportDialogAsync(setup);
            }
            finally {
                this.IsExporting = false;
            }

            await this.ActiveProject.Timeline.DoRenderAsync(false);
        }

        private void OnProjectModified(object sender, string property) {
            this.activeProject?.OnProjectModified(sender, property);
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

            await this.SetProject(project, true);
            this.ActiveProject.SetHasUnsavedChanges(false);
        }

        public async Task OpenProjectAction() {
            string[] result = await IoC.FilePicker.OpenFiles(Filters.ProjectTypeAndAllFiles, null, "Select a project file to open");
            if (result == null) {
                return;
            }

            if (this.ActiveProject != null && !await this.PromptSaveProjectAction()) {
                return;
            }

            #if DEBUG
            RBEBase rbe = RBEUtils.ReadFromFilePacked(result[0]);
            RBEDictionary dictionary = (RBEDictionary) rbe;
            Project project = new Project();
            project.ReadFromRBE(dictionary);
            ProjectViewModel pvm = new ProjectViewModel(project);
            #else
            RBEDictionary dictionary;
            try {
                dictionary = RBEUtils.ReadFromFilePacked(result[0]) as RBEDictionary;
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Read error", "Failed to read project from file", e.GetToString());
                return;
            }
            if (dictionary == null) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid project", "The project contains invalid data (non RBEDictionary)");
                return;
            }
            Project projectModel = new Project();
            ProjectViewModel pvm;
            try {
                projectModel.ReadFromRBE(dictionary);
                pvm = new ProjectViewModel(projectModel);
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Project load error", "Failed to load project", e.GetToString());
                return;
            }
            #endif

            if (this.ActiveProject != null) {
                try {
                    await this.CloseProjectAction();
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception", "Failed to close previous project. This error can be ignored", e.GetToString());
                }

                this.ActiveProject = null;
            }

            await this.SetProject(pvm, true);
            this.ActiveProject.SetHasUnsavedChanges(false);
            pvm.HasSavedOnce = true;
        }

        public async Task SetProject(ProjectViewModel project, bool loadResources = false) {
            await this.HistoryManager.ResetAsync();
            await this.Playback.OnProjectChanging(project);
            this.ActiveProject = project;
            await this.Playback.OnProjectChanged(project);

            if (loadResources) {
                if (project == null) {
                    throw new Exception("Cannot load resources for null project");
                }

                if (!await ResourceCheckerViewModel.LoadProjectResources(project, true)) {
                    #if !DEBUG
                    project.Dispose();
                    #else
                    try {
                        project.Dispose();
                    }
                    catch (Exception e) {
                        await IoC.MessageDialogs.ShowMessageExAsync("Failed to close project", "...", e.GetToString());
                    }
                    #endif

                    await this.Playback.OnProjectChanging(null);
                    this.ActiveProject = null;
                    await this.Playback.OnProjectChanged(null);
                    return;
                }
            }
        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack("Exception occurred while disposing video editor")) {
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

            bool? result = await IoC.MessageDialogs.ShowYesNoCancelDialogAsync("Save project", "Do you want to save the current project first?");
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
                await IoC.MessageDialogs.ShowMessageExAsync("Close Project", "An exception occurred while closing project", e.GetToString());
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
                this.notification.OnSaveFailed(e.GetToString());
            }

            this.notification.Timeout = TimeSpan.FromSeconds(5);
            this.notification.StartAutoHideTask();
            this.notification = null;
        }

        public Task DoRenderFrame() => this.DoRenderFrame(false);

        public Task DoRenderFrame(bool schedule) {
            return this.View.Render(schedule);
        }
    }
}