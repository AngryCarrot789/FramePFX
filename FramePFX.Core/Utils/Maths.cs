using System;

namespace FramePFX.Core.Utils {
    public static class Maths {
        /// <summary>
        /// Maps a double value from the input range to the output range
        /// <code>17.5 = Map(75, 0, 100, 10, 20)</code>
        /// </summary>
        /// <param name="dIn">Input value</param>
        /// <param name="inA">Input range lower bound</param>
        /// <param name="inB">Input range upper bound</param>
        /// <param name="outA">Output range lower bound</param>
        /// <param name="outB">Output range upper bound</param>
        /// <returns>The output value, between outA and outB</returns>
        public static double Map(double dIn, double inA, double inB, double outA, double outB) {
            return outA + ((outB - outA) / (inB - inA) * (dIn - inA));
        }

        public static float Clamp(float value, float min, float max) {
            return Math.Max(Math.Min(value, max), min);
        }

        public static double Clamp(double value, double min, double max) {
            return Math.Max(Math.Min(value, max), min);
        }

        public static byte Clamp(byte value, byte min, byte max) {
            return Math.Max(Math.Min(value, max), min);
        }

        public static int Clamp(int value, int min, int max) {
            return Math.Max(Math.Min(value, max), min);
        }

        public static long Clamp(long value, long min, long max) {
            return Math.Max(Math.Min(value, max), min);
        }

        public static bool Equals(double a, double b, double tolerance = 0.0001d) {
            return Math.Abs(a - b) < tolerance;
        }

        public static bool Equals(float a, float b, float tolerance = 0.001f) {
            return Math.Abs(a - b) < tolerance;
        }

        public static bool IsOne(double value) => Math.Abs(value - 1.0) < 2.22044604925031E-15;

        public static bool IsZero(double value) => Math.Abs(value) < 2.22044604925031E-15; // 0.00000000000000222044604925031

        public static double Lerp(double a, double b, double blend) {
            return blend * (b - a) + a;
        }

        /// <summary>
        /// 64-bit integer lerp
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <param name="blend">Blend</param>
        /// <param name="roundingMode">0 = cast to long, 1 = floor, 2 = ceil, 3 = round</param>
        /// <returns>A lerp-ed long value</returns>
        public static long Lerp(long a, long b, double blend, int roundingMode) {
            double nA = a, nB = b;
            double val = blend * (nB - nA) + nA;
            switch (roundingMode) {
                case 0: return (long) val;
                case 1: return (long) Math.Floor(val);
                case 2: return (long) Math.Ceiling(val);
                case 3: return (long) Math.Round(val);
                default: throw new ArgumentOutOfRangeException(nameof(roundingMode), "Rounding Mode must be between 0 and 3");
            }
        }

        public static int Ceil(int value, int multiple) {
            int mod = value % multiple;
            return mod == 0 ? value : value + (multiple - mod);
        }

        public static long Ceil(long value, int multiple) {
            long mod = value % multiple;
            return mod == 0 ? value : value + (multiple - mod);
        }

        public static double Ceil(double value, int multiple) {
            double mod = value % multiple;
            return mod == 0D ? value : value + (multiple - mod);
        }
    }
}