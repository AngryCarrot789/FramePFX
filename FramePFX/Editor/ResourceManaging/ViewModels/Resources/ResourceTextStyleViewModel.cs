using FramePFX.Editor.ResourceManaging.Resources;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceTextStyleViewModel : ResourceItemViewModel {
        public new ResourceTextStyle Model => (ResourceTextStyle) base.Model;

        public double FontSize {
            get => this.Model.FontSize;
            set {
                this.Model.FontSize = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.Model.FontSize));
            }
        }

        public double SkewX {
            get => this.Model.SkewX;
            set {
                this.Model.SkewX = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.Model.SkewX));
            }
        }

        public string FontFamily {
            get => this.Model.FontFamily;
            set {
                this.Model.FontFamily = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.Model.FontFamily));
            }
        }

        public ResourceTextStyleViewModel(ResourceTextStyle model) : base(model) {
        }
    }
}