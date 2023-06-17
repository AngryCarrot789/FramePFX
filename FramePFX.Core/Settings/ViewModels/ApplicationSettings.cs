namespace FramePFX.Core.Settings.ViewModels {
    public class AppSettingsViewModel : BaseViewModel {
        private bool stopOnTogglePlay;
        public bool StopOnTogglePlay {
            get => this.stopOnTogglePlay;
            set => this.RaisePropertyChanged(ref this.stopOnTogglePlay, value);
        }

        public int Width {get;} = 1920;
        public int Height {get;} = 1080;

        public AppSettingsViewModel() {
        }
    }
}