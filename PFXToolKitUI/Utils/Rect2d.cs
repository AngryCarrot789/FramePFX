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
/// A struct that stores a width and height property, as doubles
/// </summary>
public readonly struct Rect2d : IEqualityComparer<Rect2d>, IEquatable<Rect2d>, IComparable<Rect2d> {
    /// <summary>
    /// A resolution whose width and height is zero
    /// </summary>
    public static readonly Rect2d Empty = new Rect2d(0, 0);

    /// <summary>
    /// The resolution's width
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// The resolution's height
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// Whether this resolution's width and height is zero or not
    /// </summary>
    public bool IsEmpty => this.Width == 0 && this.Height == 0;

    public Rect2d(double width, double height) {
        this.Width = width;
        this.Height = height;
    }

    public Rect2d WithWidth(double width) {
        return new Rect2d(width, this.Height);
    }

    public Rect2d WithHeight(double height) {
        return new Rect2d(this.Width, height);
    }

    /// <summary>
    /// Creates a new resolution using the given height and a calculated width based on the aspect ratio of this instance
    /// </summary>
    /// <param name="height">The resolution height</param>
    /// <returns>A new resolution</returns>
    public Rect2d ResizeToHeight(double height) {
        return new Rect2d(height / this.Height * this.Width, height);
    }

    /// <summary>
    /// Creates a new resolution using the given width and a calculated height based on the aspect ratio of this instance
    /// </summary>
    /// <param name="width">The width of this resolution</param>
    /// <returns>A new resolution</returns>
    public Rect2d ResizeToWidth(double width) {
        return new Rect2d(width, width / this.Width * this.Height);
    }

    public Rect2d Floor() {
        return new Rect2d(Math.Floor(this.Width), Math.Floor(this.Height));
    }

    public Rect2d Ceiling() {
        return new Rect2d(Math.Ceiling(this.Width), Math.Ceiling(this.Height));
    }

    public static Rect2d Floor(double width, double height) {
        return new Rect2d(Math.Floor(width), Math.Floor(height));
    }

    public static Rect2d Ceiling(double width, double height) {
        return new Rect2d(Math.Ceiling(width), Math.Ceiling(height));
    }

    public static Rect2d Round(double width, double height) {
        return new Rect2d(Math.Round(width), Math.Round(height));
    }

    public static explicit operator Vector2(Rect2d res) => new Vector2((float) res.Width, (float) res.Height);

    public static explicit operator Rect2d(Vector2 res) => new Rect2d(Math.Floor(res.X), Math.Floor(res.Y));

    public static bool operator ==(Rect2d a, Rect2d b) => Maths.Equals(a.Width, b.Width) && Maths.Equals(a.Height, b.Height);
    public static bool operator !=(Rect2d a, Rect2d b) => !(a == b);

    public bool Equals(Rect2d other) => this == other;

    public override bool Equals(object obj) => obj is Rect2d res && res == this;

    public override int GetHashCode() => unchecked((this.Width.GetHashCode() * 397) ^ this.Height.GetHashCode());

    public int CompareTo(Rect2d other) {
        int cmp = this.Width.CompareTo(other.Width);
        if (cmp == 0)
            cmp = this.Height.CompareTo(other.Height);
        return cmp;
    }

    public bool Equals(Rect2d x, Rect2d y) => x == this;

    public int GetHashCode(Rect2d obj) => obj.GetHashCode();

    public override string ToString() {
        return $"{this.Width}x{this.Height}";
    }
}