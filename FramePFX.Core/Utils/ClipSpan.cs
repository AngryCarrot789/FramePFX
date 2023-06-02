using System;

namespace FramePFX.Core.Utils {
    public readonly struct ClipSpan : IEquatable<ClipSpan> {
        public static readonly ClipSpan Empty = new ClipSpan(0, 0);

        /// <summary>
        /// The beginning of this span (inclusive index). This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public long Begin { get; }

        /// <summary>
        /// The duration (in frames) of this span. This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public long Duration { get; }

        /// <summary>
        /// A calculated end-index (exclusive) for this span. This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public long EndIndex {
            get => this.Begin + this.Duration;
        }

        /// <summary>
        /// Whether this span's duration is zero or not. An empty span has no frames, irregardless of the begin frame
        /// </summary>
        public bool IsEmpty => this.Duration == 0;

        public ClipSpan(long begin, long duration) {
            this.Begin = begin;
            this.Duration = duration;
        }

        public static ClipSpan FromDuration(long begin, long duration) {
            return new ClipSpan(begin, duration);
        }

        public static ClipSpan FromIndex(long begin, long endIndex) {
            return new ClipSpan(begin, endIndex - begin);
        }

        public ClipSpan Expand(long expand) {
            return new ClipSpan(this.Begin - expand, this.Duration + expand);
        }

        public ClipSpan Contract(long contract) {
            return new ClipSpan(this.Begin + contract, this.Duration + contract);
        }

        /// <summary>
        /// Returns a new span whose <see cref="Begin"/> is offset by the given amount
        /// </summary>
        public ClipSpan OffsetBegin(long frames) {
            return new ClipSpan(this.Begin + frames, this.Duration);
        }

        public ClipSpan OffsetDuration(long frames) {
            return new ClipSpan(this.Begin, this.Duration + frames);
        }

        public ClipSpan Offset(long beginOffset, long durationOffset) {
            return new ClipSpan(this.Begin + beginOffset, this.Duration + durationOffset);
        }

        public ClipSpan SetBegin(long begin) {
            return new ClipSpan(begin, this.Duration);
        }

        public ClipSpan SetDuration(long duration) {
            return new ClipSpan(this.Begin, duration);
        }

        public ClipSpan SetEndIndex(long endIndex) {
            if (endIndex < this.Begin) {
                throw new ArgumentOutOfRangeException(nameof(endIndex), $"Value cannot be smaller than the begin index ({endIndex} < {this.Begin})");
            }

            return new ClipSpan(this.Begin, endIndex - this.Begin);
        }

        /// <summary>
        /// Returns a frame span whose <see cref="Begin"/> and <see cref="Duration"/> are non-negative.
        /// If none of them are negative, the current instance is returned
        /// </summary>
        /// <returns></returns>
        public ClipSpan Abs() {
            if (this.Begin >= 0 && this.Duration >= 0) {
                return this;
            }
            else {
                return new ClipSpan(Math.Abs(this.Begin), Math.Abs(this.Duration));
            }
        }

        public bool Intersects(long frame) {
            return frame >= this.Begin && frame < this.EndIndex;
        }

        public bool Intersects(in ClipSpan span) {
            return Intersects(this, span);
        }

        public static bool Intersects(in ClipSpan a, in ClipSpan b) {
            // no idea if this works both ways... CBA to test lolol
            return a.Begin < b.EndIndex && a.EndIndex > b.Begin;
        }

        public override string ToString() {
            return $"{this.Begin} -> {this.EndIndex} ({this.Duration})";
        }

        public bool Equals(ClipSpan other) {
            return this.Begin == other.Begin && this.Duration == other.Duration;
        }

        public override bool Equals(object obj) {
            return obj is ClipSpan other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.Begin.GetHashCode() * 397) ^ this.Duration.GetHashCode();
            }
        }
    }
}