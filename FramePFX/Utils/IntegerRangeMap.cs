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
/// A class that can be used to map a range of integer keys to a single value, while supporting merging behaviour
/// </summary>
public class IntegerRangeMap<T> {
    // Example setup: 0->4 = "hello", 7->12 = "hi there!", 13->15 = "okay", 16->18 = "what"

    private readonly SortedDictionary<int, Range> ranges;

    public IntegerRangeMap() {
        this.ranges = new SortedDictionary<int, Range>();
    }

    public static int BinarySearchIndexOf(IList<int> list, int value) {
        int min = 0, max = list.Count - 1;
        while (min <= max) {
            int mid = min + (max - min) / 2;
            int cmp = value.CompareTo(list[mid]);
            if (cmp == 0)
                return mid;
            else if (cmp < 0)
                max = mid - 1;
            else
                min = mid + 1;
        }

        return ~min;
    }

    public void Insert(int index, T value) {
        // If there are no ranges yet, just add the new range
        if (this.ranges.Count == 0) {
            this.ranges.Add(index, new Range(index, index, value));
            return;
        }
    }

    private Range? GetLowerRange(int index) {
        Range? lowerRange = null;
        foreach (var key in this.ranges.Keys) {
            if (key <= index && this.ranges[key].End >= index)
                return this.ranges[key];
            if (key < index)
                lowerRange = this.ranges[key];
            else
                break;
        }

        return lowerRange;
    }

    private Range? GetUpperRange(int index) {
        foreach (var key in this.ranges.Keys) {
            if (key > index)
                return this.ranges[key];
        }

        return null;
    }

    public override string ToString() {
        return string.Join(", ", this.ranges.Values);
    }

    private readonly struct Range {
        public int Start { get; }
        public int End { get; }
        public T Value { get; }

        public Range(int start, int end, T value) {
            this.Start = start;
            this.End = end;
            this.Value = value;
        }

        public override string ToString() {
            return $"{this.Start}->{this.End} = {this.Value}";
        }
    }

    public static void Test() {
        IntegerRangeMap<string> map = new IntegerRangeMap<string>();
        map.Insert(0, "hello");
        map.Insert(1, "hello");
        map.Insert(2, "hello");
        map.Insert(3, "hello");
        map.Insert(4, "hello");
        map.Insert(7, "hi!");
        map.Insert(8, "hi!");
        map.Insert(9, "hi!");

        map.Insert(5, "joe mama");

        Console.WriteLine(map.ToString());
    }
}