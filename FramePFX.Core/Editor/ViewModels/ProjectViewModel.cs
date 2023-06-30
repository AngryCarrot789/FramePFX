using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ViewModels {
    public class ProjectViewModel : BaseViewModel, IDisposable {
        private string tempDir;
        private string projectDir;
        private bool hasUnsavedChanges;

        public bool HasSavedOnce { get; set; }

        public bool HasUnsavedChanges {
            get => this.hasUnsavedChanges;
            private set => this.RaisePropertyChanged(ref this.hasUnsavedChanges, value);
        }

        public ProjectSettingsViewModel Settings { get; }

        public Project Model { get; }

        public TimelineViewModel Timeline { get; }

        public ResourceManagerViewModel ResourceManager { get; }

        public AutomationEngineViewModel AutomationEngine { get; }

        /// <summary>
        /// The path of the project file, which links all of the saved data the project references
        /// </summary>
        public string ProjectDirectory {
            get => this.projectDir;
            set => this.RaisePropertyChanged(ref this.projectDir, value);
        }

        private VideoEditorViewModel editor;
        public VideoEditorViewModel Editor {
            get => this.editor;
            set {
                this.Model.Editor = value?.Model;
                this.RaisePropertyChanged(ref this.editor, value);
            }
        }

        public AsyncRelayCommand SaveCommand { get; }

        public AsyncRelayCommand SaveAsCommand { get; }

        public AsyncRelayCommand OpenSettingsCommand { get; }

        public ProjectViewModel(Project project) {
            this.Model = project ?? throw new ArgumentNullException(nameof(project));
            this.Settings = new ProjectSettingsViewModel(project.Settings);
            this.Settings.ProjectModified += this.OnProjectModified;
            this.ResourceManager = new ResourceManagerViewModel(this, project.ResourceManager);
            this.Timeline = new TimelineViewModel(this, project.Timeline);
            this.Timeline.ProjectModified += this.OnProjectModified;
            this.AutomationEngine = new AutomationEngineViewModel(this, project.AutomationEngine);

            this.SaveCommand = new AsyncRelayCommand(this.SaveActionAsync, () => this.Editor != null && !this.Model.IsSaving);
            this.SaveAsCommand = new AsyncRelayCommand(this.SaveAsActionAsync, () => this.Editor != null && !this.Model.IsSaving);
            this.OpenSettingsCommand = new AsyncRelayCommand(this.OpenSettingsAction);
            this.GetDirectory();
        }

        public DirectoryInfo GetDirectory() {
            string path = this.ProjectDirectory;
            if (string.IsNullOrEmpty(path)) {
                if (string.IsNullOrEmpty(this.tempDir)) {
                    this.tempDir = path = RandomUtils.RandomStringWhere(Path.GetTempPath(), 32, x => !Directory.Exists(x));
                    return Directory.CreateDirectory(path);
                }
                else {
                    path = this.tempDir;
                }
            }

            return Directory.Exists(path) ? new DirectoryInfo(path) : Directory.CreateDirectory(path);
        }

        public async Task OpenSettingsAction() {
            EditorPlaybackViewModel playback = this.Editor?.Playback;
            if (playback == null) {
                return;
            }

            if (playback.IsPlaying) {
                await playback.StopRenderTimer();
            }

            ProjectSettings result = await IoC.Provide<IProjectSettingsEditor>().EditSettingsAsync(this.Settings.Model);
            if (result != null) {
                this.Settings.Resolution = result.Resolution;
                this.Settings.FrameRate = result.FrameRate;
                playback.SetTimerFrameRate(result.FrameRate.AsDouble);
            }
        }

        public void OnProjectModified(object sender, string property) {
            this.SetHasUnsavedChanges(true);
        }

        public void SetHasUnsavedChanges(bool value) {
            if (this.HasUnsavedChanges != value) {
                this.HasUnsavedChanges = value;
            }
        }

        public async Task<bool> SaveActionAsync() {
            if (this.Editor == null || this.Model.IsSaving) {
                return false;
            }

            if (Directory.Exists(this.ProjectDirectory) || !string.IsNullOrEmpty(this.ProjectDirectory) && this.HasSavedOnce) {
                await this.SaveToFileAsync();
                return true;
            }
            else {
                return await this.SaveAsActionAsync();
            }
        }

        public async Task<bool> SaveAsActionAsync() {
            if (this.Editor == null || this.Model.IsSaving) {
                return false;
            }

            DialogResult<string> result = IoC.FilePicker.OpenFolder(Path.GetDirectoryName(this.ProjectDirectory), "Select a folder, in which the project data will be saved into");
            if (result.IsSuccess) {
                this.ProjectDirectory = result.Value;
                await this.SaveToFileAsync();
                return true;
            }
            else {
                return false;
            }
        }

        public async Task SaveToFileAsync() {
            if (this.Editor == null)
                return;
            if (!ReferenceEquals(this, this.Editor.ActiveProject))
                throw new Exception("The editor's project does not match the current instance");

            if (string.IsNullOrEmpty(this.ProjectDirectory)) {
                throw new Exception("Project dir cannot be null or empty");
            }

            this.Model.IsSaving = true;

            bool reloadProject = false;

            try {
                DirectoryInfo dir = this.GetDirectory();
                if (this.tempDir != null && Directory.Exists(this.tempDir)) {
                    await this.ResourceManager.OfflineAllAsync();
                    foreach (string path in Directory.EnumerateFileSystemEntries(this.tempDir)) {
                        string fileName = Path.GetFileName(path);
                        Directory.Move(path, Path.Combine(dir.FullName, fileName));
                    }

                    reloadProject = true;
                }
            }
            catch (PathTooLongException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("PathTooLongException", "Could not create project directory", ex.GetToString());
            }
            catch (SecurityException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("SecurityException", "Could not create project directory", ex.GetToString());
            }
            catch (UnauthorizedAccessException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("UnauthorizedAccessException", "Could not create project directory", ex.GetToString());
            }
            catch (Exception ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Unexpected error", "Could not create project directory", ex.GetToString());
            }

            this.tempDir = null;
            await this.Editor.OnProjectSaving();

            Exception e = null;
            RBEDictionary projData = new RBEDictionary();
            #if DEBUG
            this.Model.WriteToRBE(projData);
            #else
            try {
                this.Model.WriteToRBE(projData);
            }
            catch (Exception exception) {
                e = new Exception("Failed to serialise project", exception);
            }
            #endif

            string projectFile = Path.Combine(this.ProjectDirectory, "Project" + Filters.FrameFPXExtensionDot);
            #if DEBUG
            RBEUtils.WriteToFilePacked(projData, projectFile);
            #else
            if (e == null) {
                try {
                    RBEUtils.WriteToFilePacked(projData, projectFile);
                }
                catch (Exception exception) {
                    e = new Exception("Failed to write project to the disk", exception);
                }
            }
            #endif

            this.Model.IsSaving = false;
            await this.Editor.OnProjectSaved(e == null);
            if (e == null) {
                this.HasSavedOnce = true;
                this.SetHasUnsavedChanges(false);
            }
            else {
                await IoC.MessageDialogs.ShowMessageExAsync("Error saving", "An exception occurred while saving project", e.GetToString());
            }

            if (reloadProject) {
                await ResourceCheckerViewModel.LoadProjectResources(this, false);
            }
        }

        public void Dispose() {
            using (ExceptionStack stack1 = new ExceptionStack()) {
                try {
                    this.Timeline.Dispose();
                }
                catch (Exception e) {
                    stack1.Add(e);
                }

                try {
                    this.ResourceManager.Dispose();
                }
                catch (Exception e) {
                    stack1.Add(e);
                }
            }
        }
    }
}