using FramePFX.Core.RBC;

namespace FramePFX.Core.Settings.ViewModels {
    public class ApplicationSettings : BaseViewModel {
        private bool stopOnTogglePlay;
        public bool StopOnTogglePlay {
            get => this.stopOnTogglePlay;
            set => this.RaisePropertyChanged(ref this.stopOnTogglePlay, value);
        }

        public ApplicationSettings() {

        }
    }
}