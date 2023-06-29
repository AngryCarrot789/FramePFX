using System;
using System.Runtime.CompilerServices;
using FFmpeg.AutoGen;

namespace FramePFX.Core.Editor {
    /// <summary>
    /// Rational number calculation.
    /// <para>
    /// While rational numbers can be expressed as floating-point numbers, the
    /// conversion process is a lossy one, so are floating-point operations.
    /// This set of rational number utilities serves as a generic
    /// interface for manipulating rational numbers as pairs of numerators and
    /// denominators.
    /// </para>
    /// </summary>
    public readonly struct Rational : IComparable<Rational> {
        public static readonly Rational Fps2997 = new Rational(30000, 1001);
        public static readonly Rational Fps30 = new Rational(30, 1);
        public static readonly Rational Fps5994 = new Rational(60000, 1001);
        public static readonly Rational Fps60 = new Rational(60, 1);

        /// <summary>
        /// The numerator, e.g. 30000
        /// </summary>
        public readonly int num;

        /// <summary>
        /// The denominator, e.g. 1001
        /// </summary>
        public readonly int den;

        /// <summary>
        /// Frame rate as double. <see cref="ffmpeg.av_q2d"/>
        /// </summary>
        public double AsDouble => this.num / (double) this.den;

        public Rational(int numerator, int denominator) {
            this.num = numerator;
            this.den = denominator;
        }

        public static Rational FromDouble(double d, int max) {
            const double LOG2 = 0.69314718055994530941723212145817656807550013436025;
            if (double.IsNaN(d))
                return new Rational(0, 0);
            if (double.IsInfinity(d))
                return new Rational(0, d < 0 ? -1 : 1);
            int exp = Math.Max((int) (Math.Log(Math.Abs(d) + 1e-20) / LOG2), 0);
            long den = 1L << (61 - exp);
            int out_num, out_den;
            unsafe {
                ffmpeg.av_reduce(&out_num, &out_den, (long) (d * den + 0.5), den, max);
            }

            return new Rational(out_den, out_num);
        }

        public static bool operator ==(Rational a, Rational b) => Compare(a, b) == 0;
        public static bool operator !=(Rational a, Rational b) => Compare(a, b) != 0;
        public static bool operator >(Rational a, Rational b) => Compare(a, b) > 0;
        public static bool operator >=(Rational a, Rational b) => Compare(a, b) >= 0;
        public static bool operator <(Rational a, Rational b) => Compare(a, b) < 0;
        public static bool operator <=(Rational a, Rational b) => Compare(a, b) <= 0;

        public static Rational operator *(Rational a, Rational b) => Mul(a, b);
        public static Rational operator /(Rational a, Rational b) => Div(a, b);
        public static Rational operator +(Rational a, Rational b) => Add(a, b);
        public static Rational operator -(Rational a, Rational b) => Sub(a, b);

        public static implicit operator AVRational(Rational r) => new AVRational() {den = r.den, num = r.num};

        public static implicit operator Rational(AVRational r) => new Rational(r.den, r.num);

        public void Deconstruct(out int num, out int den) {
            num = this.num;
            den = this.den;
        }

        public bool Equals(Rational o) => Compare(this, o) == 0;

        public override bool Equals(object obj) {
            return obj is Rational other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.num * 397) ^ this.den;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Rational o) => Compare(this.num, this.den, o.num, o.den);

        /// <summary>
        /// Compares two rationals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>-1 when A LT B, 0 when A == B, 1 when A GT B or <see cref="int.MinValue"/> when A or B is equal to 0 / 0</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(Rational a, Rational b) => Compare(a.num, a.den, b.num, b.den);

        /// <summary>
        /// Compares two deconstructed rational
        /// </summary>
        /// <param name="a_num">A's numerator</param>
        /// <param name="a_den">A's denominator</param>
        /// <param name="b_num">B's numerator</param>
        /// <param name="b_den">B's denominator</param>
        /// <returns>-1 when A LT B, 0 when A == B, 1 when A GT B or <see cref="int.MinValue"/> when A or B is equal to 0 / 0</returns>
        public static int Compare(int a_num, int a_den, int b_num, int b_den) {
            long tmp = a_num * (long) b_den - b_num * (long) a_den;
            if (tmp != 0) {
                return (int) ((tmp ^ a_den ^ b_den) >> 63) | 1;
            }
            else if (b_den != 0 && a_den != 0) {
                return 0;
            }
            else if (a_num != 0 && b_num != 0) {
                return (a_num >> 31) - (b_num >> 31);
            }
            else {
                return int.MinValue;
            }
        }

        public static unsafe bool Reduce(out Rational rational, long num, long den, long max) {
            int out_num, out_den;
            int ret = ffmpeg.av_reduce(&out_num, &out_den, num, den, max);
            rational = new Rational(out_num, out_den);
            return ret != 0;
        }

        public static unsafe Rational Mul(Rational b, Rational c) {
            ffmpeg.av_reduce(&b.num, &b.den, b.num * (long) c.num, b.den * (long) c.den, int.MaxValue);
            return b;
        }

        public static Rational Div(Rational b, Rational c) => Mul(b, new Rational(c.den, c.num));

        public static unsafe Rational Add(Rational b, Rational c) {
            ffmpeg.av_reduce(&b.num, &b.den, b.num * (long) c.den + c.num * (long) b.den, b.den * (long) c.den, int.MaxValue);
            return b;
        }

        public static Rational Sub(Rational b, Rational c) => Add(b, new Rational(-c.num, c.den));
    }
}