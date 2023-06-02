namespace FrameControlEx.Core.Utils {
    public readonly struct TextRange {
        public int Index { get; }

        public int Length { get; }

        public int EndIndex => this.Index + this.Length;

        public TextRange(int index, int length) {
            this.Index = index;
            this.Length = length;
        }

        public string GetString(string input) {
            return input.Substring(this.Index, this.Length);
        }
    }
}