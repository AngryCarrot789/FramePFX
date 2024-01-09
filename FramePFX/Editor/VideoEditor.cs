namespace FramePFX.Editor {
    public class VideoEditor {
        public volatile bool IsProjectSaving;
        public volatile bool IsProjectChanging;

        public bool CanRender => !this.IsProjectSaving && !this.IsProjectChanging;

        /// <summary>
        /// Gets the editor playback instance for this video editor
        /// </summary>
        public EditorPlayback Playback { get; }

        /// <summary>
        /// Gets the active project for this editor. Only 1 project can be opened at a time
        /// </summary>
        public Project ActiveProject { get; private set; }

        public VideoEditor() {
            this.Playback = new EditorPlayback(this);
        }

        static VideoEditor() {
        }

        public void SetProject(Project project) {
            this.ActiveProject = project;
        }
    }
}