using System;
using System.Threading.Tasks;
using FramePFX.Core.Settings.ViewModels;
using FramePFX.Core.Utils;

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

        public VideoEditorViewModel(IVideoEditor view, ApplicationViewModel app) {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.Model = new VideoEditorModel();
            this.App = app;
            this.Playback = new EditorPlaybackViewModel(this);
            this.Playback.Model.OnStepFrame = () => {
                this.ActiveProject?.Timeline.OnStepFrameTick();
            };

            this.NewProjectCommand = new AsyncRelayCommand(async () => {
                ProjectViewModel project = new ProjectViewModel(new ProjectModel());
                project.Settings.Resolution = new Resolution(500, 500);
                await this.LoadProjectAction(project);
            });
        }

        public async Task LoadProjectAction(ProjectViewModel project) {
            if (this.ActiveProject != null) {
                await this.CloseProjectAction();
            }

            await this.SetProject(project);
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

        private void OnUserSettingsModified(UserSettingsViewModel settings) {

        }

        public async Task CloseProjectAction() {
            await this.ActiveProject.DisposeAsync();
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
            this.Playback.StartRenderTimer();
        }

        public void DoRender(bool schedule = false) {
            this.View.RenderViewPort(schedule);
        }
    }
}