using FramePFX.Settings.ViewModels;

namespace FramePFX {
    public class ApplicationViewModel : BaseViewModel {
        public ApplicationSettings Settings { get; }

        public static ApplicationViewModel Instance { get; } = new ApplicationViewModel();

        public ApplicationViewModel() {
            this.Settings = new ApplicationSettings();
        }
    }
}