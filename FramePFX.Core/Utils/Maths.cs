using System;

namespace FramePFX.Core.Utils {
    public static class Maths {
        public static double Map(double input, double inA, double inB, double outA, double outB) {
            return outA + ((outB - outA) / (inB - inA) * (input - inA));
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
    }
}