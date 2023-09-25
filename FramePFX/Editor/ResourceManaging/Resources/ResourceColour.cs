using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.Resources {
    public class ResourceColour : ResourceItem {
        public SKColor Colour { get; set; } = new SKColor(0, 0, 0, 255);

        public float ScR {
            get => Maths.Clamp(this.Colour.Red / 255F, 0F, 1F);
            set => this.Colour = this.Colour.WithRed((byte) Maths.Clamp((int) (value * 255F), 0, 255));
        }

        public float ScG {
            get => Maths.Clamp(this.Colour.Green / 255F, 0F, 1F);
            set => this.Colour = this.Colour.WithGreen((byte) Maths.Clamp((int) (value * 255F), 0, 255));
        }

        public float ScB {
            get => Maths.Clamp(this.Colour.Blue / 255F, 0F, 1F);
            set => this.Colour = this.Colour.WithBlue((byte) Maths.Clamp((int) (value * 255F), 0, 255));
        }

        public float ScA {
            get => Maths.Clamp(this.Colour.Alpha / 255F, 0F, 1F);
            set => this.Colour = this.Colour.WithAlpha((byte) Maths.Clamp((int) (value * 255F), 0, 255));
        }

        public byte ByteR {
            get => this.Colour.Red;
            set => this.Colour = this.Colour.WithRed(value);
        }

        public byte ByteG {
            get => this.Colour.Green;
            set => this.Colour = this.Colour.WithGreen(value);
        }

        public byte ByteB {
            get => this.Colour.Blue;
            set => this.Colour = this.Colour.WithBlue(value);
        }

        public byte ByteA {
            get => this.Colour.Alpha;
            set => this.Colour = this.Colour.WithAlpha(value);
        }

        public ResourceColour() {
        }

        public ResourceColour(byte r, byte g, byte b, byte a = 255) {
            this.Colour = new SKColor(r, g, b, a);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetInt(nameof(this.Colour), (int) (uint) this.Colour);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Colour = new SKColor(data.GetUInt(nameof(this.Colour)));
        }

        protected override void LoadCloneDataFromObject(BaseResource obj) {
            base.LoadCloneDataFromObject(obj);
            ResourceColour colour = (ResourceColour) obj;
            this.Colour = colour.Colour;
        }
    }
}