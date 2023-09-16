using FramePFX.Settings.ViewModels;

namespace FramePFX {
    public class ApplicationViewModel : BaseViewModel {
        public ApplicationSettings Settings { get; }

        public ApplicationViewModel() {
            this.Settings = new ApplicationSettings();
        }
    }
}