using System;
using System.Windows;
using System.Windows.Input;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Controls.Timelines.Playheads {
    public class StopHeadControl : BasePlayHeadControl {
        private const int StateNone = 0;
        private const int StateInit = 1;
        private const int StateActive = 2;
        private const double MinDragInitPx = 5d;

        private Point clickPoint;
        private int dragState;

        public StopHeadControl() {
            this.Focusable = true;
        }

        static StopHeadControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StopHeadControl), new FrameworkPropertyMetadata(typeof(StopHeadControl)));
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            e.Handled = true;
            this.Focus();
            this.clickPoint = e.GetPosition(this);
            this.SetDragState(StateInit);
            if (!this.IsMouseCaptured) {
                this.CaptureMouse();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            base.OnMouseUp(e);
            e.Handled = true;
            this.SetDragState(StateNone);
            this.ReleaseMouseCapture();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            Point mpos = e.GetPosition(this);
            if (e.LeftButton != MouseButtonState.Pressed) {
                this.SetDragState(StateNone);
                this.ReleaseMouseCapture();
                return;
            }

            if (!(this.Timeline is Timeline timeline)) {
                return;
            }

            if (this.dragState == StateInit) {
                if (Math.Abs(mpos.X - this.clickPoint.X) < MinDragInitPx) {
                    return;
                }

                this.SetDragState(StateActive);
            }

            if (this.dragState == StateNone) {
                return;
            }

            Vector mdif = mpos - this.clickPoint;
            long oldFrame = this.Timeline.StopHeadPosition;
            if (Math.Abs(mdif.X) >= 1.0d) {
                long offset = (long) Math.Round(mdif.X / timeline.Zoom);
                if (offset != 0) {
                    // If begin is 2 and offset is -5, this sets offset to -2
                    // and since newBegin = begin+offset (2 + -2)
                    // this ensures begin never drops below 0
                    if ((oldFrame + offset) < 0) {
                        offset = -oldFrame;
                    }

                    if (offset != 0) {
                        timeline.StopHeadPosition = Math.Min(oldFrame + offset, timeline.MaxDuration - 1);
                    }
                }
            }
        }

        private void SetDragState(int state) {
            this.dragState = state;
        }

        public override long GetFrame(Timeline timeline) {
            return timeline.StopHeadPosition;
        }

        protected override void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            base.OnTimelineChanged(oldTimeline, newTimeline);
            if (oldTimeline != null) {
                oldTimeline.StopHeadChanged -= this.OnTimelineStopHeadChanged;
            }

            if (newTimeline != null) {
                newTimeline.StopHeadChanged += this.OnTimelineStopHeadChanged;
            }
        }

        private void OnTimelineStopHeadChanged(Timeline timeline, long oldvalue, long newvalue) {
            this.SetPixelFromFrameAndZoom(newvalue, timeline.Zoom);
        }
    }
}