using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Editor.Project.ViewModels;
using FramePFX.Render;

namespace FramePFX.Editor.ViewModels {
    public class PFXVideoEditor : BaseViewModel {
        public PFXViewportPlayback Playback { get; }

        private PFXProject activeProject;
        public PFXProject ActiveProject {
            get => this.activeProject;
            set => this.RaisePropertyChanged(ref this.activeProject, value);
        }

        public ICommand NewProjectCommand { get; }

        public PFXVideoEditor() {
            this.Playback = new PFXViewportPlayback(this);
            this.NewProjectCommand = new RelayCommand(this.NewProjectAction);
        }

        public void NewProjectAction() {
            this.ActiveProject = new PFXProject(this);
            this.ActiveProject.SetupDefaultProject();
            this.UpdateResolution(this.ActiveProject.Resolution);
        }

        public void UpdateResolution(Resolution res) {
            IViewPort vp = this.Playback.ViewPortHandle;
            vp?.SetResolution(res.Width, res.Height);
        }
    }
}