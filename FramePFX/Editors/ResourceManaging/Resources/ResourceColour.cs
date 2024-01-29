using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.ResourceManaging.Resources {
    public class ResourceColour : ResourceItem {
        private SKColor myColour;
        public SKColor Colour {
            get => this.myColour;
            set {
                if (this.myColour != value) {
                    this.myColour = value;
                    this.ColourChanged?.Invoke(this);
                }
            }
        }

        public float ScR {
            get => Maths.Clamp(this.myColour.Red / 255F, 0F, 1F);
            set => this.myColour = this.myColour.WithRed((byte) Maths.Clamp((int) (value * 255F), 0, 255));
        }

        public float ScG {
            get => Maths.Clamp(this.myColour.Green / 255F, 0F, 1F);
            set => this.myColour = this.myColour.WithGreen((byte) Maths.Clamp((int) (value * 255F), 0, 255));
        }

        public float ScB {
            get => Maths.Clamp(this.myColour.Blue / 255F, 0F, 1F);
            set => this.myColour = this.myColour.WithBlue((byte) Maths.Clamp((int) (value * 255F), 0, 255));
        }

        public float ScA {
            get => Maths.Clamp(this.myColour.Alpha / 255F, 0F, 1F);
            set => this.myColour = this.myColour.WithAlpha((byte) Maths.Clamp((int) (value * 255F), 0, 255));
        }

        public byte ByteR {
            get => this.myColour.Red;
            set => this.myColour = this.myColour.WithRed(value);
        }

        public byte ByteG {
            get => this.myColour.Green;
            set => this.myColour = this.myColour.WithGreen(value);
        }

        public byte ByteB {
            get => this.myColour.Blue;
            set => this.myColour = this.myColour.WithBlue(value);
        }

        public byte ByteA {
            get => this.myColour.Alpha;
            set => this.myColour = this.myColour.WithAlpha(value);
        }

        public event Events.BaseResourceEventHandler ColourChanged;

        public ResourceColour() : this(0, 0, 0) {
        }

        public ResourceColour(byte r, byte g, byte b, byte a = 255) {
            this.myColour = new SKColor(r, g, b, a);
            this.Enable(null);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetUInt(nameof(this.myColour), (uint) this.myColour);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.myColour = new SKColor(data.GetUInt(nameof(this.myColour)));
        }

        protected override void LoadDataIntoClone(BaseResource clone) {
            base.LoadDataIntoClone(clone);
            ((ResourceColour) clone).myColour = this.myColour;
        }
    }
}