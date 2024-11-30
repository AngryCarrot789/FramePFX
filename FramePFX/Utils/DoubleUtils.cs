// 
// Copyright (c) 2024-2024 REghZy
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

using System.Runtime.InteropServices;

namespace FramePFX.Utils;

public static class DoubleUtils {
    // https://en.wikipedia.org/wiki/Machine_epsilon
    internal const double DBL_EPSILON = 2.22E-16;
    internal const float FLT_EPSILON = 1.19E-7F;

    public static bool AreClose(double a, double b) {
        if (a == b)
            return true;
        double x = (Math.Abs(a) + Math.Abs(b) + 10.0) * DBL_EPSILON;
        double y = a - b;
        return -x < y && x > y;
    }

    public static bool LessThan(double a, double b) => a < b && !AreClose(a, b);

    public static bool GreaterThan(double a, double b) => a > b && !AreClose(a, b);

    public static bool LessThanOrClose(double a, double b) => a < b || AreClose(a, b);

    public static bool GreaterThanOrClose(double a, double b) => a > b || AreClose(a, b);

    public static bool IsOne(double value) => Math.Abs(value - 1.0) < 2.22044604925031E-15;

    public static bool IsZero(double value) => Math.Abs(value) < 2.22044604925031E-15;

    public static bool IsBetweenZeroAndOne(double val) => GreaterThanOrClose(val, 0.0) && LessThanOrClose(val, 1.0);

    public static int DoubleToInt(double val) => 0.0 >= val ? (int) (val - 0.5) : (int) (val + 0.5);

    public static bool IsNaN(double value) {
        NanUnion nanUnion = new NanUnion { DoubleValue = value };
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