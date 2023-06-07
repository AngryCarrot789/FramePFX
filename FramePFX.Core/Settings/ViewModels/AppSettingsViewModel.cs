namespace FramePFX.Core.Settings.ViewModels {
    public class AppSettingsViewModel : BaseViewModel {
        public AppSettingsModel Model { get; }

        public bool StopOnTogglePlay {
            get => this.Model.StopOnTogglePlay;
            set {
                this.Model.StopOnTogglePlay = value;
                this.RaisePropertyChanged();
            }
        }

        public int Width {get;} = 1920;
        public int Height {get;} = 1080;

        public AppSettingsViewModel() {
            this.Model = new AppSettingsModel();
        }
    }
}