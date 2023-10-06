#define USE_UNSAFE_FRAME_SORTING

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// A class that caches clips in chunks in order to make lookup by frame index faster, due to track clips being unordered
    /// <para>
    /// Chunks are 128 frames (0-127)
    /// </para>
    /// </summary>
    public class ClipRangeCache {
        private readonly SortedList<long, ClipList> Map;

        public long SmallestActiveFrame { get; private set; }
        public long LargestActiveFrame { get; private set; }

        public long PreviousSmallestActiveFrame { get; private set; }
        public long PreviousLargestActiveFrame { get; private set; }

        public ClipRangeCache() {
            this.Map = new SortedList<long, ClipList>();
        }

        public Clip GetPrimaryClipAt(long frame) {
            if (!this.Map.TryGetValue(GetIndex(frame), out ClipList list)) {
                return null;
            }

            for (int i = list.size - 1; i >= 0; i--) {
                Clip clip = list.items[i];
                if (clip.IntersectsFrameAt(frame)) {
                    return clip;
                }
            }

            return null;
        }

        public void OnClipAdded(Clip clip) => this.Add(clip);

        public void OnClipRemoved(Clip clip) => this.Remove(clip.FrameSpan, clip);

        public void Add(Clip clip) {
            FrameSpan span = clip.FrameSpan;
            GetRange(span, out long a, out long b);
            this.AddClipInRange(clip, a, b);
            this.PreviousSmallestActiveFrame = this.SmallestActiveFrame;
            this.SmallestActiveFrame = Math.Min(this.SmallestActiveFrame, span.Begin);
            this.PreviousLargestActiveFrame = this.LargestActiveFrame;
            this.LargestActiveFrame = Math.Max(this.LargestActiveFrame, span.EndIndex);
        }

        public void Remove(FrameSpan location, Clip clip) {
            GetRange(location, out long a, out long b);
            this.RemoveClipInRange(clip, a, b);
            this.ProcessSmallestAndLargestFrame();
        }

        #region Processor functions

        private void AddClipInRange(Clip clip, long min, long max) {
            for (long frame = min; frame <= max; frame++) {
                if (!this.Map.TryGetValue(frame, out ClipList list))
                    this.Map[frame] = list = new ClipList();
                else if (list.Contains(clip))
                    continue; // ???
                list.Add(clip);
            }
        }

        private void RemoveClipInRange(Clip clip, long min, long max) {
            for (long frame = min; frame <= max; frame++) {
                int index = this.Map.IndexOfKey(frame);
                if (index != -1) {
                    ClipList list = this.Map.Values[index];
                    if (list.RemoveClipAndGetIsEmpty(clip)) {
                        this.Map.RemoveAt(index);
                    }
                }
            }
        }

        public void OnLocationChanged(Clip clip, FrameSpan oldSpan) {
            FrameSpan newSpan = clip.FrameSpan;
            if (oldSpan == newSpan) {
                return;
            }

            GetRange(oldSpan, out long oldA, out long oldB);
            GetRange(newSpan, out long newA, out long newB);
            if (oldA != newA || oldB != newB) {
                for (long frame = oldA; frame <= oldB; frame++) {
                    if (this.Map.TryGetValue(frame, out ClipList list)) {
                        if (list.RemoveClipAndGetIsEmpty(clip)) {
                            this.Map.Remove(frame);
                        }
                    }
                }

                // Add the clip to the new grouped range
                for (long frame = newA; frame <= newB; frame++) {
                    if (!this.Map.TryGetValue(frame, out ClipList list)) {
                        this.Map[frame] = list = new ClipList();
                    }

                    list.Add(clip);
                }
            }

            this.ProcessSmallestAndLargestFrame();
        }

        public void MakeTopMost(Clip clip) {
            if (this.Map.TryGetValue(GetIndex(clip.FrameBegin), out ClipList list)) {
                int index = list.IndexOf(clip);
                if (index == -1) {
                    throw new Exception("Fatal error: clip does not exist in cache mapped list");
                }

                int newIndex = list.size - 1;
                Clip removedItem = list.items[index];
                list.RemoveAt(index);
                list.Insert(newIndex, removedItem);
            }
            else {
                throw new Exception("Clip does not exist in cache");
            }
        }

        #endregion

        private void ProcessSmallestAndLargestFrame() {
            long min = 0, max = 0;
            int index = this.Map.Count - 1;
            if (index >= 0) {
                ClipList list = this.Map.Values[index];
                for (int i = 0; i < list.size; i++) {
                    max = Math.Max(list.items[i].FrameEndIndex, max);
                }

                min = max;
                list = this.Map.Values[0];
                for (int i = 0; i < list.size; i++) {
                    min = Math.Min(list.items[i].FrameBegin, min);
                    if (min < 1) {
                        break;
                    }
                }
            }

            this.PreviousSmallestActiveFrame = this.SmallestActiveFrame;
            this.SmallestActiveFrame = min;

            this.PreviousLargestActiveFrame = this.LargestActiveFrame;
            this.LargestActiveFrame = max;
        }

        #region Util functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRange(FrameSpan span, out long a, out long b) {
            a = GetIndex(span.Begin);
            b = GetIndex(span.EndIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetIndex(long frame) => frame >> 7;

        #endregion

        public bool IsRegionEmpty(FrameSpan span) {
            GetRange(span, out long a, out long b);
            for (long i = a; i <= b; i++) {
                if (this.Map.TryGetValue(i, out ClipList list) && IntersectsAny(list, span))
                    return false;
            }

            return true;
        }

        private static bool IntersectsAny(ClipList list, FrameSpan span) {
            for (int j = list.size - 1; j >= 0; j--) {
                if (list.items[j].FrameSpan.Intersects(span))
                    return true;
            }

            return false;
        }

        private class ClipList {
            private const int DefaultCapacity = 4;
            public Clip[] items;
            public int size;
            private static readonly Clip[] EmptyArray = new Clip[0];

            public ClipList() => this.items = EmptyArray;

            public void Add(Clip item) {
                if (this.size == this.items.Length)
                    this.EnsureCapacity(this.size + 1);
                this.items[this.size++] = item;
            }

            public void Clear() {
                if (this.size > 0) {
                    Array.Clear(this.items, 0, this.size);
                    this.size = 0;
                }
            }

            public bool Contains(Clip item) {
                for (int index = 0; index < this.size; ++index) {
                    if (this.items[index] == item) {
                        return true;
                    }
                }

                return false;
            }

            private void EnsureCapacity(int min) {
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

            public int IndexOf(Clip item) {
                Clip[] array = this.items;
                for (int i = 0; i < this.size; i++) {
                    if (array[i] == item)
                        return i;
                }

                return -1;
            }

            public bool RemoveClipAndGetIsEmpty(Clip item) {
                Clip[] array = this.items;
                int nSz = this.size;
                for (int i = 0; i < nSz; i++) {
                    if (array[i] == item) {
                        this.size = --nSz;
                        if (i < nSz)
                            Array.Copy(array, i + 1, array, i, nSz - i);
                        array[nSz] = null;
                        break;
                    }
                }

                return nSz == 0;
            }

            public void Insert(int index, Clip item) {
                if (index > this.size)
                    throw new Exception("Index out of bounds");
                if (this.size == this.items.Length)
                    this.EnsureCapacity(this.size + 1);
                if (index < this.size)
                    Array.Copy(this.items, index, this.items, index + 1, this.size - index);
                this.items[index] = item;
                ++this.size;
            }

            public void RemoveAt(int index) {
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