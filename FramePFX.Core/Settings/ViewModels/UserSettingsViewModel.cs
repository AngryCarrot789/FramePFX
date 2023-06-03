namespace FramePFX.Core.Settings.ViewModels {
    public class UserSettingsViewModel : BaseViewModel {
        public UserSettingsModel Model { get; }

        public bool StopOnTogglePlay {
            get => this.Model.StopOnTogglePlay;
            set {
                this.Model.StopOnTogglePlay = value;
                this.RaisePropertyChanged();
            }
        }

        public int Width {get;} = 1920;
        public int Height {get;} = 1080;

        public UserSettingsViewModel() {
            this.Model = new UserSettingsModel();
        }
    }
}