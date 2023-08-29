using System;
using System.Numerics;

namespace FramePFX.Utils {
    public static class Vectors {
        public static readonly Vector2 Zero = Vector2.Zero;
        public static readonly Vector2 One = Vector2.One;
        public static readonly Vector2 MinValue = new Vector2(float.MinValue);
        public static readonly Vector2 MaxValue = new Vector2(float.MaxValue);

        public static Vector2 Clamp(this Vector2 a, Vector2 min, Vector2 max) => Vector2.Clamp(a, min, max);
        public static Vector3 Clamp(this Vector3 a, Vector3 min, Vector3 max) => Vector3.Clamp(a, min, max);
        public static Vector4 Clamp(this Vector4 a, Vector4 min, Vector4 max) => Vector4.Clamp(a, min, max);

        public static Vector2 Floor(this Vector2 vector) => new Vector2((float) Math.Floor(vector.X), (float) Math.Floor(vector.Y));
        public static Vector2 Ceil(this Vector2 vector) => new Vector2((float) Math.Ceiling(vector.X), (float) Math.Ceiling(vector.Y));
        public static Vector2 Round(this Vector2 vector, int digits) => new Vector2((float) Math.Round(vector.X, digits), (float) Math.Round(vector.Y, digits));

        public static Vector3 Round(this Vector3 vector, int digits) => new Vector3((float) Math.Round(vector.X, digits), (float) Math.Round(vector.Y, digits), (float) Math.Round(vector.Z, digits));

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

        public static Vector2 Lerp(in this Vector2 a, in Vector2 b, float blend) {
            return new Vector2(blend * (b.X - a.X) + a.X, blend * (b.Y - a.Y) + a.Y);
        }
    }
}