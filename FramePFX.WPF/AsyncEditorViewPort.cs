using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.Utils;
using SkiaSharp;
using Rect = System.Windows.Rect;

namespace FramePFX.WPF {
    public class AsyncEditorViewPort : SKAsyncViewPort {
        private const double thickness = 2.5d;
        private const double half_thickness = thickness / 2d;
        private readonly Pen OutlinePen = new Pen(Brushes.Orange, 2.5f);

        public static readonly DependencyProperty TimelineProperty =
            DependencyProperty.Register(
                "Timeline",
                typeof(TimelineViewModel),
                typeof(AsyncEditorViewPort),
                new PropertyMetadata(null, (d, e) => ((AsyncEditorViewPort) d).OnTimelineChanged((TimelineViewModel) e.OldValue, (TimelineViewModel) e.NewValue)));

        public TimelineViewModel Timeline {
            get => (TimelineViewModel) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        public AsyncEditorViewPort() {
        }

        private void OnTimelineChanged(TimelineViewModel oldTimeline, TimelineViewModel newTimeline) {
            if (oldTimeline != null)
                oldTimeline.ClipSelectionChanged -= this.OnClipSelectionChanged;
            if (newTimeline != null)
                newTimeline.ClipSelectionChanged += this.OnClipSelectionChanged;
        }

        private void OnClipSelectionChanged(TimelineViewModel timeline) {
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (this.Timeline is TimelineViewModel timeline) {
                foreach (TrackViewModel track in timeline.Tracks) {
                    foreach (ClipViewModel clip in track.GetSelectedClipsAtFrame(timeline.PlayHeadFrame)) {
                        if (!(clip is VideoClipViewModel) || !(((VideoClip) clip.Model).GetSize() is Vector2 frameSize)) {
                            continue;
                        }

                        SKRect rect = ((VideoClip) clip.Model).TransformationMatrix.MapRect(frameSize.ToRectAsSize(0, 0));
                        Point pos = new Point(Math.Floor(rect.Left) - half_thickness, Math.Floor(rect.Top) - half_thickness);
                        Size size = new Size(Math.Ceiling(rect.Width) + thickness, Math.Ceiling(rect.Height) + thickness);
                        dc.DrawRectangle(null, this.OutlinePen, new Rect(pos, size));
                    }
                }
            }
        }
    }
}