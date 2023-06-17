using FramePFX.Core.Settings.ViewModels;

namespace FramePFX.Core {
    public class ApplicationViewModel : BaseViewModel {
        public delegate void UserSettingsChangedEventHandler(ApplicationSettings settings);

        private ApplicationSettings settings;
        public ApplicationSettings Settings {
            get => this.settings;
            set {
                this.RaisePropertyChanged(ref this.settings, value ?? (value = new ApplicationSettings()));
                this.OnUserSettingsModified?.Invoke(value);
            }
        }

        public event UserSettingsChangedEventHandler OnUserSettingsModified;

        public ApplicationViewModel() {
            this.Settings = new ApplicationSettings();
        }
    }
}