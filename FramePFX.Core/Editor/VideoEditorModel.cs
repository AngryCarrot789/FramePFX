using System;

namespace FramePFX.Core.Editor {
    public class VideoEditorModel {
        public EditorPlaybackModel Playback { get; }

        public ProjectModel CurrentProject { get; set; }

        public volatile bool IsProjectSaving;

        public VideoEditorModel() {
            this.Playback = new EditorPlaybackModel(this);
        }
    }
}