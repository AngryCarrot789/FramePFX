using FramePFX.Core.Settings.ViewModels;

namespace FramePFX.Core {
    public class ApplicationViewModel : BaseViewModel {
        public ApplicationSettings Settings { get; }

        public ApplicationViewModel() {
            this.Settings = new ApplicationSettings();
        }
    }
}