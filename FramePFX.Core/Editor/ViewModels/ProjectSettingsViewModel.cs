using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels {
    public class ProjectSettingsViewModel : BaseViewModel {
        public ProjectSettingsModel Model { get; }

        public double FrameRate {
            get => this.Model.FrameRate;
            set {
                this.Model.FrameRate = value;
                this.RaisePropertyChanged();
            }
        }

        public Resolution Resolution {
            get => this.Model.Resolution;
            set {
                this.Model.Resolution = value;
                this.RaisePropertyChanged();
            }
        }

        public ProjectSettingsViewModel() {
            this.Model = new ProjectSettingsModel();
        }
    }
}