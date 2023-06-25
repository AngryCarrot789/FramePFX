using System.Reflection;

namespace FramePFX.Core.Settings.ViewModels {
    public class ApplicationSettings : BaseViewModel {
        public AppSettings Settings { get; private set; }

        public bool UseVerticalTrackNumberDraggerBehaviour {
            get => this.Settings.UseVerticalTrackNumberDraggerBehaviour;
            set => this.RaisePropertyChanged(ref this.Settings.UseVerticalTrackNumberDraggerBehaviour, value);
        }

        public bool StopOnTogglePlay {
            get => this.Settings.StopOnTogglePlay;
            set => this.RaisePropertyChanged(ref this.Settings.StopOnTogglePlay, value);
        }

        public ApplicationSettings() {
            this.Settings = AppSettings.Defaults();
        }

        public void SetSettings(AppSettings settings) {
            this.Settings = settings ?? AppSettings.Defaults();

            // lazy lol
            // includes settings property, even though it shouldn't be bound to
            foreach (PropertyInfo info in this.GetType().GetProperties()) {
                this.RaisePropertyChanged(info.Name);
            }
        }
    }
}