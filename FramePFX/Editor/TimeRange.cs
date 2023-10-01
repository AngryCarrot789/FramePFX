namespace FramePFX.Editor
{
    public readonly struct TimeRange
    {
        public readonly Rational Start;
        public readonly Rational End;

        public Rational Length => this.End - this.Start;

        public TimeRange(Rational start, Rational end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}