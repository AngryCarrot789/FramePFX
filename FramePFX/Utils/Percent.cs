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

/// <summary>
/// A percentage with a decimal precision of two. This is a rational number with a denominator of 100 and a typical max numerator of 10,000
/// </summary>
public readonly struct Percent : IComparable<Percent>, IComparable<int>, IEquatable<Percent>
// IComparable,
// IConvertible,
// ISpanFormattable,
// IComparable<Percent>,
// IEquatable<Percent>,
// IBinaryInteger<Percent>,
// IMinMaxValue<Percent>,
// ISignedNumber<Percent>
{
    /// <summary>
    /// One percent (1%)
    /// </summary>
    public static Percent One => new Percent(100);

    /// <summary>
    /// Zero percent (0%)
    /// </summary>
    public static Percent Zero => default;

    /// <summary>
    /// Gets the numerator part of this rational number. 100% has a numerator of 10,000. 5.25% has a numerator of 525
    /// </summary>
    public int Numerator => this.m_value;

    /// <summary>
    /// Gets this percentage as a value between 0 and 1, with 4 decimal places
    /// </summary>
    public double AsUnitDouble => this.m_value / 10000.0;

    /// <summary>
    /// Gets this struct as a regular percentage double, with two decimal points
    /// </summary>
    public double AsPercentDouble => this.m_value / 100.0;

    private readonly int m_value;

    public Percent(int mValue) {
        this.m_value = mValue;
    }

    public static explicit operator int(Percent percent) => percent.m_value;

    public static explicit operator Percent(int percent) => new Percent(percent);

    public static Percent Abs(Percent value) => (Percent) Math.Abs(value.m_value);

    public static Percent Parse(string? s, IFormatProvider? provider = null) {
        if (s == null)
            throw new ArgumentNullException(nameof(s), "String is null");

        if (TryParse(s, provider, out Percent x))
            return x;

        throw new ArgumentException("Invalid percentage");
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Percent result) {
        int max;
        if (s != null && (max = (s = s.Trim()).Length) > 0)
            return TryParseInternal(s.AsSpan(0, s[max - 1] == '%' ? (max - 1) : max), provider, out result);

        result = default;
        return false;
    }

    public static Percent Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null) {
        if (TryParse(s, provider, out Percent x))
            return x;

        throw new ArgumentException("Invalid percentage");
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Percent result) {
        int max = (s = s.Trim()).Length;
        return TryParseInternal(s.Slice(0, s[max - 1] == '%' ? (max - 1) : max), provider, out result);
    }

    private static bool TryParseInternal(ReadOnlySpan<char> span, IFormatProvider? provider, out Percent result) {
        if (int.TryParse(span, provider, out int intResult)) {
            result = new Percent(intResult * 100);
            return true;
        }

        if (double.TryParse(span, provider, out double dres)) {
            result = new Percent((int) Math.Floor(Math.Round(dres * 100.0, 2)));
            return true;
        }

        result = default;
        return false;
    }

    public int CompareTo(Percent other) => this.m_value.CompareTo(other.m_value);

    public int CompareTo(int other) => this.m_value.CompareTo(other);

    public bool Equals(Percent other) => this.m_value == other.m_value;

    public override bool Equals(object? obj) => obj is Percent other && this.Equals(other);

    public override int GetHashCode() => this.m_value;

    public override string ToString() => Math.Round(this.AsPercentDouble, 2) + "%";

    public int CompareTo(object? value) {
        if (value == null)
            return 1;

        // NOTE: Cannot use return (_value - value) as this causes a wrap
        // around in cases where _value - value > MaxValue.
        if (value is Percent i) {
            if (this.m_value < i.m_value)
                return -1;
            if (this.m_value > i.m_value)
                return 1;
            return 0;
        }

        throw new ArgumentException("Arg is not int");
    }

    public static Percent operator +(Percent left, Percent right) => new Percent(left.m_value + right.m_value);

    public static Percent operator ~(Percent value) => new Percent(~value.m_value);

    public static bool operator ==(Percent left, Percent right) => left.m_value == right.m_value;

    public static bool operator !=(Percent left, Percent right) => left.m_value != right.m_value;

    public static bool operator >(Percent left, Percent right) => left.m_value > right.m_value;

    public static bool operator >=(Percent left, Percent right) => left.m_value >= right.m_value;

    public static bool operator <(Percent left, Percent right) => left.m_value < right.m_value;

    public static bool operator <=(Percent left, Percent right) => left.m_value <= right.m_value;

    public static Percent operator -(Percent left, Percent right) => (Percent) (left.m_value - right.m_value);

    public int ToOneHundredInt(RoundingMode roundingMode = RoundingMode.Round) => Maths.Round(this.m_value / 100.0, roundingMode);

    public static Percent FromPercentInt(int value) => new(value * 100);

    public static Percent FromPercentDouble(double value, RoundingMode roundingMode = RoundingMode.Round) {
        decimal value2 = (decimal) value * 100;
        return new Percent(Maths.Round(value2, roundingMode));
    }

    public static Percent FromUnitDouble(double unitDouble, RoundingMode roundingMode = RoundingMode.Round) {
        decimal value = (decimal) unitDouble * 10000;
        return new Percent(Maths.Round(value, roundingMode));
    }
}