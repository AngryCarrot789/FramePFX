using System;

namespace FramePFX.Timeline {
    public readonly struct FrameSpan {
        public long Begin { get; }
        public long Duration { get; }
        public long EndIndex => this.Begin + this.Duration;

        public FrameSpan(long begin, long duration) {
            this.Begin = Math.Max(begin, 0);
            this.Duration = Math.Max(duration, 0);
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

        public bool Intersects(long frame) {
            return frame >= this.Begin && frame < this.EndIndex;
        }

        public bool Intersects(in FrameSpan span) {
            return Intersects(this, span);
        }

        public static bool Intersects(in FrameSpan a, in FrameSpan b) {
            return a.Begin < b.EndIndex && a.EndIndex > b.Begin;
        }

        public override string ToString() {
            return $"{this.Begin} -> {this.EndIndex} ({this.Duration})";
        }
    }
}