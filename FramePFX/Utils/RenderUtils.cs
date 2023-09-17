using System;
using SkiaSharp;

namespace FramePFX.Utils {
    public static class RenderUtils {
        public static SKColor BlendAlpha(SKColor colour, double alpha) {
            return colour.WithAlpha(MultiplyByte255(colour.Alpha, alpha));
        }

        public static byte MultiplyByte255(byte a, double b) {
            return (byte) Maths.Clamp((int) Math.Round(a / 255d * b * 255d), 0, 255);
        }

        public static byte DoubleToByte255(double value) {
            return (byte) Maths.Clamp((int) Math.Round(value / 255d), 0, 255);
        }
    }
}