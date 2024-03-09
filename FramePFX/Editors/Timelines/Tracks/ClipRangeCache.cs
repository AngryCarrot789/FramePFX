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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Timelines.Tracks
{
    public delegate void ClipRangeCacheEventHandler(ClipRangeCache handler);

    /// <summary>
    /// A class that stores clips in chunks of 128 frames (0-127, 128-255, 256-383, etc.) to efficiently
    /// locate clips at a particular frame, rather than having to scan the entire track's clip list
    /// </summary>
    public class ClipRangeCache
    {
        private readonly SortedList<long, ClipList> Map;

        /// <summary>
        /// The smallest frame that any clip takes up based on their span's begin property.
        /// This is basically calculated as:
        /// <code>
        /// foreach (clip in track) value = min(value, clip.Span.FrameBegin)
        /// </code>
        /// </summary>
        public long SmallestActiveFrame { get; private set; }

        /// <summary>
        /// The largest frame that any clip takes up based on their span's end index property.
        /// This is basically calculated as:
        /// <code>
        /// foreach (clip in track) value = max(value, clip.Span.FrameEndIndex)
        /// </code>
        /// </summary>
        public long LargestActiveFrame { get; private set; }

        /// <summary>
        /// The previous value of <see cref="SmallestActiveFrame"/> before it changed
        /// </summary>
        public long PreviousSmallestActiveFrame { get; private set; }

        /// <summary>
        /// The previous version of <see cref="LargestActiveFrame"/> before it changed
        /// </summary>
        public long PreviousLargestActiveFrame { get; private set; }

        /// <summary>
        /// Called when a clip is added, removed or its span changed
        /// </summary>
        public event ClipRangeCacheEventHandler FrameDataChanged;

        public ClipRangeCache()
        {
            this.Map = new SortedList<long, ClipList>();
        }

        public Clip GetPrimaryClipAt(long frame)
        {
            if (!this.Map.TryGetValue(GetIndex(frame), out ClipList list))
            {
                return null;
            }

            for (int i = list.size - 1; i >= 0; i--)
            {
                Clip clip = list.items[i];
                if (clip.IntersectsFrameAt(frame))
                {
                    return clip;
                }
            }

            return null;
        }

        public void GetClipsInRange(List<Clip> dstList, FrameSpan span)
        {
            long idxA = GetIndex(span.Begin);
            long idxB = GetIndex(span.EndIndex);
            for (long idx = idxA; idx <= idxB; idx++)
            {
                if (!this.Map.TryGetValue(idx, out ClipList list))
                {
                    continue;
                }

                for (int i = list.size - 1; i >= 0; i--)
                {
                    Clip clip = list.items[i];
                    if (clip.FrameSpan.Intersects(span))
                        dstList.Add(clip);
                }
            }
        }

        public void GetClipsAtFrame(List<Clip> dstList, long frame)
        {
            if (this.Map.TryGetValue(GetIndex(frame), out ClipList list))
            {
                for (int i = list.size - 1; i >= 0; i--)
                {
                    Clip clip = list.items[i];
                    if (clip.FrameSpan.Intersects(frame))
                        dstList.Add(clip);
                }
            }
        }

        public void OnClipAdded(Clip clip) => this.Add(clip);

        public void OnClipRemoved(Clip clip) => this.Remove(clip.FrameSpan, clip);

        public void Add(Clip clip)
        {
            FrameSpan span = clip.FrameSpan;
            GetRange(span, out long a, out long b);
            this.AddClipInRange(clip, a, b);
            this.PreviousSmallestActiveFrame = this.SmallestActiveFrame;
            this.SmallestActiveFrame = Math.Min(this.SmallestActiveFrame, span.Begin);
            this.PreviousLargestActiveFrame = this.LargestActiveFrame;
            this.LargestActiveFrame = Math.Max(this.LargestActiveFrame, span.EndIndex);
            this.FrameDataChanged?.Invoke(this);
        }

        public void Remove(FrameSpan location, Clip clip)
        {
            GetRange(location, out long a, out long b);
            this.RemoveClipInRange(clip, a, b);
            this.ProcessSmallestAndLargestFrame();
        }

#region Processor functions

        private void AddClipInRange(Clip clip, long min, long max)
        {
            for (long frame = min; frame <= max; frame++)
            {
                if (!this.Map.TryGetValue(frame, out ClipList list))
                    this.Map[frame] = list = new ClipList();
                else if (list.Contains(clip))
                    throw new Exception("Did not expect clip to already exist in list");
                list.Add(clip);
            }
        }

        private void RemoveClipInRange(Clip clip, long min, long max)
        {
            for (long i = min; i <= max; i++)
            {
                int index = this.Map.IndexOfKey(i);
                if (index != -1)
                {
                    ClipList list = this.Map.Values[index];
                    if (list.RemoveClipAndGetIsEmpty(clip))
                    {
                        this.Map.RemoveAt(index);
                    }
                }
                else
                {
                    throw new Exception("Expected ClipList to exist at index: " + i);
                }
            }
        }

        public void OnSpanChanged(Clip clip, FrameSpan oldSpan)
        {
            FrameSpan newSpan = clip.FrameSpan;
            if (oldSpan == newSpan)
            {
                return;
            }

            GetRange(oldSpan, out long oldA, out long oldB);
            GetRange(newSpan, out long newA, out long newB);
            if (oldA == newA && oldB == newB)
            {
                // ClipList list = this.Map[oldA];
                // list.OnClipSpanChanged(clip, oldSpan);
            }

            for (long frame = oldA; frame <= oldB; frame++)
            {
                if (this.Map[frame].RemoveClipAndGetIsEmpty(clip))
                {
                    this.Map.Remove(frame);
                }
            }

            // Add the clip to the new grouped range
            for (long frame = newA; frame <= newB; frame++)
            {
                if (!this.Map.TryGetValue(frame, out ClipList list))
                {
                    this.Map[frame] = list = new ClipList();
                }

                list.Add(clip);
            }

            this.ProcessSmallestAndLargestFrame();
        }

#endregion

        private void ProcessSmallestAndLargestFrame()
        {
            long min = 0, max = 0;
            int index = this.Map.Count - 1;
            if (index >= 0)
            {
                ClipList list = this.Map.Values[index];
                for (int i = 0; i < list.size; i++)
                {
                    max = Math.Max(list.items[i].FrameSpan.EndIndex, max);
                }

                min = max;
                list = this.Map.Values[0];
                for (int i = 0; i < list.size; i++)
                {
                    min = Math.Min(list.items[i].FrameSpan.Begin, min);
                    if (min < 1)
                    {
                        break;
                    }
                }
            }

            this.PreviousSmallestActiveFrame = this.SmallestActiveFrame;
            this.SmallestActiveFrame = min;
            this.PreviousLargestActiveFrame = this.LargestActiveFrame;
            this.LargestActiveFrame = max;
            this.FrameDataChanged?.Invoke(this);
        }

#region Util functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRange(FrameSpan span, out long a, out long b)
        {
            a = GetIndex(span.Begin);
            b = GetIndex(span.EndIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetIndex(long frame) => frame >> 7;

#endregion

        public bool IsRegionEmpty(FrameSpan span)
        {
            GetRange(span, out long a, out long b);
            for (long i = a; i <= b; i++)
            {
                if (this.Map.TryGetValue(i, out ClipList list) && IntersectsAny(list, span))
                    return false;
            }

            return true;
        }

        private static bool IntersectsAny(ClipList list, FrameSpan span)
        {
            for (int j = list.size - 1; j >= 0; j--)
            {
                if (list.items[j].FrameSpan.Intersects(span))
                    return true;
            }

            return false;
        }

        private class ClipList
        {
            private const int DefaultCapacity = 4;
            public Clip[] items;
            public int size;
            private static readonly Clip[] EmptyArray = new Clip[0];

            public ClipList() => this.items = EmptyArray;

            public void Add(Clip item)
            {
                if (this.size == this.items.Length)
                    this.EnsureCapacity(this.size + 1);
                this.items[this.size++] = item;
                // this.Insert(CollectionUtils.GetSortInsertionIndex(this.items, 0, this.size - 1, item, OrderByBegin), item);
            }

            public int IndexOf(Clip item)
            {
                Clip[] array = this.items;
                for (int i = array.Length - 1; i >= 0; i--)
                {
                    Clip clip = array[i];
                    if (item == clip)
                        return i;
                }

                return -1;
            }

            public bool Contains(Clip item) => this.IndexOf(item) != -1;

            private void EnsureCapacity(int min)
            {
                int length = this.items.Length;
                if (length >= min)
                    return;
                int num = length == 0 ? DefaultCapacity : length * 2;
                if (num > 0x7FEFFFFF)
                    num = 0x7FEFFFFF;
                if (num < min)
                    num = min;

                if (num < this.size)
                    throw new Exception("List is too large to increase capacity");
                if (num == length)
                    return;

                Clip[] objArray = new Clip[num];
                if (this.size > 0)
                    Array.Copy(this.items, 0, objArray, 0, this.size);
                this.items = objArray;
            }

            public bool RemoveClipAndGetIsEmpty(Clip item)
            {
                int index = this.IndexOf(item);
                if (index == -1)
                    throw new Exception("Expected item to exist in list");
                this.RemoveAt(index);
                return this.size == 0;
            }

            private void Insert(int index, Clip item)
            {
                if (index > this.size)
                    throw new Exception("Index out of bounds");
                if (this.size == this.items.Length)
                    this.EnsureCapacity(this.size + 1);
                if (index < this.size)
                    Array.Copy(this.items, index, this.items, index + 1, this.size - index);
                this.items[index] = item;
                ++this.size;
            }

            private void RemoveAt(int index)
            {
                if (index >= this.size)
                    throw new Exception("Index out of bounds");
                --this.size;
                if (index < this.size)
                    Array.Copy(this.items, index + 1, this.items, index, this.size - index);
                this.items[this.size] = null;
            }
        }
    }
}