namespace FramePFX.Editor {
    public class VideoEditor {
        public volatile bool IsProjectSaving;

        public EditorPlayback Playback { get; }

        public Project ActiveProject { get; set; }

        public VideoEditor() {
            this.Playback = new EditorPlayback(this);
        }
    }
}