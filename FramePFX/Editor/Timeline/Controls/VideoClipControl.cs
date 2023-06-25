using FramePFX.Editor.Timeline.Track.Clips;

namespace FramePFX.Editor.Timeline.Controls {
    public class VideoClipControl : TimelineClipControl {
        public new VideoTrackControl Track => (VideoTrackControl) base.Track;

        public bool IsMovingControl { get; set; }

        public ClipDragData DragData { get; set; }

        public VideoClipControl() {
        }

        public override string ToString() {
            return $"{nameof(VideoClipControl)}({this.Span})";
        }
    }
}
