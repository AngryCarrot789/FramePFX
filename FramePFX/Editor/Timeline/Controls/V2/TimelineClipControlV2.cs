using System.Windows;
using System.Windows.Controls;

namespace FramePFX.Editor.Timeline.Controls.V2 {
    public class TimelineClipControlV2 : Control {
        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(long),
                typeof(TimelineClipControlV2),
                new PropertyMetadata(0L));

        public static readonly DependencyProperty FrameDurationProperty =
            DependencyProperty.Register(
                "FrameDuration",
                typeof(long),
                typeof(TimelineClipControlV2),
                new PropertyMetadata(0L));

        public long FrameBegin {
            get => (long) this.GetValue(FrameBeginProperty);
            set => this.SetValue(FrameBeginProperty, value);
        }

        public long FrameDuration {
            get => (long) this.GetValue(FrameDurationProperty);
            set => this.SetValue(FrameDurationProperty, value);
        }

        public TimelineClipControlV2() {

        }

        protected override Size MeasureOverride(Size constraint) {
            Size size = base.MeasureOverride(constraint);
            return new Size(this.FrameDuration, size.Height);
        }
    }
}