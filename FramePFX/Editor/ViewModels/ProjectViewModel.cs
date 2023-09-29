using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
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

        public Project Model { get; }

        public TimelineViewModel Timeline { get; }

        public ResourceManagerViewModel ResourceManager { get; }

        public string TheProjectDataFolder => this.Model.DataFolder;

        public string TheProjectFileName => this.Model.ProjectFileName;

        public string FullProjectFilePath => this.Model.FullProjectFilePath;

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
            set {
                if (this.IsExporting == value)
                    return;
                this.Model.IsExporting = value;
                this.RaisePropertyChanged();
            }
        }

        public ProjectViewModel(Project project) {
            this.Model = project ?? throw new ArgumentNullException(nameof(project));
            this.Settings = new ProjectSettingsViewModel(project.Settings);
            this.Settings.ProjectModified += (sender, property) => this.OnProjectModified();
            this.ResourceManager = new ResourceManagerViewModel(this, project.ResourceManager);
            this.Timeline = new TimelineViewModel(project.Timeline);
            this.Timeline.SetProject(this);
            this.SaveCommand = new AsyncRelayCommand(this.SaveActionAsync, () => this.Editor != null && !this.IsSaving);
            this.SaveAsCommand = new AsyncRelayCommand(this.SaveAsActionAsync, () => this.Editor != null && !this.IsSaving);
            this.OpenSettingsCommand = new AsyncRelayCommand(this.OpenSettingsAction);
            this.Model.GetDataFolder();
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

        public async Task OpenSettingsAction() {
            EditorPlaybackViewModel playback = this.Editor?.Playback;
            if (playback == null) {
                return;
            }

            if (playback.IsPlaying) {
                await playback.StopRenderTimer();
            }

            ProjectSettings result = await Services.GetService<IProjectSettingsEditor>().EditSettingsAsync(this.Settings.Model);
            if (result != null) {
                this.Settings.Resolution = result.Resolution;

                Rational oldFps = this.Settings.FrameRate;
                this.Settings.FrameRate = result.TimeBase;
                playback.SetTimerFrameRate(result.TimeBase);

                if (oldFps != this.Settings.FrameRate) {
                    if (await Services.DialogService.ShowYesNoDialogAsync("Convert Framerate", "Do you want to convert clip and automation to match the new FPS?")) {
                        AutomationEngine.ConvertProjectFrameRate(this, oldFps, result.TimeBase);

                        if (this.editor != null && this.editor.SelectedTimeline != null) {
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
            if (this.Model.IsTempDataFolder || this.TheProjectDataFolder == null || !File.Exists(this.FullProjectFilePath) && !this.HasSavedOnce) {
                return await this.SaveAsActionAsync();
            }
            else {
                if (this.Editor == null)
                    return false;
                if (this.IsSaving) {
                    await Services.DialogService.ShowMessageAsync("Saving", "Project is already being saved");
                    return false;
                }

                try {
                    AppLogger.PushHeader("Begin save project (SaveActionAsync)");
                    return await this.SaveProjectData(this.FullProjectFilePath);
                }
                finally {
                    AppLogger.PopHeader();
                }
            }
        }

        public async Task<bool> SaveAsActionAsync() {
            if (this.Editor == null)
                return false;
            if (this.IsSaving) {
                await Services.DialogService.ShowMessageAsync("Saving", "Project is already being saved");
                return false;
            }

            string file = await Services.FilePicker.SaveFile(Filters.ProjectTypeAndAllFiles, this.FullProjectFilePath, "Save the project to a new file");
            if (string.IsNullOrEmpty(file)) {
                return false;
            }

            try {
                AppLogger.PushHeader("Begin save project (SaveAsActionAsync)");
                return await this.SaveProjectData(file);
            }
            finally {
                AppLogger.PopHeader();
            }
        }

        // Use debug and release versions in order for the debugger to pickup exceptions or Debugger.Break() (for debugging of course)

        private async Task<bool> SaveProjectData(string fpxFilePath) {
            if (string.IsNullOrWhiteSpace(fpxFilePath)) {
                throw new Exception("Project file path cannot be an empty string");
            }

            if (!fpxFilePath.EndsWith(Filters.DotFrameFPXExtension)) {
                fpxFilePath += Filters.DotFrameFPXExtension;
            }

            string parentFolder;
            try {
                parentFolder = Path.GetDirectoryName(fpxFilePath);
            }
            catch (ArgumentException) {
                await Services.DialogService.ShowMessageAsync("Invalid file", "The project file contains invalid characters");
                return false;
            }

            if (parentFolder == null) {
                await Services.DialogService.ShowMessageAsync("Invalid file", "The project file path represents a root-level directory (e.g. C:\\ drive)");
                return false;
            }
            else if (parentFolder.Length < 1) {
                await Services.DialogService.ShowMessageAsync("Invalid file", "The project file does not have any directory information");
                return false;
            }

            string projectFileName = Path.GetFileName(fpxFilePath);
            string preview_dataFolder = Path.Combine(parentFolder, Path.GetFileNameWithoutExtension(fpxFilePath));
            string preview_fpxFilePath = Path.Combine(preview_dataFolder, projectFileName);

            string dataFolder;
            string result = await ShouldCreateDataFolderDialog.ShowAsync("Data Folder",
                $"How do you want to use the data folder?\n" +
                $"Click 'New Folder' to create the project file at: \n    {preview_fpxFilePath}\n" +
                $"Click 'Current Folder' to create the project file at: \n    {fpxFilePath}{(File.Exists(fpxFilePath) ? "\n    (will overwrite existing project file)" : "")}");
            if (result == "yes") {
                dataFolder = preview_dataFolder;
                fpxFilePath = preview_fpxFilePath;
            }
            else if (result == "no") {
                dataFolder = parentFolder;
            }
            else {
                AppLogger.WriteLine("Save project cancelled");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(dataFolder)) {
                try {
                    Directory.CreateDirectory(dataFolder);
                }
                catch (PathTooLongException ex) {
                    await Services.DialogService.ShowMessageExAsync("Path too long", "Data Folder path was too long; could not create project data folder", ex.GetToString());
                }
                catch (SecurityException ex) {
                    await Services.DialogService.ShowMessageExAsync("Security Exception", "Application does not have permission to create directories", ex.GetToString());
                }
                catch (UnauthorizedAccessException ex) {
                    await Services.DialogService.ShowMessageExAsync("Unauthorized access", "Application does not have access to that directory", ex.GetToString());
                }
                catch (Exception ex) {
                    await Services.DialogService.ShowMessageExAsync("Unexpected error", "Could not create project directory", ex.GetToString());
                }
            }

            AppLogger.WriteLine($"Saving project to '{fpxFilePath}'");

            this.IsSaving = true;
            if (this.Editor != null) {
                await this.Editor.OnProjectSaving();
            }

            bool loadResources = false;
            // copy temp project files into actual project dir
            if (this.Model.IsTempDataFolder && this.Model.DataFolder != null && Directory.Exists(this.Model.DataFolder)) {
                AppLogger.WriteLine($"Copying temporary resources to new data folder:\n'{this.Model.DataFolder}' -> '{dataFolder}'");
                await this.ResourceManager.OfflineAllAsync(false);
                IEnumerable<string> enumerable = null;
                try {
                    enumerable = Directory.EnumerateFileSystemEntries(this.Model.DataFolder);
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
                                Directory.Move(path, Path.Combine(dataFolder, Path.GetFileName(path)));
                            }
                            catch (Exception ex) {
                                AppLogger.WriteLine("Exception while moving path from temp to real directory: " + ex.GetToString());
                            }
                        }
                    }
                }

                try {
                    Directory.Delete(this.Model.DataFolder);
                }
                catch (Exception ex) {
                    AppLogger.WriteLine("Failed to delete temp data folder: " + ex.GetToString());
                }

                loadResources = true;
            }

            this.Model.SetProjectFileLocation(dataFolder, projectFileName, fpxFilePath);

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
                await Services.DialogService.ShowMessageExAsync("Error saving project", "An exception occurred serialising saving project", e.GetToString());
            }

            if (exception == null) {
                AppLogger.WriteLine("Writing packed RBE to file");
                try {
                    RBEUtils.WriteToFilePacked(dictionary, fpxFilePath);
                }
                catch (Exception e) {
                    exception = e;
                    await Services.DialogService.ShowMessageExAsync("Error saving project", "An exception occurred writing project to disk", e.GetToString());
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
            this.RaisePropertyChanged(nameof(this.TheProjectDataFolder));
            this.RaisePropertyChanged(nameof(this.TheProjectFileName));
            this.RaisePropertyChanged(nameof(this.FullProjectFilePath));
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

                AppLogger.WriteLine("Project disposed " + (list.IsEmpty ? "successfully" : "potentially unsuccessfully") + " at " + this.FullProjectFilePath);
            }
        }

        public void OnDisconnectFromEditor() {
            this.Editor = null;
        }

        public void OnConnectToEditor(VideoEditorViewModel editor) {
            this.Editor = editor; // this also sets the project model's editor
        }
    }
}