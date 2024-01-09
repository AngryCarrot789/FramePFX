using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Commands;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.Modal;

namespace FramePFX.Editor.ViewModels {
    public class ProjectViewModel : BaseViewModel {
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

        public TimelineViewModel Timeline { get; }

        public ResourceManagerViewModel ResourceManager { get; }

        public string ProjectName => this.Model.ProjectName;

        public string ProjectFolder => this.Model.ProjectFolder;

        public string ProjectFileName => this.Model.ProjectFileName;

        public string ProjectFilePath => this.Model.ProjectFilePath;

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

        public bool IsExporting {
            get => this.Model.IsExporting;
            private set {
                this.Model.IsExporting = value;
                this.RaisePropertyChanged();
            }
        }

        public Project Model { get; }

        public ProjectViewModel(Project project) {
            this.Model = project ?? throw new ArgumentNullException(nameof(project));
            this.Settings = new ProjectSettingsViewModel(project.Settings);
            this.Settings.ProjectModified += (sender, property) => this.OnProjectModified();
            this.ResourceManager = new ResourceManagerViewModel(this, project.ResourceManager);
            this.Timeline = new TimelineViewModel(project.Timeline) {
                DisplayName = "Project Timeline"
            };
            this.Timeline.SetProject(this);
            this.SaveCommand = new AsyncRelayCommand(this.SaveActionAsync, () => this.Editor != null && !this.IsSaving);
            this.SaveAsCommand = new AsyncRelayCommand(this.SaveAsActionAsync, () => this.Editor != null && !this.IsSaving);
            this.OpenSettingsCommand = new AsyncRelayCommand(this.OpenSettingsAction);
        }

        private static readonly MessageDialog ShouldCreateDataFolderDialog;

        static ProjectViewModel() {
            ShouldCreateDataFolderDialog = Dialogs.YesNoDialog.Clone();
            DialogButton yes = ShouldCreateDataFolderDialog.GetButtonById("yes");
            yes.Text = "New Folder";
            yes.ToolTip = "Creates a new folder, with the same name as the project file, which is where the project assets are stores";

            DialogButton no = ShouldCreateDataFolderDialog.GetButtonById("no");
            no.Text = "Current Folder";
            no.ToolTip = "Uses the parent folder (that the project file is stored in) as the data folder";

            DialogButton cancel = ShouldCreateDataFolderDialog.AddButton("Cancel", "cancel", false);
            cancel.ToolTip = "Cancel project save; no files will be created, deleted or moved";

            ShouldCreateDataFolderDialog.ShowAlwaysUseNextResultOption = true;
        }

        public void OnExportBegin() {
            this.IsExporting = true;
        }

        public void OnExportEnded() {
            this.IsExporting = false;
        }

        public async Task OpenSettingsAction() {
            EditorPlaybackViewModel playback = this.Editor?.Playback;
            if (playback == null) {
                return;
            }

            if (playback.IsPlaying) {
                await playback.StopRenderTimer();
            }

            ProjectSettings result = await IoC.GetService<IProjectSettingsEditor>().EditSettingsAsync(this.Settings.Model);
            if (result != null) {
                this.Settings.Resolution = result.Resolution;

                Rational oldFps = this.Settings.FrameRate;
                this.Settings.FrameRate = result.TimeBase;
                playback.SetTimerFrameRate(result.TimeBase);

                if (oldFps != this.Settings.FrameRate) {
                    if (await IoC.DialogService.ShowYesNoDialogAsync("Convert Framerate", "Do you want to convert clip and automation to match the new FPS?")) {
                        AutomationEngine.ConvertProjectFrameRate(this, oldFps, result.TimeBase);
                        if (this.editor?.SelectedTimeline != null) {
                            double ratio = result.TimeBase.ToDouble / oldFps.ToDouble;
                            this.Editor?.View?.OnFrameRateRatioChanged(this.editor.SelectedTimeline, ratio);
                        }
                    }
                }
            }
        }

        public void OnProjectModified() {
            this.SetHasUnsavedChanges(true);
        }

        public void SetHasUnsavedChanges(bool value) {
            if (this.HasUnsavedChanges != value) {
                this.HasUnsavedChanges = value;
            }
        }

        public async Task<bool> SaveActionAsync() {
            if (!this.Model.IsUsingTempFolder && (File.Exists(this.ProjectFilePath) || this.HasSavedOnce)) {
                if (this.IsSaving) {
                    await IoC.DialogService.ShowMessageAsync("Saving", "Project is already being saved");
                    return false;
                }

                try {
                    AppLogger.PushHeader("Begin save project (SaveActionAsync)");
                    return await this.SaveProjectData(this.ProjectFilePath, this.ProjectFolder);
                }
                finally {
                    AppLogger.PopHeader();
                }
            }
            else {
                return await this.SaveAsActionAsync();
            }
        }

        public async Task<bool> SaveAsActionAsync() {
            if (this.IsSaving) {
                await IoC.DialogService.ShowMessageAsync("Saving", "Project is already being saved");
                return false;
            }

            string initialPath;
            if (this.Model.IsUsingTempFolder) {
                initialPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), this.ProjectFileName);
            }
            else {
                initialPath = this.ProjectFilePath;
            }

            string newFilePath = await IoC.FilePicker.SaveFile(Filters.ProjectTypeAndAllFiles, initialPath, "Save the project to a new file");
            if (string.IsNullOrEmpty(newFilePath)) {
                return false;
            }

            string newDataFolder = Path.GetDirectoryName(newFilePath);
            if (string.IsNullOrEmpty(newDataFolder)) {
                await IoC.DialogService.ShowDialogAsync("Invalid file path", $"The file path is not located in a directory...? '{newFilePath}'");
                return false;
            }

            string newFileName = Path.GetFileName(newFilePath);
            string testDataFolder = Path.Combine(newDataFolder, Path.GetFileNameWithoutExtension(newFilePath));
            string testProjectFilePath = Path.Combine(testDataFolder, newFileName);
            // TODO: maybe use a custom dialog class to make the UI look better?
            string result = await ShouldCreateDataFolderDialog.ShowAsync("Data Folder",
                $"How do you want to use the data folder?\n" +
                $"Click 'New Folder' to create the project file at: \n    {testProjectFilePath}\n" +
                $"Click 'Current Folder' to create the project file at: \n    {newFilePath}{(File.Exists(newFilePath) ? "\n    (will overwrite existing project file)" : "")}");
            if (result == "yes") {
                newDataFolder = testDataFolder;
                newFilePath = testProjectFilePath;
            }
            else if (result != "no") {
                AppLogger.WriteLine("Save project cancelled");
                return false;
            }

            try {
                Directory.CreateDirectory(newDataFolder);
            }
            catch (PathTooLongException ex) {
                await IoC.DialogService.ShowMessageExAsync("Path too long", "Data Folder path was too long; could not create project data folder", ex.GetToString());
                return false;
            }
            catch (SecurityException ex) {
                await IoC.DialogService.ShowMessageExAsync("Security Exception", "Application does not have permission to create directories", ex.GetToString());
                return false;
            }
            catch (UnauthorizedAccessException ex) {
                await IoC.DialogService.ShowMessageExAsync("Unauthorized access", "Application does not have access to that directory", ex.GetToString());
                return false;
            }
            catch (Exception ex) {
                await IoC.DialogService.ShowMessageExAsync("Unexpected error", "Could not create project directory", ex.GetToString());
                return false;
            }

            try {
                AppLogger.PushHeader("Begin save project (SaveAsActionAsync)");
                return await this.SaveProjectData(newFilePath, newDataFolder);
            }
            finally {
                AppLogger.PopHeader();
            }
        }

        // Use debug and release versions in order for the debugger to pickup exceptions or Debugger.Break() (for debugging of course)

        /// <summary>
        /// Saves this project's data to the given folder and fpx file
        /// </summary>
        /// <param name="filePath">The path of the project file</param>
        /// <param name="projectFolder">The folder in which project data is saved in</param>
        /// <returns>True if the project was saved, otherwise false if the save was cancelled by the user</returns>
        /// <exception cref="Exception">Invalid project file paths</exception>
        private async Task<bool> SaveProjectData(string filePath, string projectFolder) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new Exception("File path cannot be empty");
            }

            AppLogger.WriteLine($"Saving project to '{filePath}'");

            this.IsSaving = true;
            if (this.Editor != null) {
                await this.Editor.OnProjectSaving();
            }

            bool loadResources = false;
            // copy temp project files into actual project dir
            if (this.Model.IsUsingTempFolder && Directory.Exists(this.Model.ProjectFolder)) {
                string sourceFolder = this.Model.ProjectFolder;
                AppLogger.WriteLine($"Copying temporary resources to new data folder:\n'{sourceFolder}' -> '{projectFolder}'");
                await this.ResourceManager.OfflineAllAsync(false);
                IEnumerable<string> enumerable = null;
                try {
                    enumerable = Directory.EnumerateFileSystemEntries(sourceFolder);
                }
                catch (Exception e) {
                    // e.g. no access or the folder gets deleted in the microseconds after Directory.Exists returns
                    AppLogger.WriteLine("Failed to create temp folder enumerator: " + e.GetToString());
                }

                if (enumerable != null) {
                    using (IEnumerator<string> enumerator = enumerable.GetEnumerator()) {
                        bool wasLastEnumerationError = false;
                        while (true) {
                            try {
                                if (!enumerator.MoveNext()) {
                                    break;
                                }
                            }
                            catch (Exception e) {
                                if (wasLastEnumerationError) {
                                    break;
                                }
                                else {
                                    wasLastEnumerationError = true;
                                }

                                AppLogger.WriteLine("Exception enumerating next file in temp folder: " + e.GetToString());
                                continue;
                            }

                            string path = enumerator.Current;
                            if (string.IsNullOrEmpty(path)) {
                                continue;
                            }

                            try {
                                Directory.Move(path, Path.Combine(projectFolder, Path.GetFileName(path)));
                            }
                            catch (Exception ex) {
                                AppLogger.WriteLine("Exception while moving path from temp to real directory: " + ex.GetToString());
                            }
                        }
                    }
                }

                try {
                    Directory.Delete(sourceFolder);
                }
                catch (Exception ex) {
                    AppLogger.WriteLine("Failed to delete temp data folder: " + ex.GetToString());
                }

                loadResources = true;
            }

            this.Model.SetProjectPaths(filePath, projectFolder);
            if (loadResources) {
                ResourceCheckerViewModel checker = new ResourceCheckerViewModel() {
                    Caption = "Moving files from temp to new data folder appears to broken things. Fix them here"
                };

                await ResourceCheckerViewModel.LoadProjectResources(checker, this, false);
            }

            // TODO: maybe add methods to allow resources or clips to save stuff to file here?

            AppLogger.WriteLine("Serialising to RBE");
            Exception exception = null;
            RBEDictionary dictionary = new RBEDictionary();
            try {
                this.Model.WriteToRBE(dictionary);
            }
            catch (Exception e) {
                exception = e;
                await IoC.DialogService.ShowMessageExAsync("Error saving project", "An exception occurred serialising saving project", e.GetToString());
            }

            if (exception == null) {
                AppLogger.WriteLine("Writing packed RBE to file");
                try {
                    RBEUtils.WriteToFilePacked(dictionary, filePath);
                }
                catch (Exception e) {
                    exception = e;
                    await IoC.DialogService.ShowMessageExAsync("Error saving project", "An exception occurred writing project to disk", e.GetToString());
                }
            }

            this.IsSaving = false;
            if (this.Editor != null) {
                await this.Editor.OnProjectSaved(exception);
            }

            if (exception == null) {
                this.HasSavedOnce = true;
                this.SetHasUnsavedChanges(false);
            }

            AppLogger.WriteLine("Project saved " + (exception == null ? "successfully" : "unsuccessfully"));
            this.RaisePropertyChanged(nameof(this.ProjectFolder));
            this.RaisePropertyChanged(nameof(this.ProjectFileName));
            this.RaisePropertyChanged(nameof(this.ProjectFilePath));
            return true;
        }

        public void Dispose() {
            using (ErrorList list = new ErrorList("Encountered an error while disposing project")) {
                try {
                    this.Timeline.ClearAndDispose();
                }
                catch (Exception e) {
                    list.Add(e);
                }

                try {
                    this.ResourceManager.ClearAndDispose();
                }
                catch (Exception e) {
                    list.Add(e);
                }

                AppLogger.WriteLine("Project disposed " + (list.IsEmpty ? "successfully" : "potentially unsuccessfully") + " at " + this.ProjectFilePath);
            }
        }

        public void OnDisconnectFromEditor() {
            this.Editor = null;
            this.Model.OnDisconnectedFromEditor();
        }

        public void OnConnectToEditor(VideoEditorViewModel editor) {
            this.Editor = editor; // this also sets the project model's editor
            this.Model.OnConnectedToEditor(editor.Model);
        }
    }
}