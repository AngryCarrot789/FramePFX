using System;
using System.Runtime.InteropServices;

namespace FramePFX.Editor.Timeline.Utils {
    public static class TimelineUtils {
        public const double MinUnitZoom = 0.001d;
        public static readonly object MinUnitZoomObject = MinUnitZoom;

        public static object ClampUnit(object value) {
            return (double) value < MinUnitZoom ? MinUnitZoomObject : value;
        }

        public static double ClampUnit(double value) {
            return value < MinUnitZoom ? MinUnitZoom : value;
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
