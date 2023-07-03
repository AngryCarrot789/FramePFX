using FFmpeg.AutoGen;

namespace FramePFX.Core.Editor {
    /// <summary>
    /// A helper class for dealing with time
    /// </summary>
    public static class Timecode {
        public static readonly Rational Fps23976 = new Rational(24000, 1001);
        public static readonly Rational Fps2997 = new Rational(30000, 1001);
        public static readonly Rational Fps30 = new Rational(30, 1);
        public static readonly Rational Fps5994 = new Rational(60000, 1001);
        public static readonly Rational Fps60 = new Rational(60, 1);

        /// <summary>
        /// Converts a timestamp into a time duration, in the given timebase. This is the same as multiplying the time base by a new rational where the numerator is timestamp and the denominator is 1
        /// <para>
        /// Example:
        /// <code>
        /// TimestampToTime(new Rational(30000, 1001), 3) == new Rational(90000, 1001)
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="tbase">Timebase</param>
        /// <returns></returns>
        public static unsafe Rational TimeStampToDuration(this Rational tbase, long timestamp) {
            ffmpeg.av_reduce(&tbase.num, &tbase.den, tbase.num * timestamp, tbase.den, int.MaxValue);
            return tbase;
        }

        public static unsafe Rational FromFrame(Rational tb, long frame) {
            ffmpeg.av_reduce(&tb.num, &tb.den, tb.num * frame, tb.den * 1L, int.MaxValue);
            return tb;
        }

        public static Rational PixelToTime(double pixel, double scale) {
            double unscaled = pixel / scale;
            return Rational.FromDouble(unscaled);
        }

        public static Rational TimeBaseToMediaTime(Rational r, double speed) {
            return r * Rational.FromDouble(speed);
        }
    }
}