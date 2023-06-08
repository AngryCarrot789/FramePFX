using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.RBC;
using FramePFX.Core.Settings.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ViewModels {
    /// <summary>
    /// A view model that represents a video editor
    /// </summary>
    public class VideoEditorViewModel : BaseViewModel, IDisposable {
        private ProjectViewModel activeProject;

        /// <summary>
        /// The project that is currently being edited in this editor. May be null if no project is loaded
        /// </summary>
        public ProjectViewModel ActiveProject {
            get => this.activeProject;
            private set {
                if (ReferenceEquals(this.activeProject, value)) {
                    return;
                }

                if (this.activeProject != null) {
                    this.activeProject.Editor = null;
                }

                if (value != null) {
                    this.Model.ActiveProject = value.Model;
                    value.Editor = this;
                }
                else {
                    this.Model.ActiveProject = null;
                }

                this.RaisePropertyChanged(ref this.activeProject, value);
            }
        }

        public bool IsProjectSaving {
            get => this.Model.IsProjectSaving;
            set {
                this.Model.IsProjectSaving = value;
                this.RaisePropertyChanged();
            }
        }

        public EditorPlaybackViewModel Playback { get; }

        public ApplicationViewModel App { get; }

        public VideoEditorModel Model { get; }

        public IVideoEditor View { get; }

        public AsyncRelayCommand NewProjectCommand { get; }

        public HistoryManagerViewModel HistoryManager { get; }

        public AsyncRelayCommand OpenProjectCommand { get; }

        public VideoEditorViewModel(IVideoEditor view, ApplicationViewModel app) {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.Model = new VideoEditorModel();
            this.HistoryManager = new HistoryManagerViewModel(this.Model.HistoryManager);
            this.App = app;
            this.Playback = new EditorPlaybackViewModel(this);
            this.Playback.Model.OnStepFrame = () => {
                this.ActiveProject?.Timeline.OnStepFrameTick();
            };

            this.NewProjectCommand = new AsyncRelayCommand(async () => {
                ProjectViewModel project = new ProjectViewModel(new ProjectModel());
                project.Settings.Resolution = new Resolution(500, 500);
                if (this.ActiveProject != null && !await this.SaveAndCloseProjectAction()) {
                    return;
                }

                await this.SetProject(project);
            });

            this.OpenProjectCommand = new AsyncRelayCommand(async () => {
                if (this.ActiveProject != null && !await this.SaveProjectAction()) {
                    return;
                }

                DialogResult<string[]> result = IoC.FilePicker.ShowFilePickerDialog(Filters.ProjectTypeAndAllFiles);
                if (!result.IsSuccess || result.Value.Length < 1) {
                    return;
                }

                RBEDictionary dictionary;
                try {
                    dictionary = RBEUtils.ReadFromFile(result.Value[0]) as RBEDictionary;
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Read error", "Failed to read project from file", e.GetToString());
                    return;
                }

                if (dictionary == null) {
                    await IoC.MessageDialogs.ShowMessageAsync("Invalid project", "The project contains invalid data (non RBEDictionary)");
                    return;
                }

                ProjectModel project = new ProjectModel();
                try {
                    project.ReadFromRBE(dictionary);
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Project load error", "Failed to load project", e.GetToString());
                    return;
                }

                if (this.ActiveProject != null) {
                    try {
                        await this.CloseProjectAction();
                    }
                    catch (Exception e) {
                        await IoC.MessageDialogs.ShowMessageExAsync("Exception", "Failed to close previous project. This error can be ignored", e.GetToString());
                    }

                    this.ActiveProject = null;
                }

                ProjectViewModel vm = new ProjectViewModel(project);
                await this.SetProject(vm);
                await this.CheckProjectResources(vm);
            });
        }

        public async Task CheckProjectResources(ProjectViewModel project) {
            // TODO: Implement proper checks for resources that are offline
            foreach (ResourceItemViewModel resource in project.ResourceManager.Resources) {
                if (resource is ResourceImageViewModel image) {
                    if (!string.IsNullOrEmpty(image.FilePath)) {
                        try {
                            await image.Model.LoadImageAsync(image.FilePath);
                        }
                        catch (Exception e) {
                            /* ignored, for now */
                            Debug.WriteLine(e.GetToString());
                        }
                    }
                }
                else if (resource is ResourceMediaViewModel media) {
                    if (!string.IsNullOrEmpty(media.FilePath)) {
                        try {
                            media.Model.OpenMediaFromFile();
                        }
                        catch (Exception e) {
                            /* ignored, for now */
                            Debug.WriteLine(e.GetToString());
                        }
                    }
                }
            }
        }

        public async Task SetProject(ProjectViewModel project) {
            await this.Playback.OnProjectChanging(project);
            this.ActiveProject = project;
            await this.Playback.OnProjectChanged(project);
        }

        public void Dispose() {
            IoC.App.OnUserSettingsModified -= this.OnUserSettingsModified;
            using (ExceptionStack stack = new ExceptionStack("Exception occurred while disposing video editor")) {
                if (this.ActiveProject != null) {
                    try {
                        this.ActiveProject.Dispose();
                    }
                    catch (Exception e) {
                        stack.Push(new Exception("Exception disposing active project", e));
                    }
                }

                try {
                    this.Playback.Dispose();
                }
                catch (Exception e) {
                    stack.Push(new Exception("Exception disposing playback", e));
                }
            }
        }

        private void OnUserSettingsModified(AppSettingsViewModel settings) {

        }

        /// <summary>
        /// Saves and closes the project
        /// </summary>
        /// <returns>True when the project was closed,</returns>
        public async Task<bool> SaveAndCloseProjectAction() {
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

            await this.CloseProjectAction();
            return true;
        }

        public async Task<bool> SaveProjectAction() {
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

            this.ActiveProject.Dispose();
            await this.SetProject(null);
        }

        public async Task OnProjectSaving(ProjectViewModel project) {
            if (project != this.ActiveProject) {
                throw new Exception("Project does not equal the given project");
            }

            this.IsProjectSaving = true;
            await this.Playback.OnProjectSaving();
            await this.Playback.StopRenderTimer();
        }

        public async Task OnProjectSaved(ProjectViewModel project) {
            if (project != this.ActiveProject) {
                throw new Exception("Project does not equal the given project");
            }

            this.IsProjectSaving = false;
            await this.Playback.OnProjectSaved();
        }

        public void DoRender(bool schedule = false) {
            this.View.RenderViewPort(schedule);
        }
    }
}