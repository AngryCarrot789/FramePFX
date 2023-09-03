using System;

namespace FramePFX.Utils {
    /// <summary>
    /// Represents an immutable slice of time in frames (similar to <see cref="TimeSpan"/>) and some utility functions.
    /// <para>
    /// This structure is 16 bytes; <see cref="Begin"/> and <see cref="Duration"/> fields
    /// </para>
    /// </summary>
    public readonly struct FrameSpan : IEquatable<FrameSpan> {
        public static readonly FrameSpan Empty = new FrameSpan(0, 0);

        /// <summary>
        /// The beginning of this span (inclusive index). This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public readonly long Begin;

        /// <summary>
        /// The duration (in frames) of this span. This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public readonly long Duration;

        /// <summary>
        /// A calculated end-index (exclusive) for this span. This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public long EndIndex => this.Begin + this.Duration;

        public FrameSpan(long begin, long duration) {
            this.Begin = begin;
            this.Duration = duration;
        }

        public static FrameSpan FromDuration(long begin, long duration) {
            return new FrameSpan(begin, duration);
        }

        public static FrameSpan FromIndex(long begin, long endIndex) {
            return new FrameSpan(begin, endIndex - begin);
        }

        public FrameSpan Expand(long expand) {
            return new FrameSpan(this.Begin - expand, this.Duration + expand + expand);
        }

        public FrameSpan Contract(long contract) {
            return new FrameSpan(this.Begin + contract, this.Duration - contract - contract);
        }

        /// <summary>
        /// Returns a new span where the <see cref="Begin"/> property is offset by the given amount, and <see cref="Duration"/> is untouched
        /// </summary>
        public FrameSpan Offset(long frames) {
            return new FrameSpan(this.Begin + frames, this.Duration);
        }

        /// <summary>
        /// Returns a new span where the <see cref="Duration"/> property is offset by the given amount, and <see cref="Begin"/> is untouched
        /// </summary>
        public FrameSpan OffsetDuration(long frames) {
            return new FrameSpan(this.Begin, this.Duration + frames);
        }

        public FrameSpan Offset(long offsetBegin, long offsetDuration) {
            return new FrameSpan(this.Begin + offsetBegin, this.Duration + offsetDuration);
        }

        public FrameSpan WithBegin(long newBegin) {
            return new FrameSpan(newBegin, this.Duration);
        }

        public FrameSpan WithDuration(long newDuration) {
            return new FrameSpan(this.Begin, newDuration);
        }

        // The clamped versions of the below 4 functions are useful for resizing clips by dragging the left and right handles

        /// <summary>
        /// Returns a new frame span, where the <see cref="Begin"/> is locked in place, and the <see cref="EndIndex"/> is modified
        /// </summary>
        /// <param name="value">The new end index value. This value is trusted to be non-negative</param>
        /// <returns>A new frame span</returns>
        /// <exception cref="ArgumentOutOfRangeException">Input value is less than the begin value</exception>
        public FrameSpan WithEndIndex(long value) {
            if (value < this.Begin) {
                throw new ArgumentOutOfRangeException(nameof(value), $"Value cannot be smaller than the begin index ({value} < {this.Begin})");
            }

            return new FrameSpan(this.Begin, value - this.Begin);
        }

        /// <summary>
        /// Same as <see cref="WithEndIndex"/>, but instead of throwing, the span is clamped to empty
        /// </summary>
        /// <param name="value">The new end index value. This value is trusted to be non-negative</param>
        /// <param name="upperLimit">The upper limit for the end index. By default, this is <see cref="long.MaxValue"/> meaning effectively no upper limit</param>
        /// <returns>A new frame span, or empty when the value is less than or equal to the begin value</returns>
        public FrameSpan WithEndIndexClamped(long value, long upperLimit = long.MaxValue) {
            if (value > this.Begin) {
                if (value > upperLimit)
                    value = upperLimit;
                return new FrameSpan(this.Begin, value - this.Begin);
            }

            return Empty;
        }

        /// <summary>
        /// Returns a new frame span, where the <see cref="EndIndex"/> is locked in place, and the <see cref="Begin"/> is modified
        /// </summary>
        /// <param name="value">The new begin 'index'. This value is trusted to be non-negative</param>
        /// <returns>A new frame span</returns>
        /// <exception cref="ArgumentOutOfRangeException">Input value is greater than the end index</exception>
        public FrameSpan WithBeginIndex(long value) {
            long endIndex = this.Begin + this.Duration;
            if (value > endIndex) {
                throw new ArgumentOutOfRangeException(nameof(value), $"New begin value cannot exceed the end index ({value} > {endIndex})");
            }

            return new FrameSpan(value, this.Duration - (value - this.Begin));
        }

        /// <summary>
        /// Same as <see cref="WithBeginIndex"/>, but instead of throwing, the span is clamped to empty
        /// </summary>
        /// <param name="value">The new end index value. This value is trusted to be non-negative</param>
        /// <param name="lowerLimit">The lower limit for the begin 'index'. By default, this is 0</param>
        /// <returns>A new frame span, or empty when the value is greater than or equal to the end index value</returns>
        public FrameSpan WithBeginIndexClamped(long value, long lowerLimit = 0L) {
            long endIndex = this.Begin + this.Duration;
            if (value < lowerLimit)
                value = lowerLimit;
            return value < endIndex ? new FrameSpan(value, this.Duration - (value - this.Begin)) : Empty;
        }

        /// <summary>
        /// Returns a frame span whose <see cref="Begin"/> and <see cref="Duration"/> are non-negative.
        /// If none of them are negative, the current instance is returned
        /// </summary>
        /// <returns></returns>
        public FrameSpan Abs() {
            if (this.Begin >= 0 && this.Duration >= 0) {
                return this;
            }
            else {
                return new FrameSpan(Math.Abs(this.Begin), Math.Abs(this.Duration));
            }
        }

        /// <summary>
        /// Returns a new span where the smallest <see cref="Begin"/> and largest <see cref="EndIndex"/> are returned
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public FrameSpan MinMax(FrameSpan other) {
            long minBegin = Math.Min(this.Begin, other.Begin);
            long maxEnd = Math.Max(this.Begin + this.Duration, other.Begin + other.Duration);
            return new FrameSpan(minBegin, maxEnd - minBegin);
        }

        public bool Intersects(long frame) {
            return frame >= this.Begin && frame < this.EndIndex;
        }

        public bool Intersects(FrameSpan span) {
            return Intersects(this, span);
        }

        public static bool Intersects(FrameSpan a, FrameSpan b) {
            // no idea if this works both ways... CBA to test lolol
            return a.Begin < b.EndIndex && a.EndIndex > b.Begin;
        }

        public static bool operator ==(in FrameSpan a, in FrameSpan b) {
            return a.Begin == b.Begin && a.Duration == b.Duration;
        }

        public static bool operator !=(in FrameSpan a, in FrameSpan b) {
            return a.Begin != b.Begin || a.Duration != b.Duration;
        }

        public override string ToString() {
            return $"{this.Begin} -> {this.EndIndex} ({this.Duration})";
        }

        public bool Equals(FrameSpan other) {
            return this.Begin == other.Begin && this.Duration == other.Duration;
        }

        public override bool Equals(object obj) {
            return obj is FrameSpan other && this == other;
        }

        public override int GetHashCode() {
            unchecked {
                return (this.Begin.GetHashCode() * 397) ^ this.Duration.GetHashCode();
            }
        }
    }
}