using System;
using System.Numerics;

namespace FramePFX.Core.Utils {
    public static class Vectors {
        public static readonly Vector2 Zero = Vector2.Zero;
        public static readonly Vector2 One = Vector2.One;
        public static readonly Vector2 NegativeInfinity = new Vector2(float.NegativeInfinity);
        public static readonly Vector2 PositiveInfinity = new Vector2(float.PositiveInfinity);

        public static Vector2 Clamp(in this Vector2 a, in Vector2 min, in Vector2 max) {
            return new Vector2(Maths.Clamp(a.X, min.X, max.X), Maths.Clamp(a.Y, min.Y, max.Y));
        }

        public static Vector2 Clamp(in this Vector2 a, Vector2 min, Vector2 max) {
            return new Vector2(Maths.Clamp(a.X, min.X, max.X), Maths.Clamp(a.Y, min.Y, max.Y));
        }

        public static Vector2 Round(in this Vector2 vector, int dX, int dY) {
            return new Vector2((float) Math.Round(vector.X, dX), (float) Math.Round(vector.X, dY));
        }

        public static Vector2 Round(in this Vector2 vector, int digits) {
            return new Vector2((float) Math.Round(vector.X, digits), (float) Math.Round(vector.X, digits));
        }

        public static bool IsPositiveInfinityX(in this Vector2 vector) {
            return float.IsPositiveInfinity(vector.X);
        }

        public static bool IsPositiveInfinityY(in this Vector2 vector) {
            return float.IsPositiveInfinity(vector.Y);
        }

        public static bool IsNegativeInfinityX(in this Vector2 vector) {
            return float.IsNegativeInfinity(vector.X);
        }

        public static bool IsNegativeInfinityY(in this Vector2 vector) {
            return float.IsNegativeInfinity(vector.Y);
        }

        public static Vector2 Lerp(in this Vector2 a, in Vector2 b, double blend) {
            return new Vector2((float) (blend * (b.X - a.X) + a.X), (float) (blend * (b.Y - a.Y) + a.Y));
        }
    }
}