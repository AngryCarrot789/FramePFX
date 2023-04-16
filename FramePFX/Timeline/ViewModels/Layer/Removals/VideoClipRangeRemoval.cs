using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Timeline.ViewModels.Clips;

namespace FramePFX.Timeline.ViewModels.Layer.Removals {
    public class VideoClipRangeRemoval {
        public List<VideoClipCut> ModifiedClips { get; }

        public IEnumerable<TimelineVideoClip> RemovedClips => this.ModifiedClips.Where(x => x.IsClipRemoved).Select(x => x.Clip);

        public VideoClipRangeRemoval(List<VideoClipCut> modifiedClips) {
            this.ModifiedClips = modifiedClips ?? throw new ArgumentNullException(nameof(modifiedClips), "modifiedClips list cannot be null");
        }

        public VideoClipRangeRemoval() {
            this.ModifiedClips = new List<VideoClipCut>();
        }

        public void ApplyAll() {
            foreach (VideoClipCut cut in this.ModifiedClips) {
                FrameSpan? cutLeft = cut.CutLeft;
                FrameSpan? cutRight = cut.CutRight;
                if (cutLeft.HasValue && cutRight.HasValue) { // double split
                    cut.Clip.VideoLayer.SplitClip(cut.Clip, cutLeft.Value, cutRight.Value);
                }
                else if (cutLeft.HasValue) { // make clip take up the left span
                    cut.Clip.Span = cutLeft.Value;
                }
                else if (cutRight.HasValue) { // make clip take up the right span
                    cut.Clip.Span = cutRight.Value;
                }
                else { // remove clip
                    cut.Clip.Layer.RemoveClip(cut.Clip);
                }
            }
        }

        public void AddRemovedClip(TimelineVideoClip clip) {
            this.ModifiedClips.Add(new VideoClipCut(clip.Span, null, null, clip));
        }

        public void AddSplitClip(TimelineVideoClip clip, FrameSpan? a, FrameSpan? b) {
            this.ModifiedClips.Add(new VideoClipCut(clip.Span, a, b, clip));
        }
    }
}