using System;

namespace FramePFX.Core.Utils {
    public readonly struct FrameSpan : IEquatable<FrameSpan> {
        public static readonly FrameSpan Empty = new FrameSpan(0, 0);
        private readonly long begin;
        private readonly long duration;

        /// <summary>
        /// The beginning of this span (inclusive index). This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public long Begin => this.begin;

        /// <summary>
        /// The duration (in frames) of this span. This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public long Duration => this.duration;

        /// <summary>
        /// A calculated end-index (exclusive) for this span. This value may be negative (which isn't a valid span value, but is allowed anyway)
        /// </summary>
        public long EndIndex {
            get => this.begin + this.duration;
        }

        public FrameSpan(long begin, long duration) {
            this.begin = begin;
            this.duration = duration;
        }

        public static FrameSpan FromDuration(long begin, long duration) {
            return new FrameSpan(begin, duration);
        }

        public static FrameSpan FromIndex(long begin, long endIndex) {
            return new FrameSpan(begin, endIndex - begin);
        }

        public FrameSpan Expand(long expand) {
            return new FrameSpan(this.begin - expand, this.duration + expand + expand);
        }

        public FrameSpan Contract(long contract) {
            return new FrameSpan(this.begin + contract, this.duration - contract - contract);
        }

        /// <summary>
        /// Returns a new span where the <see cref="Begin"/> property is offset by the given amount, and <see cref="Duration"/> is untouched
        /// </summary>
        public FrameSpan Offset(long frames) {
            return new FrameSpan(this.begin + frames, this.duration);
        }

        /// <summary>
        /// Returns a new span where the <see cref="Duration"/> property is offset by the given amount, and <see cref="Begin"/> is untouched
        /// </summary>
        public FrameSpan OffsetDuration(long frames) {
            return new FrameSpan(this.begin, this.duration + frames);
        }

        public FrameSpan Offset(long offsetBegin, long offsetDuration) {
            return new FrameSpan(this.begin + offsetBegin, this.duration + offsetDuration);
        }

        public FrameSpan SetBegin(long newBegin) {
            return new FrameSpan(newBegin, this.duration);
        }

        public FrameSpan SetDuration(long newDuration) {
            return new FrameSpan(this.begin, newDuration);
        }

        public FrameSpan SetEndIndex(long newEndIndex) {
            if (newEndIndex < this.begin) {
                throw new ArgumentOutOfRangeException(nameof(newEndIndex), $"Value cannot be smaller than the begin index ({newEndIndex} < {this.begin})");
            }

            return new FrameSpan(this.begin, newEndIndex - this.begin);
        }

        /// <summary>
        /// Returns a frame span whose <see cref="Begin"/> and <see cref="Duration"/> are non-negative.
        /// If none of them are negative, the current instance is returned
        /// </summary>
        /// <returns></returns>
        public FrameSpan Abs() {
            if (this.begin >= 0 && this.duration >= 0) {
                return this;
            }
            else {
                return new FrameSpan(Math.Abs(this.begin), Math.Abs(this.duration));
            }
        }

        public bool Intersects(long frame) {
            return frame >= this.begin && frame < this.EndIndex;
        }

        public bool Intersects(in FrameSpan span) {
            return Intersects(this, span);
        }

        public static bool Intersects(in FrameSpan a, in FrameSpan b) {
            // no idea if this works both ways... CBA to test lolol
            return a.begin < b.EndIndex && a.EndIndex > b.begin;
        }

        public static bool operator ==(in FrameSpan a, in FrameSpan b) {
            return a.begin == b.begin && a.duration == b.duration;
        }

        public static bool operator !=(in FrameSpan a, in FrameSpan b) {
            return a.begin != b.begin || a.duration != b.duration;
        }

        public override string ToString() {
            return $"{this.begin} -> {this.EndIndex} ({this.duration})";
        }

        public bool Equals(FrameSpan other) {
            return this.begin == other.begin && this.duration == other.duration;
        }

        public override bool Equals(object obj) {
            return obj is FrameSpan other && this == other;
        }

        public override int GetHashCode() {
            unchecked {
                return (this.begin.GetHashCode() * 397) ^ this.duration.GetHashCode();
            }
        }
    }
}