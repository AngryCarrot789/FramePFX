using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.Resources {
    public class ResourceARGB : ResourceItem {
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

        public ResourceARGB(ResourceManager manager) : base(manager) {

        }
    }
}