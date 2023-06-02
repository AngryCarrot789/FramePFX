using System;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Utils;
using FramePFX.Editor.Timeline.ViewModels.Clips;
using FramePFX.Editor.Timeline.ViewModels.Layer.Removals;

namespace FramePFX.Editor.Timeline.ViewModels.Layer {
    public class PFXVideoLayer : PFXTimelineLayer {
        private float opacity;

        /// <summary>
        /// The opacity of this layer. Between 0f and 1f (not yet implemented properly)
        /// </summary>
        public float Opacity {
            get => this.opacity;
            set {
                this.RaisePropertyChanged(ref this.opacity, Maths.Clamp(value, 0f, 1f));
                this.Timeline.ScheduleRender(false);
            }
        }

        public PFXVideoLayer(PFXTimeline timeline) : base(timeline) {
            this.Opacity = 1f;
        }

        public VideoClipRangeRemoval GetRangeRemoval(long spanBegin, long spanDuration) {
            if (spanDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(spanDuration), "Span duration cannot be negative");
            long spanEnd = spanBegin + spanDuration;

            VideoClipRangeRemoval range = new VideoClipRangeRemoval();
            foreach (PFXClipViewModel baseTimelineClip in this.Clips) {
                if (baseTimelineClip is PFXVideoClipViewModel clip) {
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

        public void SplitClip(PFXVideoClipViewModel clip, FrameSpan left, FrameSpan right) {
            PFXVideoClipViewModel rightClone = (PFXVideoClipViewModel) clip.NewInstanceOverride();
            clip.Span = left;
            this.clips.Add(rightClone);
            rightClone.Span = right;
        }

        public override PFXClipViewModel SliceClip(PFXClipViewModel clip, long frame) {
            if (!(clip is PFXVideoClipViewModel videoClip)) {
                throw new ArgumentException("Clip is not a video clip");
            }

            if (frame == videoClip.FrameBegin || frame == videoClip.FrameEndIndex) {
                return null;
            }

            long endIndex = videoClip.FrameEndIndex;
            PFXVideoClipViewModel clone = (PFXVideoClipViewModel) videoClip.NewInstanceOverride();
            videoClip.FrameEndIndex = frame;
            clone.FrameBegin = frame;
            clone.FrameEndIndex = endIndex;
            this.AddClip(clone);
            return clone;
        }
    }
}