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

        public UserSettingsViewModel() {
            this.Model = new UserSettingsModel();
        }
    }
}