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

using System.Collections;
using System.Diagnostics;

namespace FramePFX.Utils;

/// <summary>
/// This class efficiently stores a set of integers. It is pretty much an integer hash set
/// </summary>
public class IntegerRangeList
{
    private readonly List<IntRange> list;

    public IEnumerable<IntRange> Items => this.list;

    public bool IsEmpty => this.list.Count < 1;

    public IntegerRangeList()
    {
        this.list = new List<IntRange>();
    }

    /// <summary>
    /// Adds a range, both numbers being inclusive
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public void AddRange(int a, int b)
    {
        for (int i = a; i <= b; i++)
            this.Add(i);
    }

    public void Add(int value)
    {
        for (int i = 0; i < this.list.Count; i++)
        {
            IntRange range = this.list[i];

            if (value >= range.A && value <= range.B)
            {
                return;
            }
            else if (value == range.A - 1)
            {
                this.list[i] = new IntRange(value, range.B);
                this.MergeRanges(i);
                return;
            }
            else if (value == range.B + 1)
            {
                this.list[i] = new IntRange(range.A, value);
                this.MergeRanges(i);
                return;
            }
            else if (value < range.A)
            {
                this.list.Insert(i, new IntRange(value, value));
                return;
            }
        }

        this.list.Add(new IntRange(value, value));
    }

    public void Remove(int value)
    {
        for (int i = 0; i < this.list.Count; i++)
        {
            IntRange range = this.list[i];

            if (value >= range.A && value <= range.B)
            {
                if (range.A == range.B)
                {
                    this.list.RemoveAt(i);
                }
                else if (value == range.A)
                {
                    this.list[i] = new IntRange(range.A + 1, range.B);
                }
                else if (value == range.B)
                {
                    this.list[i] = new IntRange(range.A, range.B - 1);
                }
                else
                {
                    this.list[i] = new IntRange(range.A, value - 1);
                    this.list.Insert(i + 1, new IntRange(value + 1, range.B));
                }

                return;
            }
        }
    }

    private void MergeRanges(int index)
    {
        if (index < this.list.Count - 1 && this.list[index].B + 1 == this.list[index + 1].A)
        {
            this.list[index] = new IntRange(this.list[index].A, this.list[index + 1].B);
            this.list.RemoveAt(index + 1);
        }
    }

    public override string ToString()
    {
        return string.Join(", ", this.list);
    }

    public static void Test()
    {
        IntegerRangeList list = new IntegerRangeList();
        list.Add(1);
        list.Add(3);
        list.Add(2);
        list.Add(40);
        list.Add(5);
        list.Add(9);
        list.Add(7);
        list.Add(6);
        list.Add(8);
        list.Add(42);
        list.Add(41);
        list.Add(4);
        Debug.Assert(list.list.Count == 2);
        Debug.Assert(list.list[0].Equals(new IntRange(1, 9)));
        Debug.Assert(list.list[1].Equals(new IntRange(40, 42)));
    }
}

/// <summary>
/// Contains two pairs of integer, both inclusive
/// </summary>
public readonly struct IntRange : IEquatable<IntRange>, IEnumerable<int>
{
    /// <summary>
    /// The starting position, inclusive
    /// </summary>
    public readonly int A;

    /// <summary>
    /// The ending position, inclusive
    /// </summary>
    public readonly int B;

    public IntRange(int a, int b)
    {
        this.A = a;
        this.B = b;
    }

    public bool Equals(IntRange other) => this.A == other.A && this.B == other.B;

    public override bool Equals(object? obj) => obj is IntRange other && this.Equals(other);

    public override int GetHashCode() => HashCode.Combine(this.A, this.B);

    public override string ToString() => $"({this.A} -> {this.B})";

    public static bool operator ==(IntRange left, IntRange right) => left.A == right.A && left.B == right.B;

    public static bool operator !=(IntRange left, IntRange right) => left.A != right.A || left.B != right.B;

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = this.A; i < +this.B; i++)
        {
            yield return i;
        }
    }
}