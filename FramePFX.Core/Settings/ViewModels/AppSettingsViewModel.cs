namespace FramePFX.Core.Settings.ViewModels {
    public class UserSettingsViewModel : BaseViewModel {
        public AppSettingsModel Model { get; }

        public bool StopOnTogglePlay {
            get => this.Model.StopOnTogglePlay;
            set {
                this.Model.StopOnTogglePlay = value;
                this.RaisePropertyChanged();
            }
        }

        public int Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        public int Width {get;} = 1920;
        public int Height {get;} = 1080;

        public UserSettingsViewModel() {
            this.Model = new AppSettingsModel();
            this.width = 1920;
        }
    }
}