using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core.Editor;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Utils;
using FramePFX.Utils;
using Keyboard = System.Windows.Input.Keyboard;

namespace FramePFX.Editor.Timeline.Controls {
    public class TimelinePlayHeadControl : Control, IPlayHeadHandle {
        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register(
                "FrameIndex",
                typeof(long),
                typeof(TimelinePlayHeadControl),
                new FrameworkPropertyMetadata(
                    0L,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelinePlayHeadControl) d).OnFrameBeginChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => {
                        long value = (long) v;
                        if (value < 0) {
                            return 0;
                        }
                        else if (((TimelinePlayHeadControl) d).Timeline is TimelineControl timeline && value > timeline.MaxDuration) {
                            return timeline.MaxDuration;
                        }
                        else {
                            return v;
                        }
                    }));

        public static readonly DependencyProperty TimelineProperty =
            DependencyProperty.Register(
                "Timeline",
                typeof(TimelineControl),
                typeof(TimelinePlayHeadControl),
                new PropertyMetadata(null));

        /// <summary>
        /// The zero-based frame index where this play head begins
        /// </summary>
        public long FrameIndex {
            get => (long) this.GetValue(FrameIndexProperty);
            set => this.SetValue(FrameIndexProperty, value);
        }

        public long PlayHeadFrame {
            get => this.FrameIndex;
            set => this.FrameIndex = value;
        }

        public TimelineControl Timeline {
            get => (TimelineControl) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        /// <summary>
        /// The rendered X position of this element
        /// </summary>
        public double RealPixelX {
            get => this.Margin.Left; // Canvas.GetLeft(this)
            set {
                // Canvas.SetLeft(this, value);
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

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            return base.ArrangeOverride(arrangeBounds);
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

            long oldFrame = this.FrameIndex;
            long newFrame = Maths.Clamp(oldFrame + change, 0, timeline.MaxDuration - 1);
            if (newFrame == oldFrame) {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) {
                const long range = 80;
                List<VideoClipControl> clips = timeline.GetClipsInSpan<VideoClipControl>(new FrameSpan(newFrame - (range / 2), range)).ToList();
                if (clips.Count >= 1) {
                    long closestFrame = long.MaxValue;
                    long targetFrame = newFrame;
                    foreach (VideoClipControl clip in clips) {
                        FrameSpan span = clip.Span;
                        long distBegin = Math.Abs(span.Begin - newFrame);
                        long distEnd = Math.Abs(span.EndIndex - newFrame);
                        if (distBegin <= range && distBegin < closestFrame) {
                            closestFrame = distBegin;
                            targetFrame = span.Begin;
                        }

                        if (distEnd <= range && distEnd < closestFrame) {
                            closestFrame = distEnd;
                            targetFrame = span.EndIndex;
                        }
                    }

                    this.SetFrameIndex(targetFrame);
                }
                else {
                    this.SetFrameIndex(newFrame);
                }
            }
            else {
                this.SetFrameIndex(newFrame);
            }
        }

        private void SetFrameIndex(long frame) {
            try {
                this.isDraggingThumb = true;
                this.FrameIndex = frame;
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
            this.RealPixelX = this.FrameIndex * (this.Timeline?.UnitZoom ?? 1d);
        }

        private static readonly FieldInfo IsDraggingPropertyKeyField = typeof(Thumb).GetField("IsDraggingPropertyKey", BindingFlags.NonPublic | BindingFlags.Static);

        public void EnableDragging(Point point) {
            this.isDraggingThumb = true;
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

                this.isDraggingThumb = false;
            }
        }
    }
}
