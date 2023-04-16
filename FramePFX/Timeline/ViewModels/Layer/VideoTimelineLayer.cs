using System;
using FramePFX.Core.Utils;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Layer.Removals;

namespace FramePFX.Timeline.ViewModels.Layer {
    public class VideoTimelineLayer : EditorTimelineLayer {
        private float opacity;

        /// <summary>
        /// The opacity of this layer. Between 0f and 1f (not yet implemented properly)
        /// </summary>
        public float Opacity {
            get => this.opacity;
            set => this.RaisePropertyChanged(ref this.opacity, Maths.Clamp(value, 0f, 1f), () => this.Timeline.MarkRenderDirty());
        }

        public VideoTimelineLayer(EditorTimeline timeline) : base(timeline) {
            this.Opacity = 1f;
        }

        public VideoClipRangeRemoval GetRangeRemoval(long spanBegin, long spanDuration) {
            if (spanDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(spanDuration), "Span duration cannot be negative");
            long spanEnd = spanBegin + spanDuration;

            VideoClipRangeRemoval range = new VideoClipRangeRemoval();
            foreach (BaseTimelineClip baseTimelineClip in this.Clips) {
                if (baseTimelineClip is TimelineVideoClip clip) {
                    long clipBegin = clip.FrameBegin;
                    long clipDuration = clip.FrameDuration;
                    long clipEnd = clipBegin + clipDuration;
                    if (clipEnd <= spanBegin && clipBegin >= spanEnd) {
                        continue; // not intersecting
                    }

                    if (spanBegin <= clipBegin) { // cut the left part away
                        if (spanEnd >= clipEnd) {
                            // remove clip entirely
                            range.AddRemovedClip(clip);
                        }
                        else if (spanEnd <= clipBegin) { // not intersecting
                            continue;
                        }
                        else {
                            range.AddSplitClip(clip, null, FrameSpan.FromIndex(spanEnd, clipEnd));
                        }
                    }
                    else if (spanEnd >= clipEnd) { // cut the right part away
                        if (spanBegin >= clipEnd) { // not intersecting
                            continue;
                        }
                        else {
                            range.AddSplitClip(clip, FrameSpan.FromIndex(clipBegin, spanBegin), null);
                        }
                    }
                    else { // fully intersecting; double split
                        range.AddSplitClip(clip, FrameSpan.FromIndex(clipBegin, spanBegin), FrameSpan.FromIndex(spanEnd, clipEnd));
                    }
                }
            }

            return range;
        }

        public void SplitClip(TimelineVideoClip clip, FrameSpan left, FrameSpan right) {
            TimelineVideoClip rightClone = (TimelineVideoClip) clip.CloneInstance();
            clip.Span = left;
            this.clips.Add(rightClone);
            rightClone.Span = right;
        }
    }
}