using FramePFX.Core.Editor.ResourceManaging.Resources;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceColourViewModel : ResourceItemViewModel {
        public new ResourceColour Model => (ResourceColour) base.Model;

        public float A {
            get => this.Model.A;
            set {
                this.Model.A = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.ByteA));
            }
        }

        public byte ByteA {
            get => this.Model.ByteA;
            set {
                this.Model.ByteA = value;
                this.RaisePropertyChanged(nameof(this.A));
                this.RaisePropertyChanged();
            }
        }

        public float R {
            get => this.Model.R;
            set {
                this.Model.R = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.ByteR));
            }
        }

        public byte ByteR {
            get => this.Model.ByteR;
            set {
                this.Model.ByteR = value;
                this.RaisePropertyChanged(nameof(this.R));
                this.RaisePropertyChanged();
            }
        }

        public float G {
            get => this.Model.G;
            set {
                this.Model.G = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.ByteG));
            }
        }

        public byte ByteG {
            get => this.Model.ByteG;
            set {
                this.Model.ByteG = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.G));
            }
        }

        public float B {
            get => this.Model.B;
            set {
                this.Model.B = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.ByteB));
            }
        }

        public byte ByteB {
            get => this.Model.ByteB;
            set {
                this.Model.ByteB = value;
                this.RaisePropertyChanged(nameof(this.B));
                this.RaisePropertyChanged();
            }
        }

        public ResourceColourViewModel(ResourceColour model) : base(model) {

        }
    }
}