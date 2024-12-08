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

namespace FramePFX.Editing.Timelines;

/// <summary>
/// Represents an immutable slice of time in frames (similar to <see cref="TimeSpan"/>) and some utility functions.
/// <para>
/// This structure is 16 bytes; <see cref="Begin"/> and <see cref="Duration"/> fields
/// </para>
/// </summary>
public readonly struct FrameSpan : IEquatable<FrameSpan>
{
    public static readonly FrameSpan Empty = new FrameSpan(0, 0);

    /// <summary>
    /// The beginning of this span (inclusive index). This value may be negative (which isn't a valid span value, but is allowed anyway)
    /// </summary>
    public readonly long Begin;

    /// <summary>
    /// The duration (in frames) of this span. This value may be negative (which isn't a valid span value, but is allowed anyway)
    /// </summary>
    public readonly long Duration;

    /// <summary>
    /// A calculated end-index (exclusive) for this span. This value may be negative (which isn't a valid span value, but is allowed anyway)
    /// </summary>
    public long EndIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.Begin + this.Duration;
    }

    /// <summary>
    /// Whether or not this frame span's duration is zero
    /// </summary>
    public bool IsEmpty => this.Duration == 0;

    public FrameSpan(long begin, long duration)
    {
        this.Begin = begin;
        this.Duration = duration;
    }

    /// <summary>
    /// Creates a frame span with the given beginning frame and duration
    /// </summary>
    /// <param name="begin">The begin part</param>
    /// <param name="duration">The duration</param>
    /// <returns>A new frame span</returns>
    public static FrameSpan FromDuration(long begin, long duration)
    {
        return new FrameSpan(begin, duration);
    }

    /// <summary>
    /// Creates a frame span from the given begin (inclusive) and end (exclusive) indices
    /// </summary>
    /// <param name="begin">The begin part</param>
    /// <param name="endIndex">The end index</param>
    /// <returns>A new frame span</returns>
    public static FrameSpan FromIndex(long begin, long endIndex)
    {
        return new FrameSpan(begin, endIndex - begin);
    }

    /// <summary>
    /// Expands this span by the given number of frames. Does not clamp begin
    /// to zero nor does it ensure duration does not overflow
    /// </summary>
    /// <param name="count">The number of frames to subtract from begin and to add to the end index</param>
    /// <returns>A new expanded frame span</returns>
    public FrameSpan Expand(long count)
    {
        // adding twice is probably faster than multiplication by 2 :)
        return new FrameSpan(this.Begin - count, this.Duration + count + count);
    }

    /// <summary>
    /// Contracts this span by the given number of frames. Does not ensure
    /// </summary>
    /// <param name="contract"></param>
    /// <returns></returns>
    public FrameSpan Contract(long contract)
    {
        return new FrameSpan(this.Begin + contract, this.Duration - contract - contract);
    }

    /// <summary>
    /// Returns a new span where the <see cref="Begin"/> property is offset by the given amount, and <see cref="Duration"/> is untouched
    /// </summary>
    public FrameSpan Offset(long frames)
    {
        return new FrameSpan(this.Begin + frames, this.Duration);
    }

    public FrameSpan Offset(long offsetBegin, long offsetDuration)
    {
        return new FrameSpan(this.Begin + offsetBegin, this.Duration + offsetDuration);
    }

    /// <summary>
    /// Returns a new span where the <see cref="Duration"/> property is offset by the given amount, and <see cref="Begin"/> is untouched
    /// </summary>
    public FrameSpan OffsetDuration(long frames)
    {
        return new FrameSpan(this.Begin, this.Duration + frames);
    }

    /// <summary>
    /// Returns a frame span with the given begin and this instance's duration
    /// </summary>
    /// <param name="newBegin">The begin value</param>
    /// <returns>A new frame span</returns>
    public FrameSpan WithBegin(long newBegin)
    {
        return new FrameSpan(newBegin, this.Duration);
    }

    /// <summary>
    /// Returns a frame span with this instance's begin and the given duration
    /// </summary>
    /// <param name="newDuration">The duration value</param>
    /// <returns>A new frame span</returns>
    public FrameSpan WithDuration(long newDuration)
    {
        return new FrameSpan(this.Begin, newDuration);
    }

    /// <summary>
    /// Returns a new frame span, where the <see cref="Begin"/> is locked in place, and the <see cref="EndIndex"/> is modified
    /// </summary>
    /// <param name="value">The new end index value. This value is trusted to be non-negative</param>
    /// <returns>A new frame span</returns>
    /// <exception cref="ArgumentOutOfRangeException">Input value is less than the begin value</exception>
    public FrameSpan WithEndIndex(long value)
    {
        if (value < this.Begin)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Value cannot be smaller than the begin index ({value} < {this.Begin})");
        }

        return new FrameSpan(this.Begin, value - this.Begin);
    }

    /// <summary>
    /// Same as <see cref="WithEndIndex"/>, but instead of throwing, the span is clamped to empty
    /// </summary>
    /// <param name="value">The new end index value. This value is trusted to be non-negative</param>
    /// <param name="upperLimit">The upper limit for the end index. By default, this is <see cref="long.MaxValue"/> meaning effectively no upper limit</param>
    /// <returns>A new frame span, or empty when the value is less than or equal to the begin value</returns>
    public FrameSpan WithEndIndexClamped(long value, long upperLimit = long.MaxValue)
    {
        if (value > this.Begin)
        {
            if (value > upperLimit)
                value = upperLimit;
            return new FrameSpan(this.Begin, value - this.Begin);
        }

        return new FrameSpan(this.Begin, 0);
    }

    /// <summary>
    /// Returns a new frame span, where the <see cref="EndIndex"/> is locked in place, and the <see cref="Begin"/> is modified
    /// </summary>
    /// <param name="value">The new begin 'index'. This value is trusted to be non-negative</param>
    /// <returns>A new frame span</returns>
    /// <exception cref="ArgumentOutOfRangeException">Input value is greater than the end index</exception>
    public FrameSpan MoveBegin(long value)
    {
        long endIndex = this.Begin + this.Duration;
        if (value > endIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"New begin value cannot exceed the end index ({value} > {endIndex})");
        }

        return new FrameSpan(value, this.Duration - (value - this.Begin));
    }

    /// <summary>
    /// Same as <see cref="MoveBegin"/>, but instead of throwing, the span is clamped to empty
    /// </summary>
    /// <param name="value">The new end index value. This value is trusted to be non-negative</param>
    /// <param name="lowerLimit">The lower limit for the begin 'index'. By default, this is 0</param>
    /// <returns>A new frame span, or empty when the value is greater than or equal to the end index value</returns>
    public FrameSpan MoveBeginIndexClamped(long value, long lowerLimit = 0L)
    {
        long endIndex = this.Begin + this.Duration;
        if (value < lowerLimit)
            value = lowerLimit;
        return value < endIndex ? new FrameSpan(value, this.Duration - (value - this.Begin)) : new FrameSpan(endIndex, 0);
    }

    public FrameSpan AddEndIndex(long value)
    {
        return this.WithEndIndex(this.EndIndex + value);
    }

    public FrameSpan AddEndIndexClamped(long value, long upperLimit = long.MaxValue)
    {
        return this.WithEndIndexClamped(this.EndIndex + value, upperLimit);
    }

    public FrameSpan AddBeginIndex(long value)
    {
        return this.MoveBegin(this.Begin + value);
    }

    public FrameSpan AddBeginIndexClamped(long value, long lowerLimit = 0L)
    {
        return this.MoveBeginIndexClamped(this.Begin + value, lowerLimit);
    }

    public FrameSpan AddBegin(long value)
    {
        return new FrameSpan(this.Begin + value, this.Duration);
    }

    /// <summary>
    /// Returns a frame span whose <see cref="Begin"/> and <see cref="Duration"/> are non-negative.
    /// If none of them are negative, the current instance is returned
    /// </summary>
    /// <returns></returns>
    public FrameSpan Abs()
    {
        if (this.Begin >= 0 && this.Duration >= 0)
        {
            return this;
        }
        else
        {
            return new FrameSpan(Math.Abs(this.Begin), Math.Abs(this.Duration));
        }
    }

    /// <summary>
    /// Returns a new span which contains the smallest <see cref="Begin"/> and the largest <see cref="EndIndex"/> value
    /// of the current instance and <see cref="other"/>. This is used by <see cref="UnionAll"/>
    /// </summary>
    /// <param name="other">Input span</param>
    /// <returns>Output span</returns>
    public FrameSpan Union(FrameSpan other)
    {
        long begin = Math.Min(this.Begin, other.Begin);
        long endIndex = Math.Max(this.EndIndex, other.EndIndex);
        return new FrameSpan(begin, endIndex - begin);
    }

    /// <summary>
    /// Returns a new span containing the largest <see cref="Begin"/> and the smallest <see cref="EndIndex"/>
    /// </summary>
    /// <param name="clamp">Input span limit</param>
    /// <returns>A clamped frame span</returns>
    public FrameSpan Clamp(FrameSpan clamp)
    {
        return FromIndex(Math.Max(clamp.Begin, this.Begin), Math.Min(clamp.EndIndex, this.EndIndex));
    }

    public bool Intersects(long frame)
    {
        return frame >= this.Begin && frame < this.EndIndex;
    }

    public bool Intersects(FrameSpan span) => Intersects(in this, in span);

    public static bool Intersects(in FrameSpan a, in FrameSpan b)
    {
        // no idea if this works both ways... CBA to test lolol
        return a.Begin < b.EndIndex && a.EndIndex > b.Begin;
    }

    public static bool operator ==(in FrameSpan a, in FrameSpan b)
    {
        return a.Begin == b.Begin && a.Duration == b.Duration;
    }

    public static bool operator !=(in FrameSpan a, in FrameSpan b)
    {
        return a.Begin != b.Begin || a.Duration != b.Duration;
    }

    public override string ToString()
    {
        return $"{this.Begin}->{this.EndIndex} ({this.Duration})";
    }

    public bool Equals(FrameSpan other)
    {
        return this.Begin == other.Begin && this.Duration == other.Duration;
    }

    public override bool Equals(object obj)
    {
        return obj is FrameSpan other && this == other;
    }

    public override int GetHashCode()
    {
        return unchecked((this.Begin.GetHashCode() * 397) ^ this.Duration.GetHashCode());
    }

    /// <summary>
    /// Returns a new span which contains the smallest <see cref="Begin"/> and the largest <see cref="EndIndex"/> value
    /// </summary>
    /// <param name="a">Span A</param>
    /// <param name="b">Span B</param>
    /// <returns>Output span</returns>
    public static FrameSpan Union(FrameSpan a, FrameSpan b) => a.Union(b);

    /// <summary>
    /// Gets the total range coverage of all spans. This calculates the smallest <see cref="Begin"/> value and
    /// largest <see cref="EndIndex"/> value across all the spans, and returns a new frame span with those 2 values
    /// </summary>
    /// <param name="spans">Input span enumerable</param>
    /// <returns>The span range, or empty, if the enumerable is empty</returns>
    public static FrameSpan UnionAll(IEnumerable<FrameSpan> spans)
    {
        using (IEnumerator<FrameSpan> enumerator = spans.GetEnumerator())
        {
            return enumerator.MoveNext() ? UnionAllInternal(enumerator) : Empty;
        }
    }

    public static bool TryUnionAll(IEnumerable<FrameSpan> spans, out FrameSpan span)
    {
        using (IEnumerator<FrameSpan> enumerator = spans.GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                span = default;
                return false;
            }

            span = UnionAllInternal(enumerator);
            return true;
        }
    }

    private static FrameSpan UnionAllInternal(IEnumerator<FrameSpan> enumerator)
    {
        FrameSpan range = enumerator.Current;
        while (enumerator.MoveNext())
        {
            range = range.Union(enumerator.Current);
        }

        return range;
    }
}