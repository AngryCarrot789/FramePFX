using System;

namespace FramePFX.Core.Editor {
    public readonly struct Rational {
        public static readonly Rational Fps2997 = new Rational(30000, 1001);
        public static readonly Rational Fps30 = new Rational(30, 1);
        public static readonly Rational Fps5994 = new Rational(60000, 1001);
        public static readonly Rational Fps60 = new Rational(60, 1);

        /// <summary>
        /// The numerator, e.g. 60000
        /// </summary>
        public readonly int num;

        /// <summary>
        /// The denominator, e.g. 1001
        /// </summary>
        public readonly int den;

        /// <summary>
        /// A value of the number of frames per second, as a <see cref="double"/>
        /// </summary>
        public double FPS => (double) this.num / this.den;

        public Rational(int numerator, int denominator) {
            this.num = numerator;
            this.den = denominator;
        }

        public static Rational FromFPS(double fps, int denominator) {
            return new Rational((int) Math.Floor(fps * denominator), denominator);
        }

        public static bool operator ==(Rational a, Rational b) {
            return a.num == b.num && a.den == b.den;
        }

        public static bool operator !=(Rational a, Rational b) {
            return a.num != b.num || a.den != b.den;
        }

        public bool Equals(Rational other) {
            return this.num == other.num && this.den == other.den;
        }

        public override bool Equals(object obj) {
            return obj is Rational other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.num * 397) ^ this.den;
            }
        }
    }
}