using FramePFX.Core.Settings.ViewModels;

namespace FramePFX.Core {
    public class ApplicationViewModel : BaseViewModel {
        public delegate void UserSettingsChangedEventHandler(UserSettingsViewModel settings);

        private UserSettingsViewModel userSettings;
        public UserSettingsViewModel UserSettings {
            get => this.userSettings;
            set {
                this.RaisePropertyChanged(ref this.userSettings, value ?? (value = new UserSettingsViewModel()));
                this.OnUserSettingsModified?.Invoke(value);
            }
        }

        public event UserSettingsChangedEventHandler OnUserSettingsModified;

        public ApplicationViewModel() {
            this.UserSettings = new UserSettingsViewModel();
        }
    }
}