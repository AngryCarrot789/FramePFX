using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Editor;
using FramePFX.Utils;
using FramePFX.WPF.Editor.Timeline.Utils;
using Keyboard = System.Windows.Input.Keyboard;

namespace FramePFX.WPF.Editor.Timeline.Controls {
    public class TimelinePlayHeadControl : Control, IPlayHead {
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
                            return TimelineUtils.ZeroLongBox;
                        }
                        else if (((TimelinePlayHeadControl) d).Timeline is TimelineControl timeline && value >= timeline.MaxDuration) {
                            return timeline.MaxDuration - 1;
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
                List<TimelineClipControl> clips = timeline.GetClipsInSpan(new FrameSpan(newFrame - (range / 2), range)).ToList();
                if (clips.Count > 0) {
                    long closestFrame = long.MaxValue;
                    long targetFrame = newFrame;
                    foreach (TimelineClipControl clip in clips) {
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

                this.isDraggingThumb = false;
            }
        }
    }
}