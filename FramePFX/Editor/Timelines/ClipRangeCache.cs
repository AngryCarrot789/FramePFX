using System;
using System.Collections.Generic;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// A class that caches clips in chunks in order to make lookup by frame index faster, due to track clips being unordered
    /// </summary>
    public class ClipRangeCache {
        // Uses chunks of 128 (0-127), 128-255, etc.
        private readonly Dictionary<long, List<Clip>> map;

        public ClipRangeCache() {
            this.map = new Dictionary<long, List<Clip>>();
        }

        public IEnumerable<Clip> GetIntersectingClips(long frame) {
            if (!this.map.TryGetValue(GetIndex(frame), out var list)) {
                yield break;
            }

            foreach (Clip clip in list) {
                if (clip.IntersectsFrameAt(frame)) {
                    yield return clip;
                }
            }
        }

        public Clip GetPrimaryClipAt(long frame) {
            if (!this.map.TryGetValue(GetIndex(frame), out var list)) {
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
            if (!this.map.TryGetValue(GetIndex(frame), out List<Clip> list) || (count = list.Count) < 1) {
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

        public void OnClipAdded(Clip clip) => this.Add(clip.FrameSpan, clip);

        public void OnClipRemoved(Clip clip) => this.Remove(clip.FrameSpan, clip);

        public void OnLocationChanged(Clip clip, FrameSpan oldSpan) {
            FrameSpan span = clip.FrameSpan;
            if (oldSpan != clip.FrameSpan) {
                this.Remove(oldSpan, clip);
                this.Add(span, clip);
            }
        }

        public void Remove(FrameSpan span, Clip clip) {
            GetRange(span, out long a, out long b);
            for (long i = a; i <= b; i++) {
                if (this.map.TryGetValue(i, out List<Clip> list)) {
                    list.Remove(clip);
                }
            }
        }

        public void Add(FrameSpan span, Clip clip) {
            GetRange(span, out long a, out long b);
            for (long i = a; i <= b; i++) {
                if (!this.map.TryGetValue(i, out List<Clip> list)) {
                    this.map[i] = list = new List<Clip>();
                }
                else if (list.Contains(clip)) {
                    continue;
                }

                list.Add(clip);
            }
        }

        public static void GetRange(FrameSpan span, out long a, out long b) {
            a = GetIndex(span.Begin);
            b = GetIndex(span.EndIndex);
        }

        public static long GetIndex(long frame) => frame >> 7;

        public void MakeTopMost(Clip clip) {
            if (this.map.TryGetValue(GetIndex(clip.FrameBegin), out List<Clip> list)) {
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
    }
}