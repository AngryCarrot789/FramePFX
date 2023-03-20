using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FramePFX.Timeline {
    public class TimelinePlayheadControl : Control, INativePlayHead {
        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(long),
                typeof(TimelinePlayheadControl),
                new FrameworkPropertyMetadata(
                    0L,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelinePlayheadControl) d).OnFrameBeginChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? 0 : v));

        public static readonly DependencyProperty TimelineProperty =
            DependencyProperty.Register(
                "Timeline",
                typeof(TimelineControl),
                typeof(TimelinePlayheadControl),
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
            set => this.FrameBegin = value;
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

        public TimelinePlayheadControl() {

        }

        private T GetTemplateElement<T>(string name) where T : DependencyObject {
            return this.GetTemplateChild(name) is T value ? value : throw new Exception($"Missing templated child '{name}' of type {typeof(T).Name} in control '{this.GetType().Name}'");
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ThumbHead = GetTemplateElement<Thumb>("PART_ThumbHead");
            this.PART_ThumbBody = GetTemplateElement<Thumb>("PART_ThumbBody");
            if (this.PART_ThumbHead != null) {
                this.PART_ThumbHead.DragDelta += PART_ThumbOnDragDelta;
            }
            if (this.PART_ThumbBody != null) {
                this.PART_ThumbBody.DragDelta += PART_ThumbOnDragDelta;
            }
        }

        private void PART_ThumbOnDragDelta(object sender, DragDeltaEventArgs e) {
            if (this.isDraggingThumb) {
                return;
            }

            long change = TimelineUtils.PixelToFrame(e.HorizontalChange, this.Timeline.UnitZoom);
            if (change == 0) {
                return;
            }

            long duration = this.FrameBegin + change;
            if (duration < 0) {
                return;
            }

            try {
                this.isDraggingThumb = true;
                this.FrameBegin = duration;
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
    }
}
