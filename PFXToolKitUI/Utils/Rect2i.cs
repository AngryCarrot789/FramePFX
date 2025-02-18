//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Numerics;

namespace PFXToolKitUI.Utils;

/// <summary>
/// A struct that stores a width and height property, as ints
/// </summary>
public readonly struct Rect2i : IEqualityComparer<Rect2i>, IEquatable<Rect2i>, IComparable<Rect2i> {
    /// <summary>
    /// A resolution whose width and height is zero
    /// </summary>
    public static readonly Rect2i Empty = new Rect2i(0, 0);

    /// <summary>
    /// The resolution's width
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The resolution's height
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Whether this resolution's width and height is zero or not
    /// </summary>
    public bool IsEmpty => this.Width == 0 && this.Height == 0;

    public Rect2i(int width, int height) {
        this.Width = width;
        this.Height = height;
    }

    public Rect2i WithWidth(int width) {
        return new Rect2i(width, this.Height);
    }

    public Rect2i WithHeight(int height) {
        return new Rect2i(this.Width, height);
    }

    /// <summary>
    /// Creates a new resolution using the given height and a calculated width based on the aspect ratio of this instance
    /// </summary>
    /// <param name="height">The resolution height</param>
    /// <returns>A new resolution</returns>
    public Rect2i ResizeToHeight(int height) {
        return new Rect2i((int) ((double) height / this.Height * this.Width), height);
    }

    /// <summary>
    /// Creates a new resolution using the given width and a calculated height based on the aspect ratio of this instance
    /// </summary>
    /// <param name="width">The width of this resolution</param>
    /// <returns>A new resolution</returns>
    public Rect2i ResizeToWidth(int width) {
        return new Rect2i(width, (int) ((double) width / this.Width * this.Height));
    }

    public static Rect2i Floor(double width, double height) {
        return new Rect2i((int) Math.Floor(width), (int) Math.Floor(height));
    }

    public static Rect2i Ceiling(double width, double height) {
        return new Rect2i((int) Math.Ceiling(width), (int) Math.Ceiling(height));
    }

    public static Rect2i Round(double width, double height) {
        return new Rect2i((int) Math.Round(width), (int) Math.Round(height));
    }

    public static explicit operator Vector2(Rect2i res) => new Vector2(res.Width, res.Height);

    public static explicit operator Rect2i(Vector2 res) => new Rect2i((int) Math.Floor(res.X), (int) Math.Floor(res.Y));

    public static explicit operator Rect2i(ulong res) => new Rect2i((int) (res >> 32), (int) (res & uint.MaxValue));

    public static explicit operator ulong(Rect2i res) => ((ulong) res.Width << 32) | (uint) res.Height;

    public static bool operator ==(Rect2i a, Rect2i b) => a.Width == b.Width && a.Height == b.Height;
    public static bool operator !=(Rect2i a, Rect2i b) => a.Width != b.Width || a.Height != b.Height;

    public bool Equals(Rect2i other) => this == other;

    public override bool Equals(object obj) => obj is Rect2i res && res == this;

    public override int GetHashCode() => unchecked((this.Width * 397) ^ this.Height);

    public int CompareTo(Rect2i other) {
        int cmp = this.Width.CompareTo(other.Width);
        if (cmp == 0)
            cmp = this.Height.CompareTo(other.Height);
        return cmp;
    }

    public bool Equals(Rect2i x, Rect2i y) => x == this;

    public int GetHashCode(Rect2i obj) => obj.GetHashCode();

    public override string ToString() {
        return $"{this.Width}x{this.Height}";
    }
}