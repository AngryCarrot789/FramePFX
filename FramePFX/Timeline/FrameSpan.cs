using System;

namespace FramePFX.Core.Timeline {
    public readonly struct FrameSpan {
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

        public override string ToString() {
            return $"{this.Begin} -> {this.EndIndex} ({this.Duration})";
        }
    }
}