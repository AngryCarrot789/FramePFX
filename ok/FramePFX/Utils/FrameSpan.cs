using System;

namespace FramePFX.Utils {
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

        public FrameSpan WithEndIndex(long newEndIndex) {
            if (newEndIndex < this.Begin) {
                throw new ArgumentOutOfRangeException(nameof(newEndIndex), $"Value cannot be smaller than the begin index ({newEndIndex} < {this.Begin})");
            }

            return new FrameSpan(this.Begin, newEndIndex - this.Begin);
        }

        public FrameSpan WithBeginIndex(long newBeginIndex) {
            long begin = this.Begin, dur = this.Duration, end = begin + dur;

            if (newBeginIndex > end) {
                throw new ArgumentOutOfRangeException(nameof(newBeginIndex), $"Value cannot be greater than the end index ({newBeginIndex} > {end})");
            }

            return new FrameSpan(newBeginIndex, dur - (newBeginIndex - begin));
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

        public bool Intersects(in FrameSpan span) {
            return Intersects(this, span);
        }

        public static bool Intersects(in FrameSpan a, in FrameSpan b) {
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