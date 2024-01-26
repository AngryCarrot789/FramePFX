using FramePFX.Destroying;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors {
    public class Project : IDestroy {
        /// <summary>
        /// Gets this project's primary timeline. This does not change
        /// </summary>
        public Timeline MainTimeline { get; }

        /// <summary>
        /// Gets this project's resource manager. This does not change
        /// </summary>
        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// Gets a reference to the video editor that this project is currently loaded in
        /// </summary>
        public VideoEditor Editor { get; private set; }

        public ProjectSettings Settings { get; }

        public RenderManager RenderManager { get; }

        public Project() {
            this.Settings = ProjectSettings.Default;
            this.RenderManager = new RenderManager(this);
            this.ResourceManager = new ResourceManager(this);
            this.MainTimeline = new Timeline();
            Timeline.SetMainTimelineProjectReference(this.MainTimeline, this);
        }

        /// <summary>
        /// Destroys all of this project's resources, timeline, tracks, clips, etc., allowing for it to be safely garbage collected.
        /// This is called when closing a project, or loading a new project (old project destroyed, new one is loaded)
        /// </summary>
        public void Destroy() {
            this.MainTimeline.Destroy();
        }

        internal static void OnOpened(VideoEditor editor, Project project) {
            project.Editor = editor;
        }

        internal static void OnClosed(VideoEditor editor, Project project) {
            project.Editor = null;
        }
    }
}