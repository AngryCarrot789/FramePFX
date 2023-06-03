using FramePFX.Core.ResourceManaging.Resources;
using FramePFX.Core.Utils;

namespace FramePFX.Core.ResourceManaging.ViewModels.Resources {
    public class ResourceARGBViewModel : ResourceItemViewModel {
        public new ResourceARGB Model => (ResourceARGB) base.Model;

        public float A {
            get => this.Model.A;
            set {
                this.Model.A = value; this.RaisePropertyChanged();
                this.Model.OnModified?.Invoke(this.Model, nameof(this.Model.A));
            }
        }

        public byte ByteA {
            get => (byte) Maths.Clamp((int) (this.A * 255F), 0, 255);
            set {
                this.A = Maths.Clamp(value / 255f, 0f, 1f); this.RaisePropertyChanged();
            }
        }

        public float R {
            get => this.Model.R;
            set {
                this.Model.R = value; this.RaisePropertyChanged();
                this.Model.OnModified?.Invoke(this.Model, nameof(this.Model.R));
            }
        }

        public byte ByteR {
            get => (byte) Maths.Clamp((int) (this.R * 255F), 0, 255);
            set {
                this.R = Maths.Clamp(value / 255f, 0f, 1f); this.RaisePropertyChanged();
            }
        }

        public float G {
            get => this.Model.G;
            set {
                this.Model.G = value; this.RaisePropertyChanged();
                this.Model.OnModified?.Invoke(this.Model, nameof(this.Model.G));
            }
        }

        public byte ByteG {
            get => (byte) Maths.Clamp((int) (this.G * 255F), 0, 255);
            set {
                this.G = Maths.Clamp(value / 255f, 0f, 1f); this.RaisePropertyChanged();
            }
        }

        public float B {
            get => this.Model.B;
            set {
                this.Model.B = value; this.RaisePropertyChanged();
                this.Model.OnModified?.Invoke(this.Model, nameof(this.Model.B));
            }
        }

        public byte ByteB {
            get => (byte) Maths.Clamp((int) (this.B * 255F), 0, 255);
            set {
                this.B = Maths.Clamp(value / 255f, 0f, 1f); this.RaisePropertyChanged();
            }
        }

        public ResourceARGBViewModel(ResourceManagerViewModel manager, ResourceARGB model) : base(manager, model) {

        }
    }
}