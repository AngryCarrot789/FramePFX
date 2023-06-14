namespace FramePFX.Editor.Timeline.Controls {
    public class AudioClipControl : TimelineClipControl {
        public new AudioLayerControl Layer => (AudioLayerControl) base.Layer;

        public AudioClipControl() {

        }

        public override string ToString() {
            return $"{nameof(AudioClipControl)}({this.Span})";
        }
    }
}