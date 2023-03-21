namespace FramePFX.Timeline {
    public readonly struct FrameDuration {
        public long Begin { get; }
        public long Duration { get; }
        public long EndIndex => this.Begin + this.Duration;

        public FrameDuration(long begin, long duration) {
            this.Begin = begin;
            this.Duration = duration;
        }

        public bool Intersects(long frame) {
            return frame >= this.Begin && frame < this.EndIndex;
        }

        public bool Intersects(in FrameDuration duration) {
            return Intersects(this, duration);
        }

        public static bool Intersects(in FrameDuration a, in FrameDuration b) {
            return a.Begin < b.EndIndex && a.EndIndex > b.Begin;
        }
    }
}