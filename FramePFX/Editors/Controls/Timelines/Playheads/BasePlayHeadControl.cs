using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Controls.Timelines.Playheads {
    public abstract class BasePlayHeadControl : Control {
        protected static readonly FieldInfo IsDraggingPropertyKeyField = typeof(Thumb).GetField("IsDraggingPropertyKey", BindingFlags.NonPublic | BindingFlags.Static);
        public static readonly DependencyProperty TimelineProperty = DependencyProperty.Register("Timeline", typeof(Timeline), typeof(BasePlayHeadControl), new PropertyMetadata(null, (d, e) => ((BasePlayHeadControl) d).OnTimelineChanged((Timeline) e.OldValue, (Timeline) e.NewValue)));

        public Timeline Timeline {
            get => (Timeline) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        protected BasePlayHeadControl() {
        }

        public abstract long GetFrame(Timeline timeline);

        protected virtual void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            if (oldTimeline != null) {
                oldTimeline.ZoomTimeline -= this.OnTimelineZoomed;
            }

            if (newTimeline != null) {
                newTimeline.ZoomTimeline += this.OnTimelineZoomed;
            }
        }

        private void OnTimelineZoomed(Timeline timeline, double oldzoom, double newzoom, ZoomType zoomtype) {
            this.SetPixelFromFrameAndZoom(this.GetFrame(timeline), newzoom);
        }

        protected void SetPixelFromFrame(long frame) {
            this.SetPixelFromFrameAndZoom(frame, this.Timeline?.Zoom ?? 1.0d);
        }

        protected void SetPixelFromFrameAndZoom(long frame, double zoom) {
            Thickness m = this.Margin;
            this.Margin = new Thickness(frame * zoom, m.Top, m.Right, m.Bottom);
        }
    }
}