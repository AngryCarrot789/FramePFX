using FramePFX.Editor.ViewModels;
using FramePFX.Settings.ViewModels;

namespace FramePFX {
    public class ApplicationViewModel : BaseViewModel {
        public ApplicationSettings Settings { get; }

        public VideoEditorViewModel Editor { get; set; }

        public ApplicationViewModel() {
            this.Settings = new ApplicationSettings();
        }
    }
}