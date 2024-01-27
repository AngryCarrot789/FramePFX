using System.Numerics;
using System.Windows;
using SkiaSharp;

namespace FramePFX.Editors.Utils {
    public static class SKUtils {
        public static SKPoint AsSkia(this Point point) {
            return new SKPoint((float) point.X, (float) point.Y);
        }

        public static SKRect AsSkia(this Rect point) {
            return new SKRect((float) point.Left, (float) point.Top, (float) point.Right, (float) point.Bottom);
        }

        public static SKRect ToSkiaAsSize(this Vector2 size, float x, float y) {
            return new SKRect(x, y, size.X + x, size.Y + y);
        }
    }
}