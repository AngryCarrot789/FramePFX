using System;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Controls;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Track.Clips {

    /*
     * TODO: reimplement this using a more 'Core' implementation (that doesn't reference UI objects). The current way is very
     * bug prone, as it's all handled in the clip's drag methods
     */
    
    public class ClipDragData {
        public TimelineControl Timeline { get; }
        public VideoClipControl Clip { get; }
        public VideoClipControl CopiedClip { get; set; }

        public bool HasCopy { get; set; }
        public long OriginalFrameBegin { get; set; }
        public long TargetFrameBegin { get; set; }
        public bool IsFinished { get; set; }

        public ClipDragData(VideoClipControl clip) {
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
            if (this.HasCopy) {
                return;
            }

            // TODO: maybe a better way of creating clips?
            if (this.Clip.DataContext is ClipViewModel clip) {
                Clip clone = clip.Model.Clone();
                clone.FrameSpan = clone.FrameSpan.WithBegin(this.OriginalFrameBegin);
                clone.DisplayName = TextIncrement.GetNextText(clone.DisplayName);
                clip.Track.CreateClip(clone);
                this.HasCopy = true;
                if (ICGenUtils.GetContainerForItem2<ClipViewModel, VideoClipControl>(x => x.Model == clone, this.Clip.Track.ItemContainerGenerator, out VideoClipControl control)) {
                    this.CopiedClip = control;
                }

                clip.Track.MakeTopMost(clip);
            }
        }

        public void DestroyCopiedClip() {
            this.ValidateFinalizationState();
            if (this.HasCopy && this.CopiedClip != null) {
                ClipViewModel clip = (ClipViewModel) this.CopiedClip.DataContext;
                clip.Track.RemoveClipFromTrack(clip);
                this.CopiedClip.DragData = null; // should be null anyway
                this.CopiedClip = null;
                this.HasCopy = false;
            }
        }

        private void ValidateFinalizationState() {
            if (this.IsFinished) {
                throw new InvalidOperationException("This move data has already been finalized");
            }
        }
    }
}