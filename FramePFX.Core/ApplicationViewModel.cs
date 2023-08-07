using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Settings.ViewModels;

namespace FramePFX.Core {
    public class ApplicationViewModel : BaseViewModel {
        public ApplicationSettings Settings { get; }

        public VideoEditorViewModel Editor { get; set; }

        public ApplicationViewModel() {
            this.Settings = new ApplicationSettings();
        }
    }
}