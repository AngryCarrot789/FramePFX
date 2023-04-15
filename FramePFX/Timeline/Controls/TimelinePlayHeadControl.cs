using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core.Utils;
using Keyboard = System.Windows.Input.Keyboard;

namespace FramePFX.Timeline.Controls {
    public class TimelinePlayHeadControl : Control, IPlayHeadHandle {
        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(long),
                typeof(TimelinePlayHeadControl),
                new FrameworkPropertyMetadata(
                    0L,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelinePlayHeadControl) d).OnFrameBeginChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? 0 : v));

        public static readonly DependencyProperty TimelineProperty =
            DependencyProperty.Register(
                "Timeline",
                typeof(TimelineControl),
                typeof(TimelinePlayHeadControl),
                new PropertyMetadata(null));

        /// <summary>
        /// The zero-based frame index where this play head begins
        /// </summary>
        public long FrameBegin {
            get => (long) this.GetValue(FrameBeginProperty);
            set => this.SetValue(FrameBeginProperty, value);
        }

        public long PlayHeadFrame {
            get => this.FrameBegin;
            set => this.FrameBegin = Maths.Clamp(value, 0, (this.Timeline?.MaxDuration ?? 1) - 1);
        }

        public TimelineControl Timeline {
            get => (TimelineControl) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        /// <summary>
        /// The rendered X position of this element
        /// </summary>
        public double RealPixelX {
            get => this.Margin.Left;
            set {
                Thickness margin = this.Margin;
                margin.Left = value;
                this.Margin = margin;
            }
        }

        private bool isUpdatingFrameBegin;

        private Thumb PART_ThumbHead;
        private Thumb PART_ThumbBody;
        private bool isDraggingThumb;

        public TimelinePlayHeadControl() {

        }

        private T GetTemplateElement<T>(string name) where T : DependencyObject {
            return this.GetTemplateChild(name) is T value ? value : throw new Exception($"Missing templated child '{name}' of type {typeof(T).Name} in control '{this.GetType().Name}'");
        }

        public override void OnApplyTemplate() {
            this.PART_ThumbHead = this.GetTemplateElement<Thumb>("PART_ThumbHead");
            this.PART_ThumbBody = this.GetTemplateElement<Thumb>("PART_ThumbBody");
            if (this.PART_ThumbHead != null) {
                this.PART_ThumbHead.DragDelta += this.PART_ThumbOnDragDelta;
            }

            if (this.PART_ThumbBody != null) {
                this.PART_ThumbBody.DragDelta += this.PART_ThumbOnDragDelta;
            }
        }

        public static long GetClosestEdge(FrameSpan span, long frame, out bool isEndIndex) {
            long endIndex = span.EndIndex;
            long beginDifferenceA = Math.Abs(span.Begin - frame);
            long endDifferenceB = Math.Abs(endIndex - frame);
            if (beginDifferenceA <= endDifferenceB) {
                isEndIndex = false;
                return span.Begin;
            }
            else {
                isEndIndex = true;
                return endIndex;
            }
        }

        private void PART_ThumbOnDragDelta(object sender, DragDeltaEventArgs e) {
            TimelineControl timeline;
            if (this.isDraggingThumb || (timeline = this.Timeline) == null) {
                return;
            }

            long change = TimelineUtils.PixelToFrame(e.HorizontalChange, timeline.UnitZoom);
            if (change == 0) {
                return;
            }

            long frame = this.FrameBegin;
            long begin = Maths.Clamp(frame + change, 0, timeline.MaxDuration - 1);
            if (begin == frame) {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) {
                TimelineClipContainerControl closestClip = null;
                long closestFrame = begin;
                List<TimelineClipContainerControl> clips = timeline.GetClipsInArea(new FrameSpan(begin - 10, 20)).ToList();
                foreach (TimelineClipContainerControl clip in clips) {
                    // this code is still broken and doesn't latch to the nearest when the clips list is 
                    FrameSpan span = clip.Span;
                    long a = Math.Abs(begin - span.Begin);
                    long b = Math.Abs(begin - span.EndIndex);

                    if (a <= closestFrame || b < closestFrame) {
                        if (a <= b) {
                            closestClip = clip;
                            closestFrame = span.Begin;
                        }
                        else {
                            closestClip = clip;
                            closestFrame = span.EndIndex;
                        }
                    }

                    // if (a < closestFrame) {
                    //     closestClip = clip;
                    //     closestFrame = span.Begin;
                    // }
                    // if (b < closestFrame) {
                    //     closestClip = clip;
                    //     closestFrame = span.EndIndex;
                    // }
                }

                if (closestClip != null) {
                    if (closestFrame == begin) {
                        e.Handled = true;
                    }
                    else {
                        this.SetFrameBegin(closestFrame);
                    }
                }
                else {
                    this.SetFrameBegin(begin);
                }
            }
            else {
                this.SetFrameBegin(begin);
            }
        }

        private void SetFrameBegin(long frame) {
            try {
                this.isDraggingThumb = true;
                this.FrameBegin = frame;
            }
            finally {
                this.isDraggingThumb = false;
            }
        }

        private void OnFrameBeginChanged(long oldStart, long newStart) {
            if (this.isUpdatingFrameBegin)
                return;
            this.isUpdatingFrameBegin = true;
            if (oldStart != newStart)
                this.UpdatePosition();
            this.isUpdatingFrameBegin = false;
        }

        public void UpdatePosition() {
            this.RealPixelX = this.FrameBegin * (this.Timeline?.UnitZoom ?? 1d);
        }

        private static readonly FieldInfo IsDraggingPropertyKeyField = typeof(Thumb).GetField("IsDraggingPropertyKey", BindingFlags.NonPublic | BindingFlags.Static);

        public void EnableDragging(Point point) {
            this.PART_ThumbBody.Focus();
            this.PART_ThumbBody.CaptureMouse();
            // lazy... could create custom control extending Thumb to modify this but this works so :D
            this.PART_ThumbBody.SetValue((DependencyPropertyKey) IsDraggingPropertyKeyField.GetValue(null), true);
            bool flag = true;
            try {
                this.PART_ThumbBody.RaiseEvent(new DragStartedEventArgs(point.X, point.Y));
                flag = false;
            }
            finally {
                if (flag) {
                    this.PART_ThumbBody.CancelDrag();
                }
            }
        }
    }
}
