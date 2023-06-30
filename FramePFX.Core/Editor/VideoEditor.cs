using FramePFX.Core.History;

namespace FramePFX.Core.Editor {
    public class VideoEditor {
        public volatile bool IsProjectSaving;

        public EditorPlaybackModel Playback { get; }

        public Project ActiveProject { get; set; }

        public HistoryManager HistoryManager { get; }

        public VideoEditor() {
            this.Playback = new EditorPlaybackModel(this);
            this.HistoryManager = new HistoryManager();
        }
    }
}