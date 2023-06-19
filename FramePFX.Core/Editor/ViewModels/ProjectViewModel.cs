using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ViewModels {
    public class ProjectViewModel : BaseViewModel, IDisposable {
        public bool HasSavedOnce { get; set; }

        private string projectFilePath;

        private bool hasUnsavedChanges;
        public bool HasUnsavedChanges {
            get => this.hasUnsavedChanges;
            private set => this.RaisePropertyChanged(ref this.hasUnsavedChanges, value);
        }

        public ProjectSettingsViewModel Settings { get; }

        public ProjectModel Model { get; }

        public TimelineViewModel Timeline { get; }

        public ResourceManagerViewModel ResourceManager { get; }

        public AutomationEngineViewModel AutomationEngine { get; }

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
            this.Settings.ProjectModified += this.OnProjectModified;
            this.ResourceManager = new ResourceManagerViewModel(this, project.ResourceManager);
            this.ResourceManager.ProjectModified += this.OnProjectModified;
            this.Timeline = new TimelineViewModel(this, project.Timeline);
            this.Timeline.ProjectModified += this.OnProjectModified;
            this.AutomationEngine = new AutomationEngineViewModel(this, project.AutomationEngine);

            this.SaveCommand = new AsyncRelayCommand(this.SaveActionAsync, () => this.Editor != null && !this.Model.IsSaving);
            this.SaveAsCommand = new AsyncRelayCommand(this.SaveAsActionAsync, () => this.Editor != null && !this.Model.IsSaving);
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

            if (File.Exists(this.ProjectFilePath) || (!string.IsNullOrEmpty(this.ProjectFilePath) && this.HasSavedOnce)) {
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

            DialogResult<string> result = IoC.FilePicker.SaveFile(Filters.ProjectTypeAndAllFiles, Path.GetDirectoryName(this.ProjectFilePath), "Select a folder, in which the project data will be saved into");
            if (result.IsSuccess) {
                this.ProjectFilePath = result.Value;
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

            this.Model.IsSaving = true;
            await this.Editor.OnProjectSaving();

            Exception e = null;
            RBEDictionary dictionary = new RBEDictionary();
            #if DEBUG
            this.Model.WriteToRBE(dictionary);
            #else
            try {
                this.Model.WriteToRBE(dictionary);
            }
            catch (Exception exception) {
                e = new Exception("Failed to serialise project", exception);
            }
            #endif

            #if DEBUG
            RBEUtils.WriteToFilePacked(dictionary, this.ProjectFilePath);
            #else
            if (e == null) {
                try {
                    RBEUtils.WriteToFilePacked(dictionary, this.ProjectFilePath);
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