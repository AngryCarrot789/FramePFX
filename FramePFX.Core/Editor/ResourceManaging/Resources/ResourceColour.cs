using FramePFX.Core.RBC;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.ResourceManaging.Resources {
    public class ResourceColour : ResourceItem {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; } = 1f;

        public byte ByteA {
            get => (byte) Maths.Clamp((int) (this.A * 255F), 0, 255);
            set => this.A = Maths.Clamp(value / 255f, 0f, 1f);
        }

        public byte ByteR {
            get => (byte) Maths.Clamp((int) (this.R * 255F), 0, 255);
            set => this.R = Maths.Clamp(value / 255f, 0f, 1f);
        }

        public byte ByteG {
            get => (byte) Maths.Clamp((int) (this.G * 255F), 0, 255);
            set => this.G = Maths.Clamp(value / 255f, 0f, 1f);
        }

        public byte ByteB {
            get => (byte) Maths.Clamp((int) (this.B * 255F), 0, 255);
            set => this.B = Maths.Clamp(value / 255f, 0f, 1f);
        }

        public ResourceColour() {

        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetStruct("Colour", new SKColorF(this.R, this.G, this.B, this.A));
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            SKColorF colour = data.GetStruct<SKColorF>("Colour");
            this.R = colour.Red;
            this.G = colour.Green;
            this.B = colour.Blue;
            this.A = colour.Alpha;
        }
    }
}