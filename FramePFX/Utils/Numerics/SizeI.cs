using System;

namespace FramePFX.Utils.Numerics {
    public struct SizeI : IEquatable<SizeI> {
        public static readonly SizeI Empty = default;

        public int Width;
        public int Height;

        public SizeI(int width, int height) {
            this.Width = width;
            this.Height = height;
        }

        public SizeI(Vector2i vec) {
            this.Width = vec.X;
            this.Height = vec.Y;
        }

        public bool IsEmpty => this == SizeI.Empty;

        public override string ToString() => $"SizeI({this.Width.ToString()}, {this.Height.ToString()})";

        public static SizeI operator +(SizeI a, SizeI b) => new SizeI(a.Width + b.Width, a.Height + b.Height);

        public static SizeI operator -(SizeI a, SizeI b) => new SizeI(a.Width - b.Width, a.Height - b.Height);

        public bool Equals(SizeI obj) => this.Width == obj.Width && this.Height == obj.Height;

        public override bool Equals(object obj) => obj is SizeI skSizeI && this.Equals(skSizeI);

        public static bool operator ==(SizeI left, SizeI right) => left.Equals(right);

        public static bool operator !=(SizeI left, SizeI right) => !left.Equals(right);

        public override int GetHashCode() => (this.Width * 397) ^ this.Height;
    }
}