namespace FramePFX.Utils {
    public readonly struct Vec2i {
        public int X { get; }

        public int Y { get; }

        public Vec2i(int x, int y) {
            this.X = x;
            this.Y = y;
        }

        public Vec2i(int value) {
            this.X = this.Y = value;
        }

        public bool Equals(in Vec2i other) {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object obj) {
            return obj is Vec2i other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.X * 397) ^ this.Y;
            }
        }
    }
}