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

using System.Runtime.CompilerServices;
using FFmpeg.AutoGen;

namespace FramePFX.Editing;

/// <summary>
/// A rational number consisting of a numerator and denominator, as well as some helper functions (comparison, reduction, addition, etc)
/// </summary>
public readonly struct Rational : IComparable, IComparable<Rational>, IEquatable<Rational> {
    /// <summary>
    /// A rational with a numerator and denominator of 0, which is typically an invalid rational (representing zero time)
    /// </summary>
    public static readonly Rational NaN = default;

    public static readonly Rational Zero = new Rational(0, 1);
    public static readonly Rational One = new Rational(1, 1);

    /// <summary>
    /// The numerator, e.g. 30000
    /// </summary>
    public readonly int num;

    /// <summary>
    /// The denominator, e.g. 1001
    /// </summary>
    public readonly int den;

    /// <summary>
    /// <see cref="ffmpeg.av_q2d"/>
    /// </summary>
    public double AsDouble => this.num / (double) this.den;

    public int AsInt => (int) Math.Round(this.AsDouble);

    public bool IsNaN => this.num == 0 && this.den == 0;

    /// <summary>
    /// Returns a new rational where the denominator and numerator are swapped (<see cref="ffmpeg.av_inv_q"/>)
    /// </summary>
    public Rational Inverse => new Rational(this.den, this.num);

    public Rational Reduced {
        get {
            Reduce(out Rational r, this.num, this.den);
            return r;
        }
    }

    /// <summary>
    /// Creates a new rational number, with the given numerator and a denominator of 1
    /// </summary>
    /// <param name="numerator">The numerator value</param>
    public Rational(int numerator) {
        this.num = numerator;
        this.den = 1;
    }

    /// <summary>
    /// Creates a new rational number, with the given numerator and denominator
    /// </summary>
    /// <param name="numerator">The numerator value</param>
    /// <param name="denominator">The denominator value</param>
    public Rational(int numerator, int denominator) {
        this.num = numerator;
        this.den = denominator;
    }

    /// <summary>
    /// Calculates a suitable rational from the given decimal number and a maximum
    /// </summary>
    /// <param name="d">The double number</param>
    /// <param name="max">The maximum allowed numerator and denominator</param>
    /// <returns></returns>
    public static Rational FromDouble(double d, int max = int.MaxValue) {
#if false // The actual implementation but in C# code. Native is probably faster, even with the P/Invoke overhead
            Rational r; // a 2nd hidden local Rational variable is still created...
            const double LOG2 = 0.69314718055994530941723212145817656807550013436025;
            if (double.IsNaN(d))
                r = NaN;
            else if (double.IsInfinity(d))
                r = new Rational(0, d < 0 ? -1 : 1);
            else {
                int exp = Math.Max((int) (Math.Log(Math.Abs(d) + 1e-20) / LOG2), 0);
                long den = 1L << (61 - exp);
                unsafe {
                    ffmpeg.av_reduce(&r.num, &r.den, (long) (d * den + 0.5), den, max);
                }
            }
            return r;
#else
        AVRational rational = ffmpeg.av_d2q(d, max);
        return Unsafe.As<AVRational, Rational>(ref rational);
#endif
    }

    public static unsafe bool Reduce(out int dst_num, out int dst_den, long num, long den, long max) {
        int n, d, ret = ffmpeg.av_reduce(&n, &d, num, den, max);
        dst_num = n;
        dst_den = d;
        return ret != 0;
    }

    [Obsolete("Completely untested - may not work at all")]
    public static bool Reduce_SAFE(out int dst_num, out int dst_den, long num, long den, long max) {
        long a0n = 0, a0d = 1, a1n = 1, a1d = 0;
        bool sign = (num < 0) ^ (den < 0);
        long gcd = ffmpeg.av_gcd(Math.Abs(num), Math.Abs(den));
        if (gcd != 0) {
            num = Math.Abs(num) / gcd;
            den = Math.Abs(den) / gcd;
        }

        if (num <= max && den <= max) {
            a1n = num;
            a1d = den;
            den = 0;
        }

        while (den != 0) {
            long x = num / den;
            long next_den = num - den * x;
            long a2n = x * a1n + a0n;
            long a2d = x * a1d + a0d;
            if (a2n > max || a2d > max) {
                if (a1n != 0)
                    x = (max - a0n) / a1n;
                if (a1d != 0)
                    x = Math.Min(x, (max - a0d) / a1d);
                if (den * (2L * x * a1d + a0d) > num * a1d) {
                    a1n = x * a1n + a0n;
                    a1d = x * a1d + a0d;
                }

                break;
            }

            a0n = a1n;
            a0d = a1d;
            a1n = a2n;
            a1d = a2d;
            num = den;
            den = next_den;
        }

        // av_assert2(av_gcd(a1_num, a1_den) <= 1U);
        // av_assert2(a1_num <= max && a1_den <= max);
        dst_num = checked((int) (sign ? -a1n : a1n));
        dst_den = checked((int) a1d);
        return den == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Rational o) => Compare(this, o) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is Rational o && Compare(this, o) == 0;

    public override int GetHashCode() {
        return unchecked((this.num * 397) ^ this.den);
    }

    public override string ToString() {
        return $"{this.num}/{this.den}({Math.Round(this.AsDouble, 4)})";
    }

    public int CompareTo(object value) {
        if (value == null)
            return 1;
        return value is Rational r ? this.CompareTo(r) : throw new ArgumentException($"Value is not an instance of {nameof(Rational)}");
    }

    public int CompareTo(Rational o) => Compare(this.num, this.den, o.num, o.den);

    public int CompareTo(int o) => Compare(this.num, this.den, o, 1);

    public void Deconstruct(out int num, out int den) {
        num = this.num;
        den = this.den;
    }

    public static bool operator ==(Rational a, Rational b) => Compare(a, b) == 0;
    public static bool operator ==(Rational a, int b) => Compare(a.num, a.den, b, 1) == 0;
    public static bool operator !=(Rational a, Rational b) => Compare(a, b) != 0;
    public static bool operator !=(Rational a, int b) => Compare(a.num, a.den, b, 1) != 0;
    public static bool operator >(Rational a, Rational b) => Compare(a, b) == 1;
    public static bool operator >(Rational a, int b) => Compare(a.num, a.den, b, 1) == 1;
    public static bool operator >=(Rational a, Rational b) => Compare(a, b) >= 0;
    public static bool operator >=(Rational a, int b) => Compare(a.num, a.den, b, 1) >= 0;
    public static bool operator <(Rational a, Rational b) => Compare(a, b) == -1;
    public static bool operator <(Rational a, int b) => Compare(a.num, a.den, b, 1) == -1;

    public static bool operator <=(Rational a, Rational b) {
        int cmp = Compare(a, b); // compare can return int.MinValue when a or b is NaN, so <= 0 cannot be used
        return cmp == -1 || cmp == 0;
    }

    public static bool operator <=(Rational a, int b) {
        int cmp = Compare(a.num, a.den, b, 1);
        return cmp == -1 || cmp == 0;
    }

    public static Rational operator *(Rational a, Rational b) => MulInternal(a, b);
    public static Rational operator *(Rational a, int num) => a / new Rational(num);
    public static Rational operator /(Rational a, Rational b) => MulInternal(a, new Rational(b.den, b.num));
    public static Rational operator /(Rational a, int num) => a / new Rational(1, num);
    public static Rational operator +(Rational a, Rational b) => AddInternal(a, b);
    public static Rational operator +(Rational a, int num) => a / new Rational(num);
    public static Rational operator -(Rational a, Rational b) => AddInternal(a, new Rational(-b.num, b.den));
    public static Rational operator -(Rational a, int num) => a / new Rational(-num, 1);

    // Not sure if the unsafe version is faster
    // public static unsafe implicit operator AVRational(Rational r) => *(AVRational*) &r;
    // public static unsafe implicit operator Rational(AVRational r) => *(Rational*) &r;
    // public static implicit operator AVRational(Rational r) => new AVRational {num = r.num, den = r.den};
    // public static implicit operator Rational(AVRational r) => new Rational(r.den, r.num);
    public static implicit operator AVRational(Rational r) => Unsafe.As<Rational, AVRational>(ref r);
    public static implicit operator Rational(AVRational r) => Unsafe.As<AVRational, Rational>(ref r);
    public static explicit operator Rational(int num) => new Rational(num);
    public static explicit operator Rational(ulong res) => new Rational((int) (res >> 32), (int) (res & uint.MaxValue));
    public static explicit operator ulong(Rational res) => ((ulong) res.num << 32) | (uint) res.den;

    /// <summary>
    /// Compares two rationals
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>-1 when A LT B, 0 when A == B, 1 when A GT B or <see cref="int.MinValue"/> when A or B is equal to 0 / 0</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(Rational a, Rational b) => Compare(a.num, a.den, b.num, b.den);

    /// <summary>
    /// Compares two deconstructed rational. This is a C# implementation of <see cref="ffmpeg.av_cmp_q"/>
    /// </summary>
    /// <param name="numA">A's numerator</param>
    /// <param name="denA">A's denominator</param>
    /// <param name="numB">B's numerator</param>
    /// <param name="denB">B's denominator</param>
    /// <returns>-1 when A &lt; B, 1 when A &gt; B, 0 when A == B, or <see cref="int.MinValue"/> when A or B is <see cref="NaN"/> (0 / 0)</returns>
    public static int Compare(int numA, int denA, int numB, int denB) {
        long tmp = numA * (long) denB - numB * (long) denA;
        if (tmp != 0) {
            return (int) ((tmp ^ denA ^ denB) >> 63) | 1;
        }
        else if (denB != 0 && denA != 0) {
            return 0;
        }
        else if (numA != 0 && numB != 0) {
            return (numA >> 31) - (numB >> 31);
        }
        else {
            return int.MinValue;
        }
    }

    public static unsafe bool Reduce(Rational* r, long num, long den, long max = int.MaxValue) {
        return ffmpeg.av_reduce(&r->num, &r->den, num, den, max) == 1;
    }

    public static unsafe bool Reduce(out Rational r, long num, long den, long max = int.MaxValue) {
        Rational val;
        int ret = ffmpeg.av_reduce(&val.num, &val.den, num, den, max);
        r = val;
        return ret == 1;
    }

    /// <summary>
    /// Transforms an input time from timebase A to timebase B
    /// <example>
    /// Transform(new Rational(90), new Rational(30), new Rational(1000)) == new Rational(3000)
    /// </example>
    /// </summary>
    /// <param name="time">Input time</param>
    /// <param name="src">Input timebase</param>
    /// <param name="dst">Output timebase</param>
    /// <returns>The input time but in the B timebase</returns>
    public static unsafe Rational Transform(Rational time, Rational src, Rational dst) {
        // return time * (new Rational(1) / tA) * tB;
        int num, den;
        ffmpeg.av_reduce(&num, &den, src.den, src.num, int.MaxValue);
        ffmpeg.av_reduce(&num, &den, time.num * (long) num, time.den * (long) den, int.MaxValue);
        ffmpeg.av_reduce(&num, &den, num * (long) dst.num, den * (long) dst.den, int.MaxValue);
        return new Rational(num, den);
    }

    /// <summary>
    /// Transforms an input time from timebase A to timebase B
    /// <example>
    /// Transform(90, new Rational(30), new Rational(1000)) == 3000
    /// </example>
    /// </summary>
    /// <param name="time">Input time</param>
    /// <param name="src">Input timebase</param>
    /// <param name="dst">Output timebase</param>
    /// <returns>The input time but in the B timebase</returns>
    public static unsafe long Transform(long time, Rational src, Rational dst) {
        int num, den;
        ffmpeg.av_reduce(&num, &den, src.den, src.num, int.MaxValue);
        ffmpeg.av_reduce(&num, &den, time * num, den, int.MaxValue);
        ffmpeg.av_reduce(&num, &den, num * (long) dst.num, den * (long) dst.den, int.MaxValue);
        return (long) Math.Round((double) num / den);
    }

    public static unsafe Rational MulInternal(Rational b, Rational c) {
        ffmpeg.av_reduce(&b.num, &b.den, b.num * (long) c.num, b.den * (long) c.den, int.MaxValue);
        return b;
    }

    public static unsafe Rational AddInternal(Rational b, Rational c) {
        ffmpeg.av_reduce(&b.num, &b.den, b.num * (long) c.den + c.num * (long) b.den, b.den * (long) c.den, int.MaxValue);
        return b;
    }
}