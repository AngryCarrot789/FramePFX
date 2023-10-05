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
        private readonly SortedList<long, List<Clip>> Map;


        public long SmallestActiveFrame { get; private set; }
        public long LargestActiveFrame { get; private set; }

        public long PreviousSmallestActiveFrame { get; private set; }
        public long PreviousLargestActiveFrame { get; private set; }

        public ClipRangeCache() {
            this.Map = new SortedList<long, List<Clip>>();
        }

        public Clip GetPrimaryClipAt(long frame) {
            if (!this.Map.TryGetValue(GetIndex(frame), out List<Clip> list)) {
                return null;
            }

            for (int i = list.Count - 1; i >= 0; i--) {
                Clip clip = list[i];
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
                if (!this.Map.TryGetValue(frame, out List<Clip> list))
                    this.Map[frame] = list = new List<Clip>();
                else if (list.Contains(clip))
                    continue; // ???
                list.Add(clip);
            }
        }

        private void RemoveClipInRange(Clip clip, long min, long max) {
            for (long frame = min; frame <= max; frame++) {
                int index = this.Map.IndexOfKey(frame);
                if (index != -1) {
                    List<Clip> list = this.Map.Values[index];
                    if (list.Remove(clip) && list.Count < 1) {
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
                    if (this.Map.TryGetValue(frame, out List<Clip> list)) {
                        list.Remove(clip);
                        if (list.Count == 0) {
                            this.Map.Remove(frame);
                        }
                    }
                }

                // Add the clip to the new grouped range
                for (long frame = newA; frame <= newB; frame++) {
                    if (!this.Map.TryGetValue(frame, out List<Clip> list)) {
                        this.Map[frame] = list = new List<Clip>();
                    }

                    list.Add(clip);
                }
            }

            this.ProcessSmallestAndLargestFrame();

            // if (oldSpan != newSpan) {
            //     long oldA = GetIndex(oldSpan.Begin);
            //     long oldB = GetIndex(oldSpan.EndIndex);
            //     long newA = GetIndex(newSpan.Begin);
            //     long newB = GetIndex(newSpan.EndIndex);
            //     this.RemoveClipInRange(clip, oldA, oldB);
            //     this.AddClipInRange(clip, newA, newB);
            //     this.ProcessLargestFrame();
            // }
        }

        public void MakeTopMost(Clip clip) {
            if (this.Map.TryGetValue(GetIndex(clip.FrameBegin), out List<Clip> list)) {
                int index = list.IndexOf(clip);
                if (index == -1) {
                    throw new Exception("Clip does not exist in cache mapped list");
                }

                list.MoveItem(index, list.Count - 1);
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
                foreach (Clip clp in this.Map.Values[index])
                    max = Math.Max(clp.FrameEndIndex, max);

                min = max;
                foreach (Clip clp in this.Map.Values[0]) {
                    min = Math.Min(clp.FrameBegin, min);
                    if (min < 1)
                        break;
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
                if (this.Map.TryGetValue(i, out List<Clip> list) && IntersectsAny(list, span))
                    return false;
            }

            return true;
        }

        private static bool IntersectsAny(List<Clip> list, FrameSpan span) {
            for (int j = list.Count - 1; j >= 0; j--) {
                if (list[j].FrameSpan.Intersects(span))
                    return true;
            }

            return false;
        }
    }
}