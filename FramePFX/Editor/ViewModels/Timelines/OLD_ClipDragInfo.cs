using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines {
    public class ClipDragInfo {
        public readonly TimelineViewModel timeline;
        public readonly ClipDragInfoData source;
        public readonly List<ClipDragInfoData> clips;

        public bool IsDraggingBody, IsDraggingLeft, IsDraggingRight;

        private ClipDragInfo(TimelineViewModel timeline, ClipViewModel source, IEnumerable<ClipViewModel> selectionWithoutSource) {
            this.timeline = timeline;
            this.source = new ClipDragInfoData(this, source);
            this.clips = new List<ClipDragInfoData> {this.source};
            this.clips.AddRange(selectionWithoutSource.Select(x => new ClipDragInfoData(this, x)));
        }

        public static ClipDragInfo FromClip(ClipViewModel source) {
            TimelineViewModel timeline = source.Timeline ?? throw new Exception("No timeline available from clip");
            List<ClipViewModel> clips = new List<ClipViewModel>();
            foreach (TrackViewModel track in timeline.Tracks) {
                clips.AddRange(track.SelectedClips);
            }

            int index = clips.IndexOf(source);
            if (index >= 0)
                clips.RemoveAt(index);
            return new ClipDragInfo(timeline, source, clips);
        }
    }

    public class ClipDragInfoData {
        public readonly ClipDragInfo drag;
        public readonly ClipViewModel clip;
        public FrameSpan OriginalSpan;
        public FrameSpan DragBody, DragLeft, DragRight;
        public long excess;

        public ClipDragInfoData(ClipDragInfo drag, ClipViewModel clip) {
            this.drag = drag;
            this.clip = clip;
            this.OriginalSpan = clip.FrameSpan;
            this.DragBody = this.DragLeft = this.DragRight = clip.FrameSpan;
        }
    }
}