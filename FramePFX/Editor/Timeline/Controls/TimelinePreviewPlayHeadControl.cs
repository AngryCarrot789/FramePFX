using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Editor;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public class TimelinePreviewPlayHeadControl : Control, IPlayHead {
        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register(
                "FrameIndex",
                typeof(long),
                typeof(TimelinePreviewPlayHeadControl),
                new FrameworkPropertyMetadata(
                    0L,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelinePreviewPlayHeadControl) d).OnFrameBeginChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => {
                        long value = (long) v;
                        if (value < 0) {
                            return TimelineUtils.ZeroLongBox;
                        }
                        else if (((TimelinePreviewPlayHeadControl) d).Timeline is TimelineControl timeline && value >= timeline.MaxDuration) {
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
                typeof(TimelinePreviewPlayHeadControl),
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

        public TimelinePreviewPlayHeadControl() {
        }

        private void OnFrameBeginChanged(long oldStart, long newStart) {
            if (this.isUpdatingFrameBegin || oldStart == newStart)
                return;
            this.isUpdatingFrameBegin = true;
            this.RealPixelX = this.FrameIndex * (this.Timeline?.UnitZoom ?? 1d);
            this.isUpdatingFrameBegin = false;
        }
    }
}