using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Project;

namespace FramePFX {
    public class VideoEditorViewModel : BaseViewModel {
        public ViewportPlayback PlaybackView { get; }

        private Project.Project activeProject;
        public Project.Project ActiveProject {
            get => this.activeProject;
            set => this.RaisePropertyChanged(ref this.activeProject, value);
        }

        public ICommand NewProjectCommand { get; }

        public VideoEditorViewModel() {
            this.PlaybackView = new ViewportPlayback(this);
            this.NewProjectCommand = new RelayCommand(this.NewProjectAction);
        }

        public void NewProjectAction() {
            this.ActiveProject = new Project.Project(this);
            this.ActiveProject.SetupDefaultProject();
            Resolution res = this.ActiveProject.PlaybackResolution;
            this.PlaybackView.ViewPortHandle.SetResolution(res.Width, res.Height);
        }

        public bool IsReadyForRender() {
            return this.PlaybackView.IsReadyForRender();
        }
    }
}