using System;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels {
    public class ProjectSettingsViewModel : BaseViewModel, IModifyProject {
        public ProjectSettingsModel Model { get; }

        public double FrameRate {
            get => this.Model.FrameRate;
            set {
                this.Model.FrameRate = value;
                this.RaisePropertyChanged();
                this.ProjectModified?.Invoke(this, nameof(this.FrameRate));
            }
        }

        public Resolution Resolution {
            get => this.Model.Resolution;
            set {
                this.Model.Resolution = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.Width));
                this.RaisePropertyChanged(nameof(this.Height));
                this.ProjectModified?.Invoke(this, nameof(this.Resolution));
            }
        }

        public int Width => this.Model.Resolution.Width;
        public int Height => this.Model.Resolution.Height;

        public event ProjectModifiedEvent ProjectModified;

        public ProjectSettingsViewModel(ProjectSettingsModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}