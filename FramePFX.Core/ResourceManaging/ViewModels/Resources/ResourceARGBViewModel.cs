using FramePFX.Core.ResourceManaging.Resources;
using FramePFX.Core.Utils;

namespace FramePFX.Core.ResourceManaging.ViewModels.Resources {
    public class ResourceRGBAViewModel : ResourceItemViewModel {
        public new ResourceRGBA Model => (ResourceRGBA) base.Model;

        public float A {
            get => this.Model.A;
            set {
                this.Model.A = value; this.RaisePropertyChanged();
            }
        }

        public byte ByteA {
            get => (byte) Maths.Clamp((int) (this.Model.A * 255F), 0, 255);
            set {
                this.Model.A = Maths.Clamp(value / 255f, 0f, 1f); this.RaisePropertyChanged();
            }
        }

        public float R {
            get => this.Model.R;
            set {
                this.Model.R = value; this.RaisePropertyChanged();
            }
        }

        public byte ByteR {
            get => (byte) Maths.Clamp((int) (this.Model.R * 255F), 0, 255);
            set {
                this.Model.R = Maths.Clamp(value / 255f, 0f, 1f); this.RaisePropertyChanged();
            }
        }

        public float G {
            get => this.Model.G;
            set {
                this.Model.G = value; this.RaisePropertyChanged();
            }
        }

        public byte ByteG {
            get => (byte) Maths.Clamp((int) (this.Model.G * 255F), 0, 255);
            set {
                this.Model.G = Maths.Clamp(value / 255f, 0f, 1f); this.RaisePropertyChanged();
            }
        }

        public float B {
            get => this.Model.B;
            set {
                this.Model.B = value; this.RaisePropertyChanged();
            }
        }

        public byte ByteB {
            get => (byte) Maths.Clamp((int) (this.Model.B * 255F), 0, 255);
            set {
                this.Model.B = Maths.Clamp(value / 255f, 0f, 1f); this.RaisePropertyChanged();
            }
        }

        public ResourceRGBAViewModel(ResourceManagerViewModel manager, ResourceRGBA model) : base(manager, model) {

        }
    }
}