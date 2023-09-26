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
    public class ClipRangeCache_OLD {
        // This was before swapping to SortedList, but I couldn't get it to work properly
        // private readonly Dictionary<long, (int, List<Clip>)> map;
        // private readonly List<long> SortedKeys;
        private readonly SortedList<long, List<Clip>> Map;
        private bool isProcessingLocationChange;

        /// <summary>
        /// Gets the largest frame that this cache is currently storing (updated when clips changed, are added or
        /// removed or so on). This is the end index of the clip furthest from the start of the timeline
        /// </summary>
        public long LargestActiveFrame { get; private set; }

        public long PreviousLargestActiveFrame { get; private set; }

        public ClipRangeCache_OLD() {
            // this.map = new Dictionary<long, (int, List<Clip>)>();
            // this.SortedKeys = new List<long>();
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

        private static bool ProcessDualClip(long frame, ref Clip a, ref Clip b) {
            if (a.IntersectsFrameAt(frame)) {
                if (b.IntersectsFrameAt(frame)) {
                    if (a.FrameBegin > b.FrameBegin) {
                        Clip A = a; a = b; b = A;
                    }

                    return true;
                }
                else {
                    b = null;
                    return true;
                }
            }
            else if (b.IntersectsFrameAt(frame)) {
                a = b;
                b = null;
                return true;
            }
            else {
                return false;
            }
        }

        // Doesn't work because this neesd to use FrameBegin and
        // FrameEndIndex which relies on accessing the multiple lists possibly

        public bool GetClipsAtFrame(long frame, out Clip a, out Clip b) {
            int count;
            if (!this.Map.TryGetValue(GetIndex(frame), out List<Clip> list) || (count = list.Count) < 1) {
                a = b = null;
                return false;
            }

            switch (count) {
                case 1: {
                    a = list[0];
                    b = null;
                    return a.IntersectsFrameAt(frame);
                }
                case 2: {
                    a = list[0];
                    b = list[1];
                    return ProcessDualClip(frame, ref a, ref b);
                }
                default: {
                    Clip clip, min = list[0], max = min;
                    long fMin = min.FrameBegin, fMax = min.FrameEndIndex;
                    for (int i = 1; i < count; i++) {
                        clip = list[i];
                        FrameSpan span = clip.FrameSpan;
                        if (span.Begin >= frame && span.Begin < fMin) {
                            fMin = span.Begin;
                            min = clip;
                        }

                        long endIndex = span.EndIndex;
                        if (endIndex < frame && endIndex > fMax) {
                            fMax = endIndex;
                            max = clip;
                        }
                    }

                    if (min == max) {
                        // long clamp = fMin;
                        for (int i = 0; i < count; i++) {
                            clip = list[i];
                            FrameSpan span = clip.FrameSpan;
                            long endIndex = span.EndIndex;
                            if (clip != max && frame >= span.Begin /* && frame > clamp */ && span.Begin > fMin && endIndex <= fMax) {
                                // clamp = endIndex;
                                max = clip;
                            }
                        }
                    }

                    a = min;
                    b = max;
                    return ProcessDualClip(frame, ref min, ref max);
                }
            }
        }

        public void OnClipAdded(Clip clip) => this.Add(clip);

        public void OnClipRemoved(Clip clip) => this.Remove(clip.FrameSpan, clip);

        #region Processor functions

        public void AddClipInRange(Clip clip, long a, long b) {
            for (long frame = a; frame <= b; frame++) {
                if (!this.Map.TryGetValue(frame, out List<Clip> list)) {
                    this.Map[frame] = list = new List<Clip>();
                }
                else if (list.Contains(clip)) {
                    continue; // ???
                }

                list.Add(clip);
            }

            // for (long i = a; i <= b; i++) {
            //     if (!this.map.TryGetValue(i, out List<Clip> list)) {
            //         this.map[i] = pair = (this.SortedKeys.Count, new List<Clip>());
            //         int keyIndex = CollectionUtils.GetSortInsertionIndex(this.SortedKeys, i, (pA, pB) => pA.CompareTo(pB));
            //         this.SortedKeys.Insert(keyIndex, i);
            //     }
            //     else if (pair.list.Contains(clip)) {
            //         continue; // ???
            //     }

            //     pair.list.Add(clip);
            // }
        }

        public void RemoveClipInRange(Clip clip, long a, long b) {
            for (long frame = a; frame <= b; frame++) {
                int index = this.Map.IndexOfKey(frame);
                if (index != -1) {
                    List<Clip> list = this.Map.Values[index];
                    if (list.Remove(clip) && list.Count < 1) {
                        this.Map.RemoveAt(index);
                    }
                }
            }

            // for (long i = a; i <= b; i++) {
            //     if (this.map.TryGetValue(i, out List<Clip> list)) {
            //         if (pair.list.Remove(clip) && pair.list.Count < 1) {
            //             for (int j = this.SortedKeys.Count - 1; j > pair.index; j--) {
            //                 long idx = this.SortedKeys[j];
            //                 this.SortedKeys[j]--;
            //             }
            //             // for (int j = this.SortedKeys.Count - 1; j > pair.index; j--) {
            //             //     this.SortedKeys.RemoveAt(j);
            //             // }
            //             long expected = this.SortedKeys[pair.index];
            //             this.SortedKeys.RemoveAt(pair.index);
            //             this.map.Remove(i);
            //             // for (int j = this.SortedKeys.Count - 1; j > pair.index; j--) {
            //             //     this.SortedKeys.RemoveAt(j);
            //             // }
            //             // if (expected != i) {
            //             //     throw new Exception("Clip range cache is corrupted");
            //             // }
            //             // this.SortedKeys.RemoveAt(pair.index);
            //         }
            //     }
            // }
        }

        public void Add(Clip clip) {
            FrameSpan span = clip.FrameSpan;
            GetRange(span, out long a, out long b);
            this.AddClipInRange(clip, a, b);
            this.PreviousLargestActiveFrame = this.LargestActiveFrame;
            this.LargestActiveFrame = Math.Max(this.LargestActiveFrame, span.EndIndex);
        }

        public void Remove(FrameSpan location, Clip clip) {
            GetRange(location, out long a, out long b);
            this.RemoveClipInRange(clip, a, b);
            this.ProcessLargestFrame();
        }

        public void OnLocationChanged(Clip clip, FrameSpan oldSpan) {
            FrameSpan newSpan = clip.FrameSpan;
            if (oldSpan != clip.FrameSpan) {
                GetRange(oldSpan, out long oldA, out long oldB);
                GetRange(newSpan, out long newA, out long newB);
                // if (oldA > newB || oldB < newA) {
                //     // No overlap between old and new ranges, perform removal and addition separately
                //     this.RemoveClipInRange(clip, oldA, oldB);
                //     this.AddClipInRange(clip, newA, newB);
                // }
                // else {
                //     // There is an overlap, update the common range
                //     long minA = Math.Min(oldA, newA);
                //     long maxB = Math.Max(oldB, newB);
                //     // Remove the value from the parts of the old range that are not in the new range
                //     for (long i = oldA; i < newA; i++) {
                //         this.RemoveClipInRange(clip, i, i);
                //     }
                //     for (long i = newB + 1; i <= oldB; i++) {
                //         this.RemoveClipInRange(clip, i, i);
                //     }
                //     // Add the value to the parts of the new range that are not in the old range
                //     for (long i = minA; i < newA; i++) {
                //         this.AddClipInRange(clip, i, i);
                //     }
                //     for (long i = newB + 1; i <= maxB; i++) {
                //         this.AddClipInRange(clip, i, i);
                //     }
                // }

                // if (oldA != newA || oldB != newB) {
                //     this.RemoveClipInRange(clip, oldA, oldB);
                //     this.AddClipInRange(clip, newA, newB);
                // }

                // Calculate the X coordinate range of the old and new positions

                {
                    if (newA > oldA) {
                        this.RemoveClipInRange(clip, oldA, newA);
                    }
                }

                {
                    if (newB >= newA) {
                        this.AddClipInRange(clip, newB, newA);
                    }
                }

                // if (newA >= oldA) {
                //     // A moved to the right
//
                // }
                // else {
                //     // A moved to the left
                //     long max = Math.Max()
                // }

                // this.RemoveClipInRange(clip, oldA, oldB);
                // this.AddClipInRange(clip, newA, newB);
                this.ProcessLargestFrame();
            }
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

        private void ProcessLargestFrame() {
            long max = 0;
            int index = this.Map.Count - 1;
            if (index >= 0) {
                foreach (Clip clp in this.Map.Values[index])
                    max = Math.Max(clp.FrameEndIndex, max);
            }

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
    }
}