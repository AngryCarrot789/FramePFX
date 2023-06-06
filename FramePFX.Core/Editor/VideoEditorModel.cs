using FramePFX.Core.History;

namespace FramePFX.Core.Editor {
    public class VideoEditorModel {
        public volatile bool IsProjectSaving;

        public EditorPlaybackModel Playback { get; }

        public ProjectModel ActiveProject { get; set; }

        public HistoryManager HistoryManager { get; }

        public VideoEditorModel() {
            this.Playback = new EditorPlaybackModel(this);
            this.HistoryManager = new HistoryManager();
        }
    }
}