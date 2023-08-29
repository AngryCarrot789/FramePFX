using FramePFX.Editor.ResourceManaging.Resources;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceColourViewModel : ResourceItemViewModel {
        public new ResourceColour Model => (ResourceColour) base.Model;

        public SKColor Colour {
            get => this.Model.Colour;
            set {
                this.Model.Colour = value;
                this.OnColourChanged();
            }
        }

        public float A {
            get => this.Model.ScA;
            set {
                this.Model.ScA = value;
                this.OnColourChanged();
            }
        }

        public byte ByteA {
            get => this.Model.ByteA;
            set {
                this.Model.ByteA = value;
                this.OnColourChanged();
            }
        }

        public float R {
            get => this.Model.ScR;
            set {
                this.Model.ScR = value;
                this.OnColourChanged();
            }
        }

        public byte ByteR {
            get => this.Model.ByteR;
            set {
                this.Model.ByteR = value;
                this.OnColourChanged();
            }
        }

        public float G {
            get => this.Model.ScG;
            set {
                this.Model.ScG = value;
                this.OnColourChanged();
            }
        }

        public byte ByteG {
            get => this.Model.ByteG;
            set {
                this.Model.ByteG = value;
                this.OnColourChanged();
            }
        }

        public float B {
            get => this.Model.ScB;
            set {
                this.Model.ScB = value;
                this.OnColourChanged();
            }
        }

        public byte ByteB {
            get => this.Model.ByteB;
            set {
                this.Model.ByteB = value;
                this.OnColourChanged();
            }
        }

        public ResourceColourViewModel(ResourceColour model) : base(model) {
        }

        private void OnColourChanged() {
            this.Model.OnDataModified(nameof(this.Colour));
            this.RaisePropertyChanged(nameof(this.Colour));
            this.RaisePropertyChanged(nameof(this.A));
            this.RaisePropertyChanged(nameof(this.ByteA));
            this.RaisePropertyChanged(nameof(this.R));
            this.RaisePropertyChanged(nameof(this.ByteR));
            this.RaisePropertyChanged(nameof(this.G));
            this.RaisePropertyChanged(nameof(this.ByteG));
            this.RaisePropertyChanged(nameof(this.B));
            this.RaisePropertyChanged(nameof(this.ByteB));
        }
    }
}