//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;

namespace FramePFX.Utils
{
    public static class Maths
    {
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
        public static double Map(double dIn, double inA, double inB, double outA, double outB)
        {
            return outA + ((outB - outA) / (inB - inA) * (dIn - inA));
        }

        public static float Clamp(float value, float min, float max) => Math.Max(Math.Min(value, max), min);
        public static double Clamp(double value, double min, double max) => Math.Max(Math.Min(value, max), min);
        public static byte Clamp(byte value, byte min, byte max) => Math.Max(Math.Min(value, max), min);
        public static short Clamp(short value, short min, short max) => Math.Max(Math.Min(value, max), min);
        public static int Clamp(int value, int min, int max) => Math.Max(Math.Min(value, max), min);
        public static long Clamp(long value, long min, long max) => Math.Max(Math.Min(value, max), min);
        public static decimal Clamp(decimal value, decimal min, decimal max) => Math.Max(Math.Min(value, max), min);

        public static int Compare(double a, double b, double tolerance = 0.000001D)
        {
            if (double.IsNaN(a))
            {
                return double.IsNaN(b) ? 0 : -1;
            }
            else if (double.IsNaN(b))
            {
                return 1;
            }
            else
            {
                double d = Math.Abs(a - b);
                if (d < tolerance)
                {
                    return 0;
                }
                else if (a > b)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Checks if two doubles are equal within a certain tolerance range. The default tolerance is 0.000001,
        /// which is enough precision to handle values between Int32 <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>
        /// </summary>
        /// <param name="a">The lhs</param>
        /// <param name="b">The rhs</param>
        /// <param name="tolerance">The tolerance</param>
        /// <returns>True if the difference between a and b are less than the given tolerance</returns>
        public static bool Equals(double a, double b, double tolerance = 0.000001D)
        {
            return Math.Abs(a - b) < tolerance;
        }

        /// <summary>
        /// Checks if two doubles are equal within a certain tolerance range. The default tolerance is 0.01F,
        /// which is enough precision to handle values between -1000 and +1000. 0.01F is enough to handle between
        /// Int16 <see cref="short.MinValue"/> and <see cref="short.MaxValue"/>
        /// </summary>
        /// <param name="a">The lhs</param>
        /// <param name="b">The rhs</param>
        /// <param name="tolerance">The tolerance</param>
        /// <returns>True if the difference between a and b are less than the given tolerance</returns>
        public static bool Equals(float a, float b, float tolerance = 0.0001F)
        {
            return Math.Abs(a - b) < tolerance;
        }

        public static bool IsOne(double value) => Math.Abs(value - 1.0) < 2.22044604925031E-15;

        public static bool IsZero(double value) => Math.Abs(value) < 2.22044604925031E-15; // 0.00000000000000222044604925031

        public static double Lerp(double a, double b, double blend)
        {
            return a + (b - a) * blend;
        }

        /// <summary>
        /// 64-bit integer lerp
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <param name="blend">Blend</param>
        /// <param name="roundingMode">0 = cast to long, 1 = floor, 2 = ceil, 3 = round</param>
        /// <returns>A lerp-ed long value</returns>
        public static long Lerp(long a, long b, double blend, int roundingMode)
        {
            double nA = a, nB = b;
            double val = nA + (nB - nA) * blend;
            switch (roundingMode)
            {
                case 0: return (long) val;
                case 1: return (long) Math.Floor(val);
                case 2: return (long) Math.Ceiling(val);
                case 3: return (long) Math.Round(val);
                default: throw new ArgumentOutOfRangeException(nameof(roundingMode), "Rounding Mode must be between 0 and 3");
            }
        }

        public static double InverseLerp(double a, double b, double value)
        {
            return !Equals(a, b) ? ((value - a) / (b - a)) : 0f;
        }

        public static int Ceil(int value, int multiple)
        {
            int mod = value % multiple;
            return mod == 0 ? value : value + (multiple - mod);
        }

        public static long Ceil(long value, int multiple)
        {
            long mod = value % multiple;
            return mod == 0 ? value : value + (multiple - mod);
        }

        public static double Ceil(double value, int multiple)
        {
            double mod = value % multiple;
            return mod == 0D ? value : value + (multiple - mod);
        }

        public static int CeilShr(int v, int s) => (v + (1 << s) - 1) >> s;

        public static bool WillOverflow(uint a, uint b) => b != 0 && a > uint.MaxValue - b;
        public static bool WillOverflow(ulong a, ulong b) => b != 0 && a > ulong.MaxValue - b;
        public static bool WillOverflow(int a, int b) => b > 0 && a > int.MaxValue - b;
        public static bool WillOverflow(long a, long b) => b > 0 && a > long.MaxValue - b;
        public static bool WillUnderflow(int a, int b) => b < 0 && a < int.MinValue - b;
        public static bool WillUnderflow(long a, long b) => b < 0 && a < long.MinValue - b;

        // https://stackoverflow.com/a/51099524/11034928
        public static int GetDigitCount(ulong v)
        {
            // could optimise similar to a binary search, but hopefully the JIT will help out
            if (v < 10L)
                return 1;
            if (v < 100L)
                return 2;
            if (v < 1000L)
                return 3;
            if (v < 10000L)
                return 4;
            if (v < 100000L)
                return 5;
            if (v < 1000000L)
                return 6;
            if (v < 10000000L)
                return 7;
            if (v < 100000000L)
                return 8;
            if (v < 1000000000L)
                return 9;
            if (v < 10000000000L)
                return 10;
            if (v < 100000000000L)
                return 11;
            if (v < 1000000000000L)
                return 12;
            if (v < 10000000000000L)
                return 13;
            if (v < 100000000000000L)
                return 14;
            if (v < 1000000000000000L)
                return 15;
            if (v < 10000000000000000L)
                return 16;
            if (v < 100000000000000000L)
                return 17;
            if (v < 1000000000000000000L)
                return 18;
            return v < 10000000000000000000L ? 19 : 20;
        }

        public static void Swap(ref float a, ref float b)
        {
            float tmp = a;
            a = b;
            b = tmp;
        }

        public static void Swap(ref double a, ref double b)
        {
            double tmp = a;
            a = b;
            b = tmp;
        }

        public static void Swap(ref long a, ref long b)
        {
            long tmp = a;
            a = b;
            b = tmp;
        }

        public static void Swap(ref int a, ref int b)
        {
            int tmp = a;
            a = b;
            b = tmp;
        }

        public static float Swap(ref float a, float b)
        {
            float oA = a;
            a = b;
            return oA;
        }

        public static double Swap(ref double a, double b)
        {
            double oA = a;
            a = b;
            return oA;
        }

        public static long Swap(ref long a, long b)
        {
            long oA = a;
            a = b;
            return oA;
        }

        public static int Swap(ref int a, int b)
        {
            int oA = a;
            a = b;
            return oA;
        }

        public static double GetRange(float min, float max)
        {
            return max < min ? (min - max) : (max - min);
        }

        public static double GetRange(double min, double max)
        {
            return max < min ? (min - max) : (max - min);
        }

        public static double GetRange(long min, long max)
        {
            return max < min ? (min - max) : (max - min);
        }

        public static int Round(float value, RoundingMode mode = RoundingMode.Cast)
        {
            switch (mode)
            {
                case RoundingMode.None:
                case RoundingMode.Cast:
                    return (int) value;
                case RoundingMode.Floor: return (int) Math.Floor(value);
                case RoundingMode.Ceil: return (int) Math.Ceiling(value);
                case RoundingMode.Round: return (int) Math.Round(value);
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static int Round(double value, RoundingMode mode = RoundingMode.Cast)
        {
            switch (mode)
            {
                case RoundingMode.None:
                case RoundingMode.Cast:
                    return (int) value;
                case RoundingMode.Floor: return (int) Math.Floor(value);
                case RoundingMode.Ceil: return (int) Math.Ceiling(value);
                case RoundingMode.Round: return (int) Math.Round(value);
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static long RoundLong(double value, RoundingMode mode = RoundingMode.Cast)
        {
            switch (mode)
            {
                case RoundingMode.None:
                case RoundingMode.Cast:
                    return (long) value;
                case RoundingMode.Floor: return (long) Math.Floor(value);
                case RoundingMode.Ceil: return (long) Math.Ceiling(value);
                case RoundingMode.Round: return (long) Math.Round(value);
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}