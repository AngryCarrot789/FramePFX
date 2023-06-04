using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ViewModels {
    public class ProjectViewModel : BaseViewModel, IDisposable {
        private bool hasSavedOnce;
        private string projectFilePath;

        public ProjectSettingsViewModel Settings { get; }

        public ProjectModel Model { get; }

        public TimelineViewModel Timeline { get; }

        public ResourceManagerViewModel ResourceManager { get; }

        /// <summary>
        /// The path of the project file, which links all of the saved data the project references
        /// </summary>
        public string ProjectFilePath {
            get => this.projectFilePath;
            set => this.RaisePropertyChanged(ref this.projectFilePath, value);
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

        public ProjectViewModel(ProjectModel project) {
            this.Model = project ?? throw new ArgumentNullException(nameof(project));
            this.Settings = new ProjectSettingsViewModel(project.Settings);
            this.ResourceManager = new ResourceManagerViewModel(this, project.ResourceManager);
            this.Timeline = new TimelineViewModel(this, project.Timeline);

            this.SaveCommand = new AsyncRelayCommand(this.SaveActionAsync, () => this.Editor != null && !this.Model.IsSaving);
            this.SaveAsCommand = new AsyncRelayCommand(this.SaveAsActionAsync, () => this.Editor != null && !this.Model.IsSaving);
        }

        public async Task<bool> SaveActionAsync() {
            if (this.Editor == null || this.Model.IsSaving) {
                return false;
            }

            if (File.Exists(this.ProjectFilePath) || (!string.IsNullOrEmpty(this.ProjectFilePath) && this.hasSavedOnce)) {
                await this.SaveActionAsync(this.ProjectFilePath);
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

            DialogResult<string> result = IoC.FilePicker.ShowFolderPickerDialog(Path.GetDirectoryName(this.ProjectFilePath), "Select a folder, in which the project data will be saved into");
            if (result.IsSuccess) {
                string filePath = Path.Combine(result.Value, "");
                this.ProjectFilePath = filePath;
                await this.SaveActionAsync(filePath);
                return true;
            }
            else {
                return false;
            }
        }

        public async Task SaveActionAsync(string file) {
            if (this.Editor == null) {
                return;
            }

            this.Model.IsSaving = true;
            await this.Editor.OnProjectSaving(this);

            Exception e = null;
            await Task.Run(() => {
                try {
                    this.Model.SaveProject(file);
                }
                catch (Exception exception) {
                    e = exception;
                }
            });

            this.Model.IsSaving = false;
            this.hasSavedOnce = true;
            await this.Editor.OnProjectSaved(this);

            if (e != null) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error saving", "An exception occurred while saving project", e.GetToString());
            }
        }

        public async Task DisposeAsync() {
            try {
                this.Timeline.Dispose();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Timeline exception", "An exception occurred while cleaning up timeline", e.GetToString());
            }
        }

        public void Dispose() {
            this.Timeline.Dispose();
        }
    }
}