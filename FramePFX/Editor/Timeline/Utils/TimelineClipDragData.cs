using System;
using System.Collections.Generic;
using FramePFX.Editor.Timeline.Controls;
using FramePFX.Editor.Timeline.Layer.Clips;

namespace FramePFX.Editor.Timeline.Utils {
    public class TimelineClipDragData {
        public TimelineControl Timeline { get; }
        public List<ClipDragData> DragClips { get; }
        public bool IsCompleted { get; set; }

        public long Increment;

        public TimelineClipDragData(TimelineControl timeline) {
            this.Timeline = timeline;
            this.DragClips = new List<ClipDragData>();
        }

        public bool IsBeingDragged(TimelineVideoClipControl clip) {
            return clip != null && this.IsBeingDragged(clip.DragData);
        }

        public bool IsBeingDragged(ClipDragData clip) {
            return clip != null && this.DragClips.Contains(clip);
        }

        public void OnBegin(IEnumerable<TimelineVideoClipControl> selectedClips) {
            this.IsCompleted = false;
            foreach (TimelineVideoClipControl clip in selectedClips) {
                this.DragClips.Add(new ClipDragData(clip));
            }

            this.DragClips.ForEach(x => x.OnBegin());
        }

        public void OnMouseMove(long offset) {
            this.ValidateFinalizationState();
            this.Increment++;
            this.DragClips.ForEach(x => x.OnMouseMove(offset));
        }

        public void OnCompleted() {
            this.ValidateFinalizationState();
            this.DragClips.ForEach(x => x.OnCompleted());
            this.IsCompleted = true;
        }

        public void OnCancel() {
            this.ValidateFinalizationState();
            this.DragClips.ForEach(x => x.OnCancel());
            this.IsCompleted = true;
        }

        public void OnEnterCopyMove() {
            this.ValidateFinalizationState();
            this.DragClips.ForEach(x => x.CreateCopiedClip());
        }

        public void OnEnterMoveMode() {
            this.ValidateFinalizationState();
            this.DragClips.ForEach(x => x.DestroyCopiedClip());
        }

        private void ValidateFinalizationState() {
            if (this.IsCompleted) {
                throw new InvalidOperationException("This move data has already been finalized");
            }
        }
    }
}