namespace FramePFX.Utils.Numerics
{
    public struct Vector2i
    {
        public static readonly Vector2i Empty = default;

        public int X;
        public int Y;

        public Vector2i(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2i(Vector2i vec)
        {
            this.X = vec.X;
            this.Y = vec.Y;
        }

        public bool IsEmpty => this == Vector2i.Empty;

        public override string ToString() => $"Vector2i({this.X.ToString()}, {this.Y.ToString()})";

        public static Vector2i operator +(Vector2i a, Vector2i b) => new Vector2i(a.X + b.X, a.Y + b.Y);

        public static Vector2i operator -(Vector2i a, Vector2i b) => new Vector2i(a.X - b.X, a.Y - b.Y);

        public bool Equals(Vector2i obj) => this.X == obj.X && this.Y == obj.Y;

        public override bool Equals(object obj) => obj is Vector2i skVector2i && this.Equals(skVector2i);

        public static bool operator ==(Vector2i left, Vector2i right) => left.Equals(right);

        public static bool operator !=(Vector2i left, Vector2i right) => !left.Equals(right);

        public override int GetHashCode() => (this.X * 397) ^ this.Y;
    }
}