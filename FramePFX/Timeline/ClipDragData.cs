using System;

namespace FramePFX.Timeline {
    public class ClipDragData {
        public TimelineControl Timeline { get; }
        public TimelineClipControl Clip { get; }
        public TimelineClipControl CopiedClip { get; set; }

        public bool HasCopy { get; set; }

        public long OriginalFrameBegin { get; set; }
        public long TargetFrameBegin { get; set; }
        public bool IsFinished { get; set; }

        private long buffer;

        public ClipDragData(TimelineClipControl clip) {
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

        private long incremented;

        public void OnMouseMove(long offset) {
            // this.ValidateFinalizationState();
            // this.incremented++;
            // if (this.buffer < 0) {
            //     this.buffer += offset;
            //     if (this.buffer >= 0) {
            //         offset = this.buffer;
            //         this.buffer = 0;
            //     }
            //     else {
            //         this.Clip.Content = $"{this.buffer} | {this.TargetFrameBegin} ({this.incremented})";
            //         return;
            //     }
            // }
            // long newFrameBegin = this.TargetFrameBegin + offset;
            // if (newFrameBegin < 0) {
            //     this.buffer += newFrameBegin;
            // }
            // else {
            //     this.TargetFrameBegin = newFrameBegin;
            //     this.Clip.IsMovingControl = true;
            //     this.Clip.FrameBegin = Math.Max(newFrameBegin, 0);
            //     this.Clip.IsMovingControl = false;
            // }
            // this.Clip.Content = $"{this.buffer} | {newFrameBegin} ({this.incremented})";

            this.ValidateFinalizationState();
            this.incremented++;
            long newFrameBegin = this.TargetFrameBegin + offset;
            this.TargetFrameBegin = newFrameBegin;
            if (newFrameBegin < 0) {
                newFrameBegin = 0;
            }

            this.Clip.IsMovingControl = true;
            this.Clip.FrameBegin = newFrameBegin;
            this.Clip.IsMovingControl = false;
            // this.Clip.Content = $"{this.buffer} | {newFrameBegin} ({this.incremented})";
            this.Clip.Content = $"{this.Clip.Span}";
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
                this.CopiedClip.TimelineLayer.RemoveClip(this.CopiedClip);
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