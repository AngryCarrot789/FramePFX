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

namespace FramePFX.Utils;

/// <summary>Represents a size in device pixels.</summary>
public readonly struct PixSize : IEquatable<PixSize>
{
    /// <summary>A size representing zero</summary>
    public static readonly PixSize Empty = new PixSize(0, 0);

    /// <summary>Gets the aspect ratio of the size.</summary>
    public double AspectRatio => this.Width / (double) this.Height;

    /// <summary>Gets the width.</summary>
    public readonly int Width;

    /// <summary>Gets the height.</summary>
    public readonly int Height;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Avalonia.PixelSize" /> structure.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public PixSize(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Checks for equality between two <see cref="T:Avalonia.PixelSize" />s.
    /// </summary>
    /// <param name="left">The first size.</param>
    /// <param name="right">The second size.</param>
    /// <returns>True if the sizes are equal; otherwise false.</returns>
    public static bool operator ==(PixSize left, PixSize right) => left.Equals(right);

    /// <summary>
    /// Checks for inequality between two <see cref="T:SKSizeD" />s.
    /// </summary>
    /// <param name="left">The first size.</param>
    /// <param name="right">The second size.</param>
    /// <returns>True if the sizes are unequal; otherwise false.</returns>
    public static bool operator !=(PixSize left, PixSize right) => !(left == right);

    /// <summary>
    /// Returns a boolean indicating whether the size is equal to the other given size.
    /// </summary>
    /// <param name="other">The other size to test equality against.</param>
    /// <returns>True if this size is equal to other; False otherwise.</returns>
    public bool Equals(PixSize other) => this.Width == other.Width && this.Height == other.Height;

    /// <summary>Checks for equality between a size and an object.</summary>
    /// <param name="obj">The object.</param>
    /// <returns>
    /// True if <paramref name="obj" /> is a size that equals the current size.
    /// </returns>
    public override bool Equals(object? obj) => obj is PixSize other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for a <see cref="T:Avalonia.PixelSize" />.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return (17 * 23 + this.Width.GetHashCode()) * 23 + this.Height.GetHashCode();
    }

    /// <summary>
    /// Returns a new <see cref="T:Avalonia.PixelSize" /> with the same height and the specified width.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <returns>The new <see cref="T:Avalonia.PixelSize" />.</returns>
    public PixSize WithWidth(int width) => new PixSize(width, this.Height);

    /// <summary>
    /// Returns a new <see cref="T:Avalonia.PixelSize" /> with the same width and the specified height.
    /// </summary>
    /// <param name="height">The height.</param>
    /// <returns>The new <see cref="T:Avalonia.PixelSize" />.</returns>
    public PixSize WithHeight(int height) => new PixSize(this.Width, height);

    /// <summary>
    /// Converts the <see cref="T:Avalonia.PixelSize" /> to a device-independent <see cref="T:SKSizeD" /> using the
    /// specified scaling factor.
    /// </summary>
    /// <param name="scale">The scaling factor.</param>
    /// <returns>The device-independent size.</returns>
    public SKSizeD ToSize(double scale)
    {
        return new SKSizeD((float) (this.Width / scale), (float) (this.Height / scale));
    }

    /// <summary>
    /// Converts the <see cref="T:Avalonia.PixelSize" /> to a device-independent <see cref="T:SKSizeD" /> using the
    /// specified scaling factor.
    /// </summary>
    /// <param name="scale">The scaling factor.</param>
    /// <returns>The device-independent size.</returns>
    public SKSizeD ToSize(SKPointD scale)
    {
        return new SKSizeD(this.Width / scale.X, this.Height / scale.Y);
    }

    /// <summary>
    /// Converts the <see cref="T:Avalonia.PixelSize" /> to a device-independent <see cref="T:SKSizeD" /> using the
    /// specified dots per inch (DPI).
    /// </summary>
    /// <param name="dpi">The dots per inch.</param>
    /// <returns>The device-independent size.</returns>
    public SKSizeD ToSizeWithDpi(double dpi) => this.ToSize(dpi / 96.0);

    /// <summary>
    /// Converts the <see cref="T:Avalonia.PixelSize" /> to a device-independent <see cref="T:SKSizeD" /> using the
    /// specified dots per inch (DPI).
    /// </summary>
    /// <param name="dpi">The dots per inch.</param>
    /// <returns>The device-independent size.</returns>
    public SKSizeD ToSizeWithDpi(SKPointD dpi) => this.ToSize(new SKPointD(dpi.X / 96.0, dpi.Y / 96.0));

    /// <summary>
    /// Converts a <see cref="T:SKSizeD" /> to device pixels using the specified scaling factor.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="scale">The scaling factor.</param>
    /// <returns>The device-independent size.</returns>
    public static PixSize FromSize(SKSizeD size, double scale)
    {
        return new PixSize((int) Math.Ceiling(size.Width * scale), (int) Math.Ceiling(size.Height * scale));
    }

    /// <summary>
    /// A reversible variant of <see cref="M:Avalonia.PixelSize.FromSize(SKSizeD,System.Double)" /> that uses Round instead of Ceiling to make it reversible from ToSize
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="scale">The scaling factor.</param>
    /// <returns>The device-independent size.</returns>
    internal static PixSize FromSizeRounded(SKSizeD size, double scale)
    {
        return new PixSize((int) Math.Round(size.Width * scale), (int) Math.Round(size.Height * scale));
    }

    /// <summary>
    /// Converts a <see cref="T:SKSizeD" /> to device pixels using the specified scaling factor.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="scale">The scaling factor.</param>
    /// <returns>The device-independent size.</returns>
    public static PixSize FromSize(SKSizeD size, SKPointD scale)
    {
        return new PixSize((int) Math.Ceiling(size.Width * scale.X), (int) Math.Ceiling(size.Height * scale.Y));
    }

    /// <summary>
    /// Converts a <see cref="T:SKSizeD" /> to device pixels using the specified dots per inch (DPI).
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="dpi">The dots per inch.</param>
    /// <returns>The device-independent size.</returns>
    public static PixSize FromSizeWithDpi(SKSizeD size, double dpi)
    {
        return FromSize(size, dpi / 96.0);
    }

    /// <summary>
    /// Converts a <see cref="T:SKSizeD" /> to device pixels using the specified dots per inch (DPI).
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="dpi">The dots per inch.</param>
    /// <returns>The device-independent size.</returns>
    public static PixSize FromSizeWithDpi(SKSizeD size, SKPointD dpi)
    {
        return FromSize(size, new SKPointD(dpi.X / 96.0, dpi.Y / 96.0));
    }

    /// <summary>Returns the string representation of the size.</summary>
    /// <returns>The string representation of the size.</returns>
    public override string ToString()
    {
        return $"{this.Width:F4},{this.Height:F4}";
    }
}