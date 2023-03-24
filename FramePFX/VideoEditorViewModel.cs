using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Project;

namespace FramePFX {
    public class VideoEditorViewModel : BaseViewModel {
        public PlaybackViewportViewModel Viewport { get; }

        private ProjectViewModel activeProject;
        public ProjectViewModel ActiveProject {
            get => this.activeProject;
            set => this.RaisePropertyChanged(ref this.activeProject, value);
        }

        public ICommand NewProjectCommand { get; }

        public VideoEditorViewModel() {
            this.Viewport = new PlaybackViewportViewModel();
            this.NewProjectCommand = new RelayCommand(this.NewProjectAction);
        }

        public void NewProjectAction() {
            this.ActiveProject = new ProjectViewModel();
            this.ActiveProject.SetupDefaultProject();
            Resolution res = this.ActiveProject.PlaybackResolution;
            this.Viewport.ViewPortHandle.UpdateViewportSize(res.Width, res.Height);
        }

        public bool IsReadyForRender() {
            return this.Viewport.IsReadyForRender();
        }
    }
}