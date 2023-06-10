using System;

namespace FramePFX.Core.Utils {
    public readonly struct FrameSpan : IEquatable<FrameSpan> {
        public static readonly FrameSpan Empty = new FrameSpan(0, 0);

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
            return new FrameSpan(this.Begin - expand, this.Duration + expand);
        }

        public FrameSpan Contract(long contract) {
            return new FrameSpan(this.Begin + contract, this.Duration + contract);
        }

        /// <summary>
        /// Returns a new span whose <see cref="Begin"/> is offset by the given amount
        /// </summary>
        public FrameSpan OffsetBegin(long frames) {
            return new FrameSpan(this.Begin + frames, this.Duration);
        }

        public FrameSpan OffsetDuration(long frames) {
            return new FrameSpan(this.Begin, this.Duration + frames);
        }

        public FrameSpan Offset(long beginOffset, long durationOffset) {
            return new FrameSpan(this.Begin + beginOffset, this.Duration + durationOffset);
        }

        public FrameSpan SetBegin(long begin) {
            return new FrameSpan(begin, this.Duration);
        }

        public FrameSpan SetDuration(long duration) {
            return new FrameSpan(this.Begin, duration);
        }

        public FrameSpan SetEndIndex(long endIndex) {
            if (endIndex < this.Begin) {
                throw new ArgumentOutOfRangeException(nameof(endIndex), $"Value cannot be smaller than the begin index ({endIndex} < {this.Begin})");
            }

            return new FrameSpan(this.Begin, endIndex - this.Begin);
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
            return obj is FrameSpan other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.Begin.GetHashCode() * 397) ^ this.Duration.GetHashCode();
            }
        }
    }
}