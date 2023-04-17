using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Project;
using FramePFX.Render;

namespace FramePFX {
    public class VideoEditor : BaseViewModel {
        public ViewportPlayback PlaybackView { get; }

        private EditorProject activeProject;
        public EditorProject ActiveProject {
            get => this.activeProject;
            set => this.RaisePropertyChanged(ref this.activeProject, value);
        }

        public ICommand NewProjectCommand { get; }

        public VideoEditor() {
            this.PlaybackView = new ViewportPlayback(this);
            this.NewProjectCommand = new RelayCommand(this.NewProjectAction);
        }

        public void NewProjectAction() {
            this.ActiveProject = new EditorProject(this);
            this.ActiveProject.SetupDefaultProject();
            this.UpdateResolution(this.ActiveProject.Resolution);
        }

        public bool IsReadyForRender() {
            return this.PlaybackView.IsReadyForRender();
        }

        public void UpdateResolution(Resolution res) {
            IViewPort vp = this.PlaybackView.ViewPortHandle;
            vp?.SetResolution(res.Width, res.Height);
        }
    }
}