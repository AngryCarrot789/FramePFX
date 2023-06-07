using FramePFX.Core.Settings.ViewModels;

namespace FramePFX.Core {
    public class ApplicationViewModel : BaseViewModel {
        public delegate void UserSettingsChangedEventHandler(AppSettingsViewModel settings);

        private AppSettingsViewModel appSettings;
        public AppSettingsViewModel AppSettings {
            get => this.appSettings;
            set {
                this.RaisePropertyChanged(ref this.appSettings, value ?? (value = new AppSettingsViewModel()));
                this.OnUserSettingsModified?.Invoke(value);
            }
        }

        public event UserSettingsChangedEventHandler OnUserSettingsModified;

        public ApplicationViewModel() {
            this.AppSettings = new AppSettingsViewModel();
        }
    }
}