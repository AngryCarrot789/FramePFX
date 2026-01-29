// 
// Copyright (c) 2026-2026 REghZy
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FramePFX.Editing;

/// <summary>
/// Represents the location and duration of a clip within a track.
/// </summary>
public readonly struct ClipSpan : IComparable<ClipSpan>, IEquatable<ClipSpan> {
    public static readonly ClipSpan Empty = new ClipSpan(0, 0);
    
    /// <summary>
    /// Contains <see cref="TimeSpan.Zero"/> to <see cref="TimeSpan.MaxValue"/>
    /// </summary>
    public static readonly ClipSpan MaxValue = new ClipSpan(TimeSpan.Zero, TimeSpan.MaxValue);
    
    /// <summary>
    /// Gets the start time. Note, since <see cref="TimeSpan"/> is usually a "duration" of time, you can think of this property as an offset from zero
    /// </summary>
    public TimeSpan Start { get; }

    /// <summary>
    /// Gets the end time. Note, like <see cref="Start"/>, this property represents an offset from zero.
    /// This value should (ideally) always be greater than or equal to <see cref="Start"/>
    /// </summary>
    public TimeSpan End { get; }

    /// <summary>
    /// Gets the duration of this span
    /// </summary>
    public TimeSpan Duration {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        get => this.End - this.Start;
    }

    public ClipSpan(TimeSpan start, TimeSpan end) {
        this.Start = start;
        this.End = end;
    }
    
    public ClipSpan(long start, long end) {
        this.Start = new TimeSpan(start);
        this.End = new TimeSpan(end);
    }

    /// <summary>Returns true when the location tick intersects this span</summary>
    public bool IntersectedBy(long location) => this.IntersectedBy(new TimeSpan(location));
    
    /// <summary>Returns true when the location tick intersects this span</summary>
    public bool IntersectedBy(TimeSpan location) => location >= this.Start && location < this.End;

    /// <summary>Returns true when the clip span intersects this span in any way</summary>
    public bool IntersectedBy(ClipSpan span) => span.End > this.Start && span.Start < this.End;
    
    /// <summary>Returns true when the clip span is fully contained within this span</summary>
    public bool Contains(ClipSpan span) => span.Start >= this.Start && span.End <= this.End;

    /// <summary>
    /// Creates a span that contains the smallest of <see cref="Start"/> and largest of <see cref="End"/>
    /// </summary>
    /// <param name="other">The other span to union with</param>
    /// <returns>A union of this and other</returns>
    public ClipSpan Union(ClipSpan other) {
        return new ClipSpan(Math.Min(this.Start.Ticks, other.Start.Ticks), Math.Max(this.End.Ticks, other.End.Ticks));
    }

    /// <summary>
    /// Clamps the current span within the range of the other span
    /// </summary>
    /// <param name="clamp">The clamping span</param>
    /// <returns>A clamped span</returns>
    public ClipSpan Clamp(ClipSpan clamp) {
        return FromStartEnd(Math.Max(clamp.Start.Ticks, this.Start.Ticks), Math.Min(clamp.End.Ticks, this.End.Ticks));
    }

    public override string ToString() {
        return $"{this.Start} -> {this.End} ({this.Duration})";
    }

    public int CompareTo(ClipSpan other) {
        int s = this.Start.CompareTo(other.Start);
        return s == 0 ? this.End.CompareTo(other.End) : s;
    }

    public bool Equals(ClipSpan other) {
        return this.Start == other.Start && this.End == other.End;
    }

    public override bool Equals(object? obj) {
        return obj is ClipSpan other && this.Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(this.Start, this.End);
    }

    public static bool operator ==(ClipSpan left, ClipSpan right) => left.Start == right.Start && left.End == right.End;

    public static bool operator !=(ClipSpan left, ClipSpan right) => left.Start != right.Start || left.End == right.End;

    public static ClipSpan FromStartEnd(long start, long end) {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(end);
        if (end < start) {
            ThrowNegativeDuration(start, end);
        }

        return new ClipSpan(start, end);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowNegativeDuration(long start, long end) {
            throw new ArgumentException($"End < Start ({start} < {end})");
        }
    }

    public static ClipSpan FromDuration(long start, long duration) {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(duration);
        if (start + duration < start) {
            ThrowOverflow(start, duration);
        }

        return new ClipSpan(start, start + duration);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowOverflow(long start, long duration) {
            throw new ArgumentException($"Start+Duration results in overflow of end index ({start} + {duration})");
        }
    }

    public static ClipSpan FromDuration(long start, TimeSpan duration) => FromDuration(start, duration.Ticks);

    public static ClipSpan FromDuration(TimeSpan start, long duration) => FromDuration(start.Ticks, duration);

    public static ClipSpan FromDuration(TimeSpan start, TimeSpan duration) => FromDuration(start.Ticks, duration.Ticks);

    /// <summary>
    /// Returns a new span which contains the smallest <see cref="Begin"/> and the largest <see cref="EndIndex"/> value
    /// </summary>
    /// <param name="a">Span A</param>
    /// <param name="b">Span B</param>
    /// <returns>Output span</returns>
    public static ClipSpan Union(ClipSpan a, ClipSpan b) => a.Union(b);

    /// <summary>
    /// Gets the total range coverage of all spans. This calculates the smallest <see cref="Begin"/> value and
    /// largest <see cref="EndIndex"/> value across all the spans, and returns a new frame span with those 2 values
    /// </summary>
    /// <param name="spans">Input span enumerable</param>
    /// <returns>The span range, or empty, if the enumerable is empty</returns>
    public static ClipSpan UnionAll(IEnumerable<ClipSpan> spans) {
        using IEnumerator<ClipSpan> enumerator = spans.GetEnumerator();
        return enumerator.MoveNext() ? UnionAllInternal(enumerator) : Empty;
    }

    public static bool TryUnionAll(IEnumerable<ClipSpan> spans, out ClipSpan span) {
        using IEnumerator<ClipSpan> enumerator = spans.GetEnumerator();
        if (!enumerator.MoveNext()) {
            span = default;
            return false;
        }

        span = UnionAllInternal(enumerator);
        return true;
    }

    private static ClipSpan UnionAllInternal(IEnumerator<ClipSpan> enumerator) {
        ClipSpan range = enumerator.Current;
        while (enumerator.MoveNext()) {
            range = range.Union(enumerator.Current);
        }

        return range;
    }
}