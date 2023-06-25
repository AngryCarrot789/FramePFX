namespace FramePFX.Editor.Timeline.Controls {
    public class AudioClipControl : TimelineClipControl {
        public new AudioTrackControl Track => (AudioTrackControl) base.Track;

        public AudioClipControl() {

        }

        public override string ToString() {
            return $"{nameof(AudioClipControl)}({this.Span})";
        }
    }
}