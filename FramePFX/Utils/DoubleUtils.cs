using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace FramePFX.Utils {
    public class DoubleUtils {
        internal const double DBL_EPSILON = 2.22044604925031E-16;
        internal const float FLT_MIN = 1.175494E-38f;

        public static bool AreClose(double a, double b) {
            if (a == b)
                return true;
            double x = (Math.Abs(a) + Math.Abs(b) + 10.0) * 2.22044604925031E-16;
            double y = a - b;
            return -x < y && x > y;
        }

        public static bool LessThan(double a, double b) => a < b && !AreClose(a, b);

        public static bool GreaterThan(double a, double b) => a > b && !AreClose(a, b);

        public static bool LessThanOrClose(double a, double b) => a < b || AreClose(a, b);

        public static bool GreaterThanOrClose(double a, double b) => a > b || AreClose(a, b);

        public static bool IsOne(double value) => Math.Abs(value - 1.0) < 2.22044604925031E-15;

        public static bool IsZero(double value) => Math.Abs(value) < 2.22044604925031E-15;

        public static bool AreClose(Point point1, Point point2) => AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y);

        public static bool AreClose(Size size1, Size size2) => AreClose(size1.Width, size2.Width) && AreClose(size1.Height, size2.Height);

        public static bool AreClose(Vector vector1, Vector vector2) => DoubleUtils.AreClose(vector1.X, vector2.X) && DoubleUtils.AreClose(vector1.Y, vector2.Y);

        public static bool AreClose(Rect rect1, Rect rect2) {
            if (rect1.IsEmpty)
                return rect2.IsEmpty;
            return !rect2.IsEmpty && DoubleUtils.AreClose(rect1.X, rect2.X) && (DoubleUtils.AreClose(rect1.Y, rect2.Y) && AreClose(rect1.Height, rect2.Height)) && AreClose(rect1.Width, rect2.Width);
        }

        public static bool IsBetweenZeroAndOne(double val) => GreaterThanOrClose(val, 0.0) && LessThanOrClose(val, 1.0);

        public static int DoubleToInt(double val) => 0.0 >= val ? (int) (val - 0.5) : (int) (val + 0.5);

        public static bool RectHasNaN(Rect r) => IsNaN(r.X) || IsNaN(r.Y) || (IsNaN(r.Height) || IsNaN(r.Width));

        public static bool IsNaN(double value) {
            NanUnion nanUnion = new NanUnion {DoubleValue = value};
            ulong num1 = nanUnion.UintValue & 18442240474082181120UL;
            ulong num2 = nanUnion.UintValue & 4503599627370495UL;
            return (num1 == 9218868437227405312UL || num1 == 18442240474082181120UL) && num2 > 0UL;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion {
            [FieldOffset(0)] internal double DoubleValue;
            [FieldOffset(0)] internal ulong UintValue;
        }
    }
}