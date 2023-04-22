using FramePFX.Editor.Project;

namespace FramePFX.Editor {
    public class PFXVideoEditor {
        public PFXProject ActiveProject { get; set; }

        public PFXPlayback Playback { get; }

        public PFXVideoEditor() {
            this.Playback = new PFXPlayback(this);
        }
    }
}