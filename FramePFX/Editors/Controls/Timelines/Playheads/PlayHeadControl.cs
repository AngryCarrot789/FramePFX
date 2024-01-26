using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Controls.Timelines.Playheads {
    public class PlayHeadControl : BasePlayHeadControl {
        private Thumb PART_ThumbHead;
        private Thumb PART_ThumbBody;

        public PlayHeadControl() {
        }

        static PlayHeadControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (PlayHeadControl), new FrameworkPropertyMetadata(typeof(PlayHeadControl)));
        }

        public override long GetFrame(Timeline timeline) {
            return timeline.PlayHeadPosition;
        }

        protected override void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            base.OnTimelineChanged(oldTimeline, newTimeline);
            if (oldTimeline != null) {
                oldTimeline.PlayHeadChanged -= this.OnTimelinePlayHeadChanged;
            }

            if (newTimeline != null) {
                newTimeline.PlayHeadChanged += this.OnTimelinePlayHeadChanged;
            }
        }

        private void OnTimelinePlayHeadChanged(Timeline timeline, long oldvalue, long newvalue) {
            this.SetPixelFromFrameAndZoom(newvalue, timeline.Zoom);
        }

        public override void OnApplyTemplate() {
            this.PART_ThumbHead = this.GetTemplateChild("PART_ThumbHead") as Thumb;
            this.PART_ThumbBody = this.GetTemplateChild("PART_ThumbBody") as Thumb;
            if (this.PART_ThumbHead != null) {
                this.PART_ThumbHead.DragDelta += this.PART_ThumbOnDragDelta;
            }

            if (this.PART_ThumbBody != null) {
                this.PART_ThumbBody.DragDelta += this.PART_ThumbOnDragDelta;
            }
        }

        private void PART_ThumbOnDragDelta(object sender, DragDeltaEventArgs e) {
            Timeline timeline = this.Timeline;
            if (timeline == null) {
                return;
            }

            long change = (long) (e.HorizontalChange / timeline.Zoom);
            if (change != 0) {
                long oldFrame = timeline.PlayHeadPosition;
                long newFrame = Math.Max(oldFrame + change, 0);
                if (newFrame >= timeline.MaxDuration) {
                    newFrame = timeline.MaxDuration - 1;
                }

                if (newFrame != oldFrame) {
                    timeline.PlayHeadPosition = newFrame;

                    // Don't update stop head when dragging on the ruler
                    // timeline.StopHeadPosition = newFrame;
                }
            }
        }

        public void EnableDragging(Point point) {
            Thumb thumb = this.PART_ThumbBody ?? this.PART_ThumbHead;
            if (thumb == null) {
                return;
            }

            thumb.Focus();
            thumb.CaptureMouse();
            // lazy... could create custom control extending Thumb to modify this but this works so :D
            thumb.SetValue((DependencyPropertyKey) IsDraggingPropertyKeyField.GetValue(null), true);
            bool flag = true;
            try {
                thumb.RaiseEvent(new DragStartedEventArgs(point.X, point.Y));
                flag = false;
            }
            finally {
                if (flag) {
                    thumb.CancelDrag();
                }
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            return base.ArrangeOverride(arrangeBounds);
        }
    }
}