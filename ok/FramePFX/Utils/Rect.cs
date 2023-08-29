using System.Numerics;

namespace FramePFX.Utils {
    public readonly struct Rect {
        public static readonly Rect Empty = new Rect();

        public readonly float X1;
        public readonly float Y1;
        public readonly float Width;
        public readonly float Height;

        public float X2 => this.X1 + this.Width;

        public float Y2 => this.Y1 + this.Height;

        public Vector2 Size => new Vector2(this.Width, this.Height);

        public Rect(float x1, float y1, float width, float height) {
            this.X1 = x1;
            this.Y1 = y1;
            this.Width = width;
            this.Height = height;
        }

        public Rect(Vector2 pos, float width, float height) {
            this.X1 = pos.X;
            this.Y1 = pos.Y;
            this.Width = width;
            this.Height = height;
        }

        public Rect(float x, float y, Vector2 size) {
            this.X1 = x;
            this.Y1 = y;
            this.Width = size.X;
            this.Height = size.Y;
        }

        public Rect(Vector2 pos, Vector2 size) {
            this.X1 = pos.X;
            this.Y1 = pos.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }

        public static Rect FromAABB(float x1, float y1, float x2, float y2) {
            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }
    }
}