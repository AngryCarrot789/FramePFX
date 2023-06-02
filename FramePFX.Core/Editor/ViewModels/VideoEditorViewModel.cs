using System;
using System.Threading.Tasks;
using FramePFX.Core.Settings.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels {
    /// <summary>
    /// A view model that represents a video editor
    /// </summary>
    public class VideoEditorViewModel : BaseViewModel, IDisposable {
        private ProjectViewModel project;

        /// <summary>
        /// The project that is currently being edited in this editor. May be null if no project is loaded
        /// </summary>
        public ProjectViewModel Project {
            get => this.project;
            private set {
                this.Model.CurrentProject = value?.Model;
                this.RaisePropertyChanged(ref this.project, value);
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

        public VideoEditorViewModel(IVideoEditor view, ApplicationViewModel app) {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.Model = new VideoEditorModel();
            this.App = app;
            this.Playback = new EditorPlaybackViewModel(this);
        }

        public async Task SetProject(ProjectViewModel project) {
            await this.Playback.OnProjectChanging(project);
            this.Project = project;
            await this.Playback.OnProjectChanged(project);
        }

        public void Dispose() {
            IoC.App.OnUserSettingsModified -= this.OnUserSettingsModified;
            using (ExceptionStack stack = new ExceptionStack("Exception occurred while disposing video editor")) {
                ProjectViewModel activeProject = this.Project;
                if (activeProject != null) {
                    try {
                        activeProject.Dispose();
                    }
                    catch (Exception e) {
                        stack.Push(new Exception("Exception disposing active project", e));
                    }

                    this.Project = null;
                }

                try {
                    this.Playback.Dispose();
                }
                catch (Exception e) {
                    stack.Push(new Exception("Exception disposing playback", e));
                }
            }
        }

        private void OnUserSettingsModified(UserSettingsViewModel settings) {

        }

        public async Task CloseProjectAction() {
            this.Project.Dispose();
        }

        public async Task OnProjectSaving(ProjectViewModel project) {
            if (project != this.Project) {
                throw new Exception("Project does not equal the given project");
            }

            this.IsProjectSaving = true;
            await this.Playback.StopRenderTimer();
        }

        public async Task OnProjectSaved(ProjectViewModel project) {
            if (project != this.Project) {
                throw new Exception("Project does not equal the given project");
            }

            this.IsProjectSaving = false;
            this.Playback.StartRenderTimer();
        }
    }
}