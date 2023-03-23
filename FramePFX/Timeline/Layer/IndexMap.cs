using System.Collections.Generic;

namespace FramePFX.Timeline.Layer {
    public class ClipIndexMap<T> {
        public Dictionary<T, long> ClipToFrame { get; }
        public Dictionary<long, T> FrameToClip { get; }
        public Dictionary<T, int> ClipToIndex { get; }
        public Dictionary<int, T> IndexToClip { get; }
        public List<T> OrderedClips { get; }

        public ClipIndexMap(Dictionary<T, long> clipToFrame, Dictionary<long, T> frameToClip, Dictionary<T, int> clipToIndex, Dictionary<int, T> indexToClip, List<T> orderedClips) {
            this.ClipToFrame = clipToFrame;
            this.FrameToClip = frameToClip;
            this.ClipToIndex = clipToIndex;
            this.IndexToClip = indexToClip;
            this.OrderedClips = orderedClips;
        }

        public int IndexOf(T clip) {
            return this.ClipToIndex.TryGetValue(clip, out int index) ? index : -1;
        }
    }
}