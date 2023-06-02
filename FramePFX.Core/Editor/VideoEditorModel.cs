using System;

namespace FramePFX.Core.Editor {
    public class VideoEditorModel {
        public EditorPlaybackModel Playback { get; }

        public ProjectModel CurrentProject { get; set; }

        public VideoEditorModel() {
            this.Playback = new EditorPlaybackModel(this);
        }
    }
}