using FramePFX.Core.Editor.ResourceManaging.Resources;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceTextViewModel : ResourceItemViewModel {
        public new ResourceText Model => (ResourceText) base.Model;

        public string Text {
            get => this.Model.Text;
            set {
                this.Model.Text = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.Model.Text));
            }
        }

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

        public ResourceTextViewModel(ResourceText model) : base(model) {

        }
    }
}