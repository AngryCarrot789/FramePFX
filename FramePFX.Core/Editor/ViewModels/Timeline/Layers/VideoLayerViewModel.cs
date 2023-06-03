using System;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Layers {
    public class VideoLayerViewModel : TimelineLayerViewModel {
        public new VideoLayerModel Model => (VideoLayerModel) base.Model;

        public VideoLayerViewModel(TimelineViewModel timeline, VideoLayerModel model) : base(timeline, model) {

        }

        // public VideoClipRangeRemoval GetRangeRemoval(long spanBegin, long spanDuration) {
        //     if (spanDuration < 0)
        //         throw new ArgumentOutOfRangeException(nameof(spanDuration), "Span duration cannot be negative");
        //     long spanEnd = spanBegin + spanDuration;
        //     VideoClipRangeRemoval range = new VideoClipRangeRemoval();
        //     foreach (VideoClipViewModel baseTimelineClip in this.Clips) {
        //         if (baseTimelineClip is VideoClipViewModel clip) {
        //             long clipBegin = clip.FrameBegin;
        //             long clipDuration = clip.FrameDuration;
        //             long clipEnd = clipBegin + clipDuration;
        //             if (clipEnd <= spanBegin && clipBegin >= spanEnd) {
        //                 continue; // not intersecting
        //             }
        //             if (spanBegin <= clipBegin) { // cut the left part away
        //                 if (spanEnd >= clipEnd) {
        //                     // remove clip entirely
        //                     range.AddRemovedClip(clip);
        //                 }
        //                 else if (spanEnd <= clipBegin) { // not intersecting
        //                     continue;
        //                 }
        //                 else {
        //                     range.AddSplitClip(clip, null, ClipSpan.FromIndex(spanEnd, clipEnd));
        //                 }
        //             }
        //             else if (spanEnd >= clipEnd) { // cut the right part away
        //                 if (spanBegin >= clipEnd) { // not intersecting
        //                     continue;
        //                 }
        //                 else {
        //                     range.AddSplitClip(clip, ClipSpan.FromIndex(clipBegin, spanBegin), null);
        //                 }
        //             }
        //             else { // fully intersecting; double split
        //                 range.AddSplitClip(clip, ClipSpan.FromIndex(clipBegin, spanBegin), ClipSpan.FromIndex(spanEnd, clipEnd));
        //             }
        //         }
        //     }
        //     return range;
        // }

        public void SplitClip(VideoClipViewModel clip, ClipSpan left, ClipSpan right) {
            throw new NotImplementedException();
            // VideoClipViewModel rightClone = (VideoClipViewModel) clip.NewInstanceOverride();
            // clip.Span = left;
            // this.clips.Add(rightClone);
            // rightClone.Span = right;
        }

        public virtual VideoClipViewModel SliceClip(VideoClipViewModel clip, long frame) {
            if (!(clip is VideoClipViewModel videoClip)) {
                throw new ArgumentException("Clip is not a video clip");
            }

            if (frame == videoClip.FrameBegin || frame == videoClip.FrameEndIndex) {
                return null;
            }

            throw new NotImplementedException();

            // long endIndex = videoClip.FrameEndIndex;
            // VideoClipViewModel clone = (VideoClipViewModel) videoClip.NewInstanceOverride();
            // videoClip.FrameEndIndex = frame;
            // clone.FrameBegin = frame;
            // clone.FrameEndIndex = endIndex;
            // this.AddClip(clone);
            // return clone;
        }
    }
}