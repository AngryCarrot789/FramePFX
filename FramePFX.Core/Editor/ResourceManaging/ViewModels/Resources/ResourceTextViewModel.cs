using FramePFX.Core.Editor.ResourceManaging.Resources;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceTextViewModel : ResourceItemViewModel {
        public new ResourceText Model => (ResourceText) base.Model;

        public string Text {
            get => this.Model.Text;
            set {
                this.Model.Text = value;
                this.RaisePropertyChanged();
            }
        }

        public double FontSize {
            get => this.Model.FontSize;
            set {
                this.Model.FontSize = value;
                this.RaisePropertyChanged();
            }
        }

        public double SkewX {
            get => this.Model.SkewX;
            set {
                this.Model.SkewX = value;
                this.RaisePropertyChanged();
            }
        }

        public string FontFamily {
            get => this.Model.FontFamily;
            set {
                this.Model.FontFamily = value;
                this.RaisePropertyChanged();
            }
        }

        public ResourceTextViewModel(ResourceManagerViewModel manager, ResourceText model) : base(manager, model) {

        }
    }
}