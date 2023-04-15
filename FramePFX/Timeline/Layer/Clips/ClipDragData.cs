using System;
using FramePFX.Timeline.Controls;

namespace FramePFX.Timeline.Layer.Clips {
    public class ClipDragData {
        public TimelineControl Timeline { get; }
        public TimelineVideoClipControl Clip { get; }
        public TimelineVideoClipControl CopiedClip { get; set; }

        public bool HasCopy { get; set; }

        public long OriginalFrameBegin { get; set; }
        public long TargetFrameBegin { get; set; }
        public bool IsFinished { get; set; }

        public ClipDragData(TimelineVideoClipControl clip) {
            this.Clip = clip;
            this.Clip.DragData = this;
            this.Timeline = clip.Timeline;
        }

        public void OnBegin() {
            this.OriginalFrameBegin = this.Clip.FrameBegin;
            this.TargetFrameBegin = this.Clip.FrameBegin;
            this.IsFinished = false;
            this.CopiedClip = null;
        }

        public void OnMouseMove(long offset) {
            this.ValidateFinalizationState();
            long newFrameBegin = this.TargetFrameBegin + offset;
            this.TargetFrameBegin = newFrameBegin;
            if (newFrameBegin < 0) {
                newFrameBegin = 0;
            }

            this.Clip.IsMovingControl = true;
            this.Clip.FrameBegin = newFrameBegin;
            this.Clip.IsMovingControl = false;
            this.Clip.ToolTip = $"{this.Clip.Span}";
        }

        public void OnCompleted() {
            this.IsFinished = true;
            this.Clip.DragData = null;
            if (this.CopiedClip != null) {
                this.CopiedClip.DragData = null; // should be null anyway
            }
        }

        public void OnCancel() {
            this.ValidateFinalizationState();
            this.DestroyCopiedClip();
            this.Clip.FrameBegin = this.OriginalFrameBegin;
            this.Clip.DragData = null;
            this.IsFinished = true;
        }

        public void CreateCopiedClip() {
            this.ValidateFinalizationState();
            if (this.HasCopy || this.CopiedClip != null) {
                return;
            }

            // TODO: interface with viewmodel to create duplicate clip somehow...
        }

        public void DestroyCopiedClip() {
            this.ValidateFinalizationState();
            if (this.HasCopy && this.CopiedClip != null) {
                this.CopiedClip.Layer.RemoveClip(this.CopiedClip);
                this.CopiedClip.DragData = null; // should be null anyway
                this.CopiedClip = null;
            }
        }

        private void ValidateFinalizationState() {
            if (this.IsFinished) {
                throw new InvalidOperationException("This move data has already been finalized");
            }
        }
    }
}