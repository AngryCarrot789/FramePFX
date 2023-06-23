using System;
using FFmpeg.AutoGen;

namespace FramePFX.Core.Editor {
    public readonly struct Rational {
        public static readonly Rational Fps2997 = new Rational(1001, 30000);
        public static readonly Rational Fps30 = new Rational(1, 30);
        public static readonly Rational Fps5994 = new Rational(1001, 60000);
        public static readonly Rational Fps60 = new Rational(1, 60);

        /// <summary>
        /// The numerator, e.g. 60000
        /// </summary>
        public readonly int num;

        /// <summary>
        /// The denominator, e.g. 1001
        /// </summary>
        public readonly int den;

        /// <summary>
        /// av_q2d, not a typical readable FPS quantity
        /// </summary>
        public double AVFrameRate => ffmpeg.av_q2d(this);

        /// <summary>
        /// Readable FPS value
        /// </summary>
        public double ActualFPS => (double) this.den / this.num;

        public Rational(int numerator, int denominator) {
            this.num = numerator;
            this.den = denominator;
        }

        public static bool operator ==(Rational a, Rational b) {
            return a.num == b.num && a.den == b.den;
        }

        public static bool operator !=(Rational a, Rational b) {
            return a.num != b.num || a.den != b.den;
        }

        public static implicit operator AVRational(Rational r) => new AVRational() {den = r.den, num = r.num};

        public static explicit operator Rational(AVRational r) => new Rational(r.num, r.den);

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