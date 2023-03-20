using System;

namespace FramePFX.Timeline {
    public static class TimelineUtils {
        public const double MinUnitZoom = 0.0001d;

        public static object ClampUnitZoom(object width) {
            return (double) width < MinUnitZoom ? MinUnitZoom : width;
        }

        public static bool IsZoomEqual(double a, double b) {
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
