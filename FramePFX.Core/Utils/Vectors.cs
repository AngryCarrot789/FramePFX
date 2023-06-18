using System;
using System.Numerics;

namespace FramePFX.Core.Utils {
    public static class VectorExtensions {
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
    }
}