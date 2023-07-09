namespace FramePFX.Editor.Timeline.Controls {
    public class VideoClipControl : TimelineClipControl {
        public new VideoTrackControl Track => (VideoTrackControl) base.Track;

        public VideoClipControl() {
        }

        public override string ToString() {
            return $"{nameof(VideoClipControl)}({this.Span})";
        }
    }
}
