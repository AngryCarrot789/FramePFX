using System;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Editor.ViewModels.Timeline.Removals;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Layers {
    public class VideoLayerViewModel : TimelineLayerViewModel {
        public new VideoLayerModel Model => (VideoLayerModel) base.Model;

        public VideoLayerViewModel(TimelineViewModel timeline, VideoLayerModel model) : base(timeline, model) {

        }

        public override Task SliceClipAction(ClipViewModel clip, long frame) {
            // TODO: Implement factory method to clone clips
            // And maybe use a virtual function that clips override, to set specific data for a cloned clip?
            // (e.g. base virtual function sets media pos/scale/origin,
            // and an image or video clip will try to set the target resource path)

            if (clip is ImageClipViewModel image) {
                ImageClipModel cloneModel = new ImageClipModel();
                ResourcePath<ResourceImage> imgPath = image.Model.ResourcePath;
                if (imgPath?.UniqueId != null) {
                    cloneModel.SetTargetResourceId(imgPath.UniqueId);
                }

                ImageClipViewModel clone = new ImageClipViewModel(cloneModel) {
                    Span = ClipSpan.FromIndex(frame, image.FrameEndIndex),
                    MediaPosition = image.MediaPosition,
                    MediaScale = image.MediaScale,
                    MediaScaleOrigin = image.MediaScaleOrigin
                };

                image.FrameEndIndex = frame;
                this.AddClipToLayer(clone);
            }
            else if (clip is SquareClipViewModel square) {
                SquareClipModel cloneModel = new SquareClipModel() {
                    Width = square.Width,
                    Height = square.Height,
                };

                ResourcePath<ResourceARGB> imgPath = square.Model.ResourcePath;
                if (imgPath?.UniqueId != null) {
                    cloneModel.SetTargetResourceId(imgPath.UniqueId);
                }

                SquareClipViewModel clone = new SquareClipViewModel(cloneModel) {
                    Span = ClipSpan.FromIndex(frame, square.FrameEndIndex),
                    MediaPosition = square.MediaPosition,
                    MediaScale = square.MediaScale,
                    MediaScaleOrigin = square.MediaScaleOrigin
                };

                square.FrameEndIndex = frame;
                this.AddClipToLayer(clone);
            }
            else {
                throw new Exception($"Unsupported clip to slice: {clip}");
            }

            return Task.CompletedTask;
        }

        public VideoClipRangeRemoval GetRangeRemoval(long spanBegin, long spanDuration) {
            if (spanDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(spanDuration), "Span duration cannot be negative");
            long spanEnd = spanBegin + spanDuration;
            VideoClipRangeRemoval range = new VideoClipRangeRemoval();
            foreach (ClipViewModel clipViewModel in this.Clips) {
                if (clipViewModel is VideoClipViewModel clip) {
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
                            range.AddSplitClip(clip, null, ClipSpan.FromIndex(spanEnd, clipEnd));
                        }
                    }
                    else if (spanEnd >= clipEnd) { // cut the right part away
                        if (spanBegin >= clipEnd) { // not intersecting
                            continue;
                        }
                        else {
                            range.AddSplitClip(clip, ClipSpan.FromIndex(clipBegin, spanBegin), null);
                        }
                    }
                    else { // fully intersecting; double split
                        range.AddSplitClip(clip, ClipSpan.FromIndex(clipBegin, spanBegin), ClipSpan.FromIndex(spanEnd, clipEnd));
                    }
                }
            }
            return range;
        }

        public void SplitClip(VideoClipViewModel clip, ClipSpan left, ClipSpan right) {
            throw new NotImplementedException("TODO: implement cloning clips, might need to be async too to confirm user actions, e.g. use resource by ref or copy resource");
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