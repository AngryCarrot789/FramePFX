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

using System.Runtime.CompilerServices;
using SkiaSharp;

namespace FramePFX.Utils;

/// <summary>Represents an ordered pair of floating-point x- and y-coordinates that defines a point in a two-dimensional plane.</summary>
/// <remarks>To convert a <see cref="T:SkiaSharp.SKPointD" /> to a <see cref="T:SkiaSharp.SKPointI" />, use <see cref="M:SkiaSharp.SKPointI.Round(SkiaSharp.SKPointD)" /> or <see cref="M:SkiaSharp.SKPointI.Truncate(SkiaSharp.SKPointD)" />.</remarks>
public struct SKPointD : IEquatable<SKPointD> {
    /// <summary>Represents a new instance of the <see cref="T:SkiaSharp.SKPointD" /> class with member data left uninitialized.</summary>
    /// <remarks />
    public static readonly SKPointD Empty;

    public double X;

    public double Y;

    public SKPointD(double x, double y) {
        this.X = x;
        this.Y = y;
    }

    /// <summary>Gets a value indicating whether this point is empty.</summary>
    /// <value />
    /// <remarks />
    public readonly bool IsEmpty => this == Empty;

    /// <summary>Gets the Euclidean distance from the origin (0, 0).</summary>
    /// <value />
    /// <remarks />
    public readonly double Length => Math.Sqrt(this.X * this.X + this.Y * this.Y);

    /// <summary>Gets the Euclidean distance squared from the origin (0, 0).</summary>
    /// <value />
    /// <remarks />
    public readonly double LengthSquared => this.X * this.X + this.Y * this.Y;

    /// <param name="p">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <remarks />
    public void Offset(SKPointD p) {
        this.X += p.X;
        this.Y += p.Y;
    }

    /// <param name="dx">The offset in the x-direction.</param>
    /// <param name="dy">The offset in the y-direction.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <remarks />
    public void Offset(double dx, double dy) {
        this.X += dx;
        this.Y += dy;
    }

    /// <summary>Converts this <see cref="T:SkiaSharp.SKPointD" /> to a human readable string.</summary>
    /// <returns>A string that represents this <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public readonly override string ToString() {
        DefaultInterpolatedStringHandler x = new DefaultInterpolatedStringHandler(8, 2);
        x.AppendLiteral("{X=");
        x.AppendFormatted<double>(this.X);
        x.AppendLiteral(", Y=");
        x.AppendFormatted<double>(this.Y);
        x.AppendLiteral("}");
        return x.ToStringAndClear();
    }

    /// <param name="point">The point to normalize.</param>
    /// <summary>Returns a point with the same direction as the specified point, but with a length of one.</summary>
    /// <returns>Returns a point with a length of one.</returns>
    /// <remarks />
    public static SKPointD Normalize(SKPointD point) {
        double num = 1.0 / Math.Sqrt(point.X * point.X + point.Y * point.Y);
        return new SKPointD(point.X * num, point.Y * num);
    }

    /// <param name="point">The first point.</param>
    /// <param name="other">The second point.</param>
    /// <summary>Calculate the Euclidean distance between two points.</summary>
    /// <returns>Returns the Euclidean distance between two points.</returns>
    /// <remarks />
    public static double Distance(SKPointD point, SKPointD other) {
        double num1 = point.X - other.X;
        double num2 = point.Y - other.Y;
        return Math.Sqrt(num1 * num1 + num2 * num2);
    }

    /// <param name="point">The first point.</param>
    /// <param name="other">The second point.</param>
    /// <summary>Calculate the Euclidean distance squared between two points.</summary>
    /// <returns>Returns the Euclidean distance squared between two points.</returns>
    /// <remarks />
    public static double DistanceSquared(SKPointD point, SKPointD other) {
        double num1 = point.X - other.X;
        double num2 = point.Y - other.Y;
        return num1 * num1 + num2 * num2;
    }

    /// <param name="point">The point to reflect.</param>
    /// <param name="normal">The normal.</param>
    /// <summary>Returns the reflection of a point off a surface that has the specified normal.</summary>
    /// <returns>Returns the reflection of a point.</returns>
    /// <remarks />
    public static SKPointD Reflect(SKPointD point, SKPointD normal) {
        double num = point.X * point.X + point.Y * point.Y;
        return new SKPointD(point.X - 2f * num * normal.X, point.Y - 2f * num * normal.Y);
    }

    /// <param name="pt">The point to translate</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    /// <remarks />
    public static SKPointD Add(SKPointD pt, SKSizeI sz) => pt + sz;

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    /// <remarks />
    public static SKPointD Add(SKPointD pt, SKSize sz) => pt + sz;

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <returns>Returns the translated point.</returns>
    /// <remarks />
    public static SKPointD Add(SKPointD pt, SKPointI sz) => pt + sz;

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <returns>Returns the translated point.</returns>
    /// <remarks />
    public static SKPointD Add(SKPointD pt, SKPointD sz) => pt + sz;

    /// <param name="pt">The <see cref="T:SkiaSharp.SKPointD" /> to translate.</param>
    /// <param name="sz">The <see cref="T:SkiaSharp.SKSize" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a <see cref="T:SkiaSharp.SKPointD" /> by the negative of a specified size.</summary>
    /// <returns>The translated <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public static SKPointD Subtract(SKPointD pt, SKSizeI sz) => pt - sz;

    /// <param name="pt">The <see cref="T:SkiaSharp.SKPointD" /> to translate.</param>
    /// <param name="sz">The <see cref="T:SkiaSharp.SKSize" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a <see cref="T:SkiaSharp.SKPointD" /> by the negative of a specified size.</summary>
    /// <returns>The translated <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public static SKPointD Subtract(SKPointD pt, SKSize sz) => pt - sz;

    /// <param name="pt">The <see cref="T:SkiaSharp.SKPointD" /> to translate.</param>
    /// <param name="sz">The offset that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a given point by the negative of a specified offset.</summary>
    /// <returns>The translated <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public static SKPointD Subtract(SKPointD pt, SKPointI sz) => pt - sz;

    /// <param name="pt">The <see cref="T:SkiaSharp.SKPointD" /> to translate.</param>
    /// <param name="sz">The offset that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a given point by the negative of a specified offset.</summary>
    /// <returns>The translated <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public static SKPointD Subtract(SKPointD pt, SKPointD sz) => pt - sz;

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    /// <remarks />
    public static SKPointD operator +(SKPointD pt, SKSizeI sz) {
        return new SKPointD(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    /// <remarks />
    public static SKPointD operator +(SKPointD pt, SKSize sz) {
        return new SKPointD(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <returns>Returns the translated point.</returns>
    /// <remarks />
    public static SKPointD operator +(SKPointD pt, SKPointI sz) {
        return new SKPointD(pt.X + sz.X, pt.Y + sz.Y);
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <returns>Returns the translated point.</returns>
    /// <remarks />
    public static SKPointD operator +(SKPointD pt, SKPointD sz) {
        return new SKPointD(pt.X + sz.X, pt.Y + sz.Y);
    }

    /// <param name="pt">The <see cref="T:SkiaSharp.SKPointD" /> to translate.</param>
    /// <param name="sz">The <see cref="T:SkiaSharp.SKSizeI" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a <see cref="T:SkiaSharp.SKPointD" /> by the negative of a given <see cref="T:SkiaSharp.SKSizeI" />.</summary>
    /// <returns>The translated <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public static SKPointD operator -(SKPointD pt, SKSizeI sz) {
        return new SKPointD(pt.X - sz.Width, pt.Y - sz.Height);
    }

    /// <param name="pt">The <see cref="T:SkiaSharp.SKPointD" /> to translate.</param>
    /// <param name="sz">The <see cref="T:SkiaSharp.SKSize" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a <see cref="T:SkiaSharp.SKPointD" /> by the negative of a given <see cref="T:SkiaSharp.SKSize" />.</summary>
    /// <returns>The translated <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public static SKPointD operator -(SKPointD pt, SKSize sz) {
        return new SKPointD(pt.X - sz.Width, pt.Y - sz.Height);
    }

    /// <param name="pt">The <see cref="T:SkiaSharp.SKPointD" /> to translate.</param>
    /// <param name="sz">The point that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a given point by the negative of a specified offset.</summary>
    /// <returns>The translated <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public static SKPointD operator -(SKPointD pt, SKPointI sz) {
        return new SKPointD(pt.X - sz.X, pt.Y - sz.Y);
    }

    /// <param name="pt">The <see cref="T:SkiaSharp.SKPointD" /> to translate.</param>
    /// <param name="sz">The point that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a given point by the negative of a specified offset.</summary>
    /// <returns>The translated <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public static SKPointD operator -(SKPointD pt, SKPointD sz) {
        return new SKPointD(pt.X - sz.X, pt.Y - sz.Y);
    }

    /// <param name="obj">The <see cref="T:SkiaSharp.SKPointD" /> to test.</param>
    /// <summary>Specifies whether this <see cref="T:SkiaSharp.SKPointD" /> contains the same coordinates as the specified <see cref="T:SkiaSharp.SKPointD" />.</summary>
    /// <returns>This method returns true if <paramref name="obj" /> has the same coordinates as this <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public readonly bool Equals(SKPointD obj) {
        return this.X == obj.X && this.Y == obj.Y;
    }

    /// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
    /// <summary>Specifies whether this <see cref="T:SkiaSharp.SKPointD" /> contains the same coordinates as the specified <see cref="T:System.Object" />.</summary>
    /// <returns>This method returns true if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKPointD" /> and has the same coordinates as this <see cref="T:SkiaSharp.SKPointD" />.</returns>
    /// <remarks />
    public readonly override bool Equals(object obj) {
        return obj is SKPointD skPoint && this.Equals(skPoint);
    }

    /// <param name="left">A <see cref="T:SkiaSharp.SKPointD" /> to compare.</param>
    /// <param name="right">A <see cref="T:SkiaSharp.SKPointD" /> to compare.</param>
    /// <summary>Compares two <see cref="T:SkiaSharp.SKPointD" /> structures. The result specifies whether the values of the <see cref="P:SkiaSharp.SKPointD.X" /> and <see cref="P:SkiaSharp.SKPointD.Y" /> properties of the two <see cref="T:SkiaSharp.SKPointD" /> structures are equal.</summary>
    /// <returns>true if the <see cref="P:SkiaSharp.SKPointD.X" /> and <see cref="P:SkiaSharp.SKPointD.Y" /> values of the left and right <see cref="T:SkiaSharp.SKPointD" /> structures are equal; otherwise, false.</returns>
    /// <remarks />
    public static bool operator ==(SKPointD left, SKPointD right) => left.Equals(right);

    /// <param name="left">A <see cref="T:SkiaSharp.SKPointD" /> to compare.</param>
    /// <param name="right">A <see cref="T:SkiaSharp.SKPointD" /> to compare.</param>
    /// <summary>Determines whether the coordinates of the specified points are not equal.</summary>
    /// <returns>true if the <see cref="P:SkiaSharp.SKPointD.X" /> and <see cref="P:SkiaSharp.SKPointD.Y" /> values of the left and right <see cref="T:SkiaSharp.SKPointD" /> structures differ; otherwise, false.</returns>
    /// <remarks />
    public static bool operator !=(SKPointD left, SKPointD right) => !left.Equals(right);

    /// <summary>Calculates the hashcode for this point.</summary>
    /// <returns>Returns the hashcode for this point.</returns>
    /// <remarks>You should avoid depending on GetHashCode for unique values, as two <see cref="T:System.Drawing.Point" /> objects with the same values for their X and Y properties may return the same hash code. This behavior could change in a future release.</remarks>
    public readonly override int GetHashCode() {
        HashCode hashCode = new HashCode();
        hashCode.Add<double>(this.X);
        hashCode.Add<double>(this.Y);
        return hashCode.ToHashCode();
    }

    public static implicit operator SKPoint(SKPointD point) => new SKPoint((float) point.X, (float) point.Y);
    public static implicit operator SKPointD(SKPoint point) => new SKPointD(point.X, point.Y);
}