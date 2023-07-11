using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels {
    public class ProjectViewModel : BaseViewModel, IDisposable {
        private bool hasUnsavedChanges;

        public bool HasSavedOnce { get; set; }

        public bool HasUnsavedChanges {
            get => this.hasUnsavedChanges;
            private set => this.RaisePropertyChanged(ref this.hasUnsavedChanges, value);
        }

        public bool IsSaving {
            get => this.Model.IsSaving;
            private set {
                this.Model.IsSaving = value;
                this.RaisePropertyChanged();
            }
        }

        public ProjectSettingsViewModel Settings { get; }

        public Project Model { get; }

        public TimelineViewModel Timeline { get; }

        public ResourceManagerViewModel ResourceManager { get; }

        public AutomationEngineViewModel AutomationEngine { get; }

        /// <summary>
        /// The path of the project file, which links all of the saved data the project references
        /// </summary>
        public string DataFolder {
            get => this.Model.DataFolder;
            private set {
                this.Model.DataFolder = value;
                this.RaisePropertyChanged();
            }
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

            this.AutomationEngine = new AutomationEngineViewModel(this, project.AutomationEngine);
            this.SaveCommand = new AsyncRelayCommand(this.SaveActionAsync, () => this.Editor != null && !this.IsSaving);
            this.SaveAsCommand = new AsyncRelayCommand(this.SaveAsActionAsync, () => this.Editor != null && !this.IsSaving);
            this.OpenSettingsCommand = new AsyncRelayCommand(this.OpenSettingsAction);
            this.Model.CreateDir();
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
                this.Settings.FrameRate = result.TimeBase;
                playback.SetTimerFrameRate(result.TimeBase);
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
            if (!string.IsNullOrEmpty(this.DataFolder) && (Directory.Exists(this.DataFolder) || this.HasSavedOnce)) {
                if (this.Editor == null)
                    return false;
                if (this.IsSaving) {
                    await IoC.MessageDialogs.ShowMessageAsync("Saving", "Project is already being saved");
                    return false;
                }

                return await this.SaveProjectData(this.DataFolder);
            }
            else {
                return await this.SaveAsActionAsync();
            }
        }

        public async Task<bool> SaveAsActionAsync() {
            if (this.Editor == null)
                return false;
            if (this.IsSaving) {
                await IoC.MessageDialogs.ShowMessageAsync("Saving", "Project is already being saved");
                return false;
            }

            string result = await IoC.FilePicker.OpenFolder(Path.GetDirectoryName(this.DataFolder), "Select a folder, in which the project data will be saved into");
            if (result != null && await this.SaveProjectData(result)) {
                this.DataFolder = result;
                return true;
            }

            return false;
        }

        // Use debug and release versions in order for the debugger to pickup exceptions or Debugger.Break() (for debugging of course)

        #if true // DEBUG

        private async Task<bool> SaveProjectData(string folder) {
            if (string.IsNullOrEmpty(folder)) {
                throw new Exception("Project dir cannot be null or empty");
            }

            DirectoryInfo dataFolder = null;
            try {
                dataFolder = Directory.CreateDirectory(folder);
            }
            catch (PathTooLongException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Path too long", "Path was too long. Could not create project data folder", ex.GetToString());
            }
            catch (SecurityException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Security Exception", "Application does not have permission to create directories", ex.GetToString());
            }
            catch (UnauthorizedAccessException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Unauthorized access", "Application does not have access to that directory", ex.GetToString());
            }
            catch (Exception ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Unexpected error", "Could not create project directory", ex.GetToString());
            }

            if (dataFolder == null) {
                return false;
            }

            // copy temp project files into actual project dir
            if (!string.IsNullOrEmpty(this.Model.TempDataFolder) && Directory.Exists(this.Model.TempDataFolder)) {
                await this.ResourceManager.OfflineAllAsync(false);
                using (IEnumerator<string> enumerator = Directory.EnumerateFileSystemEntries(this.Model.TempDataFolder).GetEnumerator()) {
                    while (true) {
                        try {
                            if (!enumerator.MoveNext()) {
                                break;
                            }
                        }
                        catch (Exception e) {
                            string str = e.GetToString();
                            AppLogger.WriteLine(str);
                            Debug.WriteLine(str);
                            continue;
                        }

                        string path = enumerator.Current;
                        if (string.IsNullOrEmpty(path))
                            continue;
                        string fileName = Path.GetFileName(path);
                        try {
                            Directory.Move(path, Path.Combine(dataFolder.FullName, fileName));
                        }
                        catch (Exception me) {
                            string str = me.GetToString();
                            AppLogger.WriteLine(str);
                            Debug.WriteLine(str);
                        }
                    }
                }

                try {
                    Directory.Delete(this.Model.TempDataFolder);
                }
                catch (Exception ex) {
                    string str = "Failed to delete temp data folder:\n" + ex.GetToString();
                    AppLogger.WriteLine(str);
                    Debug.WriteLine(str);
                }
                finally {
                    this.Model.TempDataFolder = null;
                }

                if (!await ResourceCheckerViewModel.LoadProjectResources(this, false)) {
                    return false;
                }
            }

            this.IsSaving = true;
            if (this.Editor != null) {
                await this.Editor.OnProjectSaving();
            }

            RBEDictionary dictionary = new RBEDictionary();
            this.Model.WriteToRBE(dictionary);

            string projectFile = Path.Combine(folder, "Project" + Filters.FrameFPXExtensionDot);
            RBEUtils.WriteToFilePacked(dictionary, projectFile);
            this.IsSaving = false;
            if (this.Editor != null) {
                await this.Editor.OnProjectSaved(null);
            }

            this.HasSavedOnce = true;
            this.SetHasUnsavedChanges(false);
            return true;
        }

        #else

        private async Task<bool> SaveProjectData(string folder) {
            if (string.IsNullOrEmpty(folder))
                throw new Exception("Project dir cannot be null or empty");

            this.Model.IsSaving = true;
            bool reloadResources = false;

            DirectoryInfo dataFolder = Directory.CreateDirectory(folder);

            try {
                // copy temp project files into actual project dir
                if (!string.IsNullOrEmpty(this.Model.TempDataFolder) && Directory.Exists(this.Model.TempDataFolder)) {
                    await this.ResourceManager.OfflineAllAsync(false);
                    Exception moveException = null;
                    foreach (string path in Directory.EnumerateFileSystemEntries(this.Model.TempDataFolder)) {
                        string fileName = Path.GetFileName(path);
                        try {
                            Directory.Move(path, Path.Combine(dataFolder.FullName, fileName));
                        }
                        catch (Exception me) {
                            (moveException ?? (moveException = new Exception("Exception moving 1 or more directories from temp directory to project directory"))).
                                AddSuppressed(me);
                        }
                    }

                    reloadResources = true;
                    try {
                        Directory.Delete(this.Model.TempDataFolder);
                    }
                    catch (Exception ex) {
                        Debug.WriteLine($"Failed to delete temp data folder: " + ex.GetToString());
                    }
                }

                this.Model.TempDataFolder = null;
            }
            catch (PathTooLongException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("PathTooLongException", "Could not create project directory", ex.GetToString());
                return false;
            }
            catch (SecurityException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("SecurityException", "Could not create project directory", ex.GetToString());
                return false;
            }
            catch (UnauthorizedAccessException ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("UnauthorizedAccessException", "Could not create project directory", ex.GetToString());
                return false;
            }
            catch (Exception ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Unexpected error", "Could not create project directory", ex.GetToString());
                return false;
            }

            if (this.Editor != null) {
                await this.Editor.OnProjectSaving();
            }

            Exception e = null;
            RBEDictionary projData = new RBEDictionary();
            this.Model.WriteToRBE(projData);

            string projectFile = Path.Combine(folder, "Project" + Filters.FrameFPXExtensionDot);
            RBEUtils.WriteToFilePacked(projData, projectFile);
            this.Model.IsSaving = false;
            if (this.Editor != null) {
                await this.Editor.OnProjectSaved(true);
            }

            this.HasSavedOnce = true;
            this.SetHasUnsavedChanges(false);

            if (reloadResources) {
                await ResourceCheckerViewModel.LoadProjectResources(this, false);
            }

            return true;
        }

        #endif

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