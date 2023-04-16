using System;
using System.Collections.Generic;
using FramePFX.Timeline.ViewModels.Clips;

namespace FramePFX.Timeline.ViewModels.Layer.Removals {
    public class VideoClipRangeRemoval {
        public List<TimelineVideoClip> RemovedClips { get; }
        public List<VideoClipModification> ModifiedClips { get; }

        public VideoClipRangeRemoval(List<TimelineVideoClip> removedClips, List<VideoClipModification> modifiedClips) {
            this.RemovedClips = removedClips ?? throw new ArgumentNullException(nameof(removedClips), "removedClips list cannot be null");
            this.ModifiedClips = modifiedClips ?? throw new ArgumentNullException(nameof(modifiedClips), "modifiedClips list cannot be null");
        }

        public VideoClipRangeRemoval() {
            this.RemovedClips = new List<TimelineVideoClip>();
            this.ModifiedClips = new List<VideoClipModification>();
        }
    }
}