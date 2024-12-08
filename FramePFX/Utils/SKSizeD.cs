// 
// Copyright (c) 2024-2024 REghZy
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

using SkiaSharp;

namespace FramePFX.Utils;

public struct SKSizeD : IEquatable<SKSizeD>
{
    /// <summary>Represents a new instance of the <see cref="T:SkiaSharp.SKSizeD" /> class with member data left uninitialized.</summary>
    /// <remarks />
    public static readonly SKSizeD Empty;

    public double Width;

    public double Height;

    public SKSizeD(double width, double height)
    {
        this.Width = width;
        this.Height = height;
    }

    public SKSizeD(SKPointD pt)
    {
        this.Width = pt.X;
        this.Height = pt.Y;
    }

    /// <summary>Gets a value that indicates whether this <see cref="T:SkiaSharp.SKSizeD" /> structure has zero width and height.</summary>
    /// <value />
    /// <remarks />
    public readonly bool IsEmpty => this == SKSizeD.Empty;

    /// <summary>Converts a <see cref="T:SkiaSharp.SKSizeD" /> structure to a <see cref="T:SkiaSharp.SKPointD" /> structure.</summary>
    /// <returns>Returns a <see cref="T:SkiaSharp.SKPointD" /> structure.</returns>
    /// <remarks />
    public readonly SKPointD ToPoint() => new SKPointD(this.Width, this.Height);

    /// <summary>Converts a <see cref="T:SkiaSharp.SKSizeD" /> structure to a <see cref="T:SkiaSharp.SKSizeI" /> structure.</summary>
    /// <returns>Returns a <see cref="T:SkiaSharp.SKSizeI" /> structure.</returns>
    /// <remarks />
    public readonly SKSizeI ToSizeI()
    {
        return new SKSizeI(checked((int) this.Width), checked((int) this.Height));
    }

    /// <summary>Converts this <see cref="T:SkiaSharp.SKSizeD" /> to a human readable string.</summary>
    /// <returns>A string that represents this <see cref="T:SkiaSharp.SKSizeD" />.</returns>
    /// <remarks />
    public readonly override string ToString()
    {
        return $"{this.Width:F4},{this.Height:F4}";
    }

    /// <param name="sz1">The first <see cref="T:SkiaSharp.SKSizeD" /> structure to add.</param>
    /// <param name="sz2">The second <see cref="T:SkiaSharp.SKSizeD" /> structure to add.</param>
    /// <summary>Adds the width and height of one <see cref="T:SkiaSharp.SKSizeD" /> structure to the width and height of another <see cref="T:SkiaSharp.SKSizeD" /> structure.</summary>
    /// <returns>A <see cref="T:SkiaSharp.SKSizeD" /> structure that is the result of the addition operation.</returns>
    /// <remarks />
    public static SKSizeD Add(SKSizeD sz1, SKSizeD sz2) => sz1 + sz2;

    /// <param name="sz1">The <see cref="T:SkiaSharp.SKSizeD" /> structure on the left side of the subtraction operator.</param>
    /// <param name="sz2">The <see cref="T:SkiaSharp.SKSizeD" /> structure on the right side of the subtraction operator.</param>
    /// <summary>Subtracts the width and height of one <see cref="T:SkiaSharp.SKSizeD" /> structure from the width and height of another <see cref="T:SkiaSharp.SKSizeD" /> structure.</summary>
    /// <returns>A <see cref="T:SkiaSharp.SKSizeD" /> that is the result of the subtraction operation.</returns>
    /// <remarks />
    public static SKSizeD Subtract(SKSizeD sz1, SKSizeD sz2) => sz1 - sz2;

    /// <param name="sz1">The first <see cref="T:SkiaSharp.SKSizeD" /> structure to add.</param>
    /// <param name="sz2">The second <see cref="T:SkiaSharp.SKSizeD" /> structure to add.</param>
    /// <summary>Adds the width and height of one <see cref="T:SkiaSharp.SKSizeD" /> structure to the width and height of another <see cref="T:SkiaSharp.SKSizeD" /> structure.</summary>
    /// <returns>A <see cref="T:SkiaSharp.SKSizeD" /> structure that is the result of the addition operation.</returns>
    /// <remarks />
    public static SKSizeD operator +(SKSizeD sz1, SKSizeD sz2)
    {
        return new SKSizeD(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
    }

    public static SKSizeD operator -(SKSizeD sz1, SKSizeD sz2)
    {
        return new SKSizeD(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
    }

    public static explicit operator SKPointD(SKSizeD size) => new SKPointD(size.Width, size.Height);

    public static implicit operator SKSizeD(SKSizeI size)
    {
        return new SKSizeD(size.Width, size.Height);
    }

    public readonly bool Equals(SKSizeD obj)
    {
        return this.Width == obj.Width && this.Height == obj.Height;
    }

    public readonly override bool Equals(object? obj) => obj is SKSizeD skSize && this.Equals(skSize);

    public static bool operator ==(SKSizeD left, SKSizeD right) => left.Equals(right);

    public static bool operator !=(SKSizeD left, SKSizeD right) => !left.Equals(right);

    /// <summary>Returns a hash code for this <see cref="T:SkiaSharp.SKSizeD" /> structure.</summary>
    /// <returns>An integer value that specifies a hash value for this <see cref="T:SkiaSharp.SKSizeD" /> structure.</returns>
    /// <remarks />
    public readonly override int GetHashCode()
    {
        HashCode hashCode = new HashCode();
        hashCode.Add<double>(this.Width);
        hashCode.Add<double>(this.Height);
        return hashCode.ToHashCode();
    }
}