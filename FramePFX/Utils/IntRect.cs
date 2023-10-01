namespace FramePFX.Utils
{
    public readonly struct IntRect
    {
        public readonly int X1;
        public readonly int Y1;
        public readonly int Width;
        public readonly int Height;

        public int X2 => this.X1 + this.Width;

        public int Y2 => this.Y1 + this.Height;

        public IntRect(int x1, int y1, int width, int height)
        {
            this.X1 = x1;
            this.Y1 = y1;
            this.Width = width;
            this.Height = height;
        }

        public static IntRect FromAABB(int x1, int y1, int x2, int y2)
        {
            return new IntRect(x1, y1, x2 - x1, y2 - y1);
        }
    }
}