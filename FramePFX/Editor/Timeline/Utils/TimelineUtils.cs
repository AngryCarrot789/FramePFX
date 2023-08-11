using System;

namespace FramePFX.Editor.Timeline.Utils {
    public static class TimelineUtils {
        public const double MinUnitZoom = 0.01d;
        public const double MaxUnitZoom = 200d;
        public static readonly object ZeroDoubleBox = 0d;
        public static readonly object ZeroLongBox = 0L;
        public static readonly object MinUnitZoomDoubleBox = MinUnitZoom;
        public static readonly object MaxUnitZoomDoubleBox = MaxUnitZoom;

        // use object and double based functions to slightly help performance. not like
        // it will be measurable but oh well. WPF uses an internal BoolBox class so...

        public static object ClampUnit(object value) {
            double dval = (double) value;
            if (dval < MinUnitZoom) {
                return MinUnitZoomDoubleBox;
            }
            else if (dval > MaxUnitZoom) {
                return MaxUnitZoomDoubleBox;
            }
            else {
                return value;
            }
        }

        public static double ClampUnit(double value) {
            if (value < MinUnitZoom) {
                return MinUnitZoom;
            }
            else if (value > MaxUnitZoom) {
                return MaxUnitZoom;
            }
            else {
                return value;
            }
        }

        public static long PixelToFrame(double pixels, double zoom) {
            return (long) (pixels / zoom);
        }

        public static double FrameToPixel(long pixels, double zoom) {
            return pixels * zoom;
        }

        public static void ValidateNonNegative(double value) {
            if (value < 0d) {
                throw new Exception("New value cannot be negative");
            }
        }

        public static void ValidateNonNegative(long value) {
            if (value < 0d) {
                throw new Exception("New value cannot be negative");
            }
        }
    }
}