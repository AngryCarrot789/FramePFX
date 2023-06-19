using FramePFX.Core.Editor.ResourceManaging.Resources;
using SkiaSharp;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceColourViewModel : ResourceItemViewModel {
        public new ResourceColour Model => (ResourceColour) base.Model;

        public SKColor Colour {
            get => this.Model.Colour;
            set {
                this.Model.Colour = value;
                this.Model.OnDataModified(nameof(this.Colour));
                this.RaisePropertyChanged();
            }
        }

        public float A {
            get => this.Model.ScA;
            set {
                this.Model.ScA = value;
                this.Model.OnDataModified(nameof(this.Model.Colour));
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ByteA));
            }
        }

        public byte ByteA {
            get => this.Model.ByteA;
            set {
                this.Model.ByteA = value;
                this.Model.OnDataModified(nameof(this.Model.Colour));
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.A));
            }
        }

        public float R {
            get => this.Model.ScR;
            set {
                this.Model.ScR = value;
                this.Model.OnDataModified(nameof(this.Model.Colour));
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ByteR));
            }
        }

        public byte ByteR {
            get => this.Model.ByteR;
            set {
                this.Model.ByteR = value;
                this.Model.OnDataModified(nameof(this.Model.Colour));
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.R));
            }
        }

        public float G {
            get => this.Model.ScG;
            set {
                this.Model.ScG = value;
                this.Model.OnDataModified(nameof(this.Model.Colour));
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ByteG));
            }
        }

        public byte ByteG {
            get => this.Model.ByteG;
            set {
                this.Model.ByteG = value;
                this.Model.OnDataModified(nameof(this.Model.Colour));
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.G));
            }
        }

        public float B {
            get => this.Model.ScB;
            set {
                this.Model.ScB = value;
                this.Model.OnDataModified(nameof(this.Model.Colour));
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ByteB));
            }
        }

        public byte ByteB {
            get => this.Model.ByteB;
            set {
                this.Model.ByteB = value;
                this.Model.OnDataModified(nameof(this.Model.Colour));
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.B));
            }
        }

        public ResourceColourViewModel(ResourceColour model) : base(model) {

        }
    }
}