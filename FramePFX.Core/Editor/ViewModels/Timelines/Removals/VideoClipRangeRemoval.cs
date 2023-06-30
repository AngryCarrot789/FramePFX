using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Removals {
    public class VideoClipRangeRemoval {
        public List<VideoClipCut> ModifiedClips { get; }

        public IEnumerable<VideoClipViewModel> RemovedClips => this.ModifiedClips.Where(x => x.IsClipRemoved).Select(x => x.Clip);

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
                    throw new NotImplementedException();
                    // cut.Clip.Clip.SplitClip(cut.Clip, cutLeft.Value, cutRight.Value);
                }
                else if (cutLeft.HasValue) { // make clip take up the left span
                    cut.Clip.FrameSpan = cutLeft.Value;
                }
                else if (cutRight.HasValue) { // make clip take up the right span
                    cut.Clip.FrameSpan = cutRight.Value;
                }
                else { // remove clip
                    throw new NotImplementedException();
                    // cut.Clip.Clip.RemoveClip(cut.Clip);
                }
            }
        }

        public void AddRemovedClip(VideoClipViewModel clip) {
            this.ModifiedClips.Add(new VideoClipCut(clip.FrameSpan, null, null, clip));
        }

        public void AddSplitClip(VideoClipViewModel clip, FrameSpan? a, FrameSpan? b) {
            this.ModifiedClips.Add(new VideoClipCut(clip.FrameSpan, a, b, clip));
        }
    }
}