using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Project;

namespace FramePFX {
    public class VideoEditorViewModel : BaseViewModel {
        public PlaybackViewportViewModel PlaybackView { get; }

        private ProjectViewModel activeProject;
        public ProjectViewModel ActiveProject {
            get => this.activeProject;
            set => this.RaisePropertyChanged(ref this.activeProject, value);
        }

        public ICommand NewProjectCommand { get; }

        public VideoEditorViewModel() {
            this.PlaybackView = new PlaybackViewportViewModel(this);
            this.NewProjectCommand = new RelayCommand(this.NewProjectAction);
        }

        public void NewProjectAction() {
            this.ActiveProject = new ProjectViewModel(this);
            this.ActiveProject.SetupDefaultProject();
            Resolution res = this.ActiveProject.PlaybackResolution;
            this.PlaybackView.ViewPortHandle.SetSize(res.Width, res.Height);
        }

        public bool IsReadyForRender() {
            return this.PlaybackView.IsReadyForRender();
        }
    }
}