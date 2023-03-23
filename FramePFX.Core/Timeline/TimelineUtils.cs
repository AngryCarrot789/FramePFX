using System;

namespace FramePFX.Core.Timeline {
    public static class TimelineUtils {
        public const double MinUnitZoom = 0.0001d;

        public static object ClampUnit(object value) {
            return (double) value < MinUnitZoom ? MinUnitZoom : value;
        }

        public static bool IsUnitEqual(double a, double b) {
            return Math.Abs(a - b) <= MinUnitZoom;
        }

        public static long PixelToFrame(double pixels, double zoom) {
            return (long) (pixels / zoom);
        }

        public static double FrameToPixel(long pixels, double zoom) {
            return pixels * zoom;
        }

        public static void ValidateNonNegative(double value) {
            if (value < 0d) {
                throw new Exception("New value cannot be null");
            }
        }

        public static void ValidateNonNegative(long value) {
            if (value < 0d) {
                throw new Exception("New value cannot be null");
            }
        }
    }
}
