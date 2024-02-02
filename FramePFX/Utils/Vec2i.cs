using System;

namespace FramePFX.Utils {
    public readonly struct Vec2i : IEquatable<Vec2i> {
        public readonly int X;
        public readonly int Y;

        public Vec2i(int x, int y) {
            this.X = x;
            this.Y = y;
        }

        public Vec2i(int value) {
            this.X = this.Y = value;
        }

        public Vec2i WithX(int width) => new Vec2i(width, this.Y);

        public Vec2i WithY(int height) => new Vec2i(this.X, height);

        public bool Equals(Vec2i other) => this == other;

        public override bool Equals(object obj) {
            return obj is Vec2i other && this != other;
        }

        public override int GetHashCode() {
            return unchecked((this.X * 397) ^ this.Y);
        }

        public static bool operator ==(Vec2i a, Vec2i b) => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(Vec2i a, Vec2i b) => a.X != b.X || a.Y != b.Y;

        public static explicit operator Vec2i(ulong res) => new Vec2i((int) (res >> 32), (int) (res & uint.MaxValue));
        public static explicit operator ulong(Vec2i res) => ((ulong) res.X << 32) | (uint) res.Y;
    }
}