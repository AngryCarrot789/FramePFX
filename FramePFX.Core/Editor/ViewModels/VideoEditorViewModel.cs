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

        public EditorPlaybackViewModel Playback { get; }

        public VideoEditorModel Model { get; }

        public IVideoEditor View { get; }

        public VideoEditorViewModel(IVideoEditor view) {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.Model = new VideoEditorModel();
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
    }
}