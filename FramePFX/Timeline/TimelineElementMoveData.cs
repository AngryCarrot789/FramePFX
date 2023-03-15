using System.Collections.Generic;

namespace FramePFX.Timeline {
    public class TimelineElementMoveData {
        /// <summary>
        /// The drag copied the original clip onto the cursor, unless <see cref="IsCopyDropAndMoveOriginal"/> in
        /// which case the copy was dropped at the cursor and the original is is being dragged
        /// </summary>
        public bool IsCopyDropAndLeaveOriginal { get; set; }

        /// <summary>
        /// The drag copied an element at the cursor location and also moved the
        /// original clip somewhere; <see cref="OriginalFrameBegin"/> is no longer in use
        /// </summary>
        public bool IsCopyDropAndMoveOriginal { get; set; }

        /// <summary>
        /// The frame at which the drag was started
        /// </summary>
        public long OriginalFrameBegin { get; set; }

        /// <summary>
        /// A copy of the original clip that was dragged, which would be located at <see cref="OriginalFrameBegin"/>
        /// </summary>
        public TimelineClipControl CopiedClip { get; set; }

        /// <summary>
        /// The element that started this drag
        /// </summary>
        public TimelineClipControl OriginalClip { get; }

        public List<TimelineElementMoveData> Children { get; }

        public TimelineElementMoveData(TimelineClipControl originalClip) {
            this.OriginalClip = originalClip;
            this.Children = new List<TimelineElementMoveData>();
        }

        public void OnDragComplete() {
            if (this.IsCopyDropAndLeaveOriginal) {
                if (this.IsCopyDropAndMoveOriginal) {
                    // Swap original and copy's positions
                    long copyFrameBegin = this.CopiedClip.FrameBegin;
                    this.CopiedClip.FrameBegin = this.OriginalClip.FrameBegin;
                    this.OriginalClip.FrameBegin = copyFrameBegin;
                }
                else {
                    // Move the copied clip to where the mouse is, and put the
                    // original clip back to where it originally was
                    this.CopiedClip.FrameBegin = this.OriginalClip.FrameBegin;
                    this.OriginalClip.FrameBegin = this.OriginalFrameBegin;
                }

                this.OriginalFrameBegin = this.OriginalFrameBegin;
                // this.OriginalClip.TimelineLayer.OnClipDragged(this.CopiedClip, this);
            }
            // else {
            //     this.OriginalClip.TimelineLayer.OnClipDragged(this.OriginalClip, this);
            // }

            foreach (TimelineElementMoveData data in this.Children) {
                data.OnDragComplete();
            }
        }

        public void OnDragCancelled() {
            this.DestroyCopiedClip(false);
            this.OriginalClip.FrameBegin = this.OriginalFrameBegin;
            foreach (TimelineElementMoveData data in this.Children) {
                data.OnDragCancelled();
            }
        }

        public void DestroyCopiedClip(bool fireChildrenEvents = true) {
            if (this.CopiedClip != null) {
                this.OriginalClip.TimelineLayer.RemoveElement(this.CopiedClip);
                this.CopiedClip = null;
            }

            if (fireChildrenEvents) {
                foreach (TimelineElementMoveData data in this.Children) {
                    data.DestroyCopiedClip();
                }
            }
        }
    }
}
