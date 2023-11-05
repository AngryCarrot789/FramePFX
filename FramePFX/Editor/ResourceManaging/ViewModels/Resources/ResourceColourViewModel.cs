using FramePFX.AdvancedContextService;
using FramePFX.AdvancedContextService.NCSP;
using FramePFX.Editor.ResourceManaging.Resources;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceColourViewModel : ResourceItemViewModel {
        public new ResourceColour Model => (ResourceColour) base.Model;

        public SKColor Colour {
            get => this.Model.Colour;
            set => this.Model.Colour = value;
        }

        public float A {
            get => this.Model.ScA;
            set => this.Model.ScA = value;
        }

        public byte ByteA {
            get => this.Model.ByteA;
            set => this.Model.ByteA = value;
        }

        public float R {
            get => this.Model.ScR;
            set => this.Model.ScR = value;
        }

        public byte ByteR {
            get => this.Model.ByteR;
            set => this.Model.ByteR = value;
        }

        public float G {
            get => this.Model.ScG;
            set => this.Model.ScG = value;
        }

        public byte ByteG {
            get => this.Model.ByteG;
            set => this.Model.ByteG = value;
        }

        public float B {
            get => this.Model.ScB;
            set => this.Model.ScB = value;
        }

        public byte ByteB {
            get => this.Model.ByteB;
            set => this.Model.ByteB = value;
        }

        public ResourceColourViewModel(ResourceColour model) : base(model) {
        }

        static ResourceColourViewModel() {
            PropertyMap.AddTranslation(typeof(ResourceColour), nameof(ResourceColour.Colour), nameof(Colour));

            IContextRegistration reg = ContextRegistry.Instance.RegisterType(typeof(ResourceColourViewModel));
            reg.AddEntry(new ActionContextEntry("action.resources.ChangeResourceColour", "Change Colour..."));
        }

        protected override void RaisePropertyChangedCore(string propertyName) {
            base.RaisePropertyChangedCore(propertyName);
            if (propertyName == nameof(this.Colour)) {
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
}