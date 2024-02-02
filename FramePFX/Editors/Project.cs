using System.Threading;
using FramePFX.Destroying;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.RBC;

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

        /// <summary>
        /// Gets or sets if a video is being exported. Used by the view port to optimise the UI for rendering
        /// </summary>
        public bool IsExporting { get; set; }

        public Project() {
            this.Settings = ProjectSettings.CreateDefault(this);
            this.RenderManager = new RenderManager(this);
            this.ResourceManager = new ResourceManager(this);
            this.MainTimeline = new Timeline();
            Timeline.InternalSetMainTimelineProjectReference(this.MainTimeline, this);
        }

        public void WriteToRBE(RBEDictionary data) {
            this.ResourceManager.WriteToRBE(data.CreateDictionary("ResourceManager"));
            this.MainTimeline.WriteToRBE(data.CreateDictionary("Timeline"));
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.ResourceManager.ReadFromRBE(data.GetDictionary("ResourceManager"));
            this.MainTimeline.ReadFromRBE(data.GetDictionary("Timeline"));
        }

        /// <summary>
        /// Destroys all of this project's resources, timeline, tracks, clips, etc., allowing for it to be safely garbage collected.
        /// This is called when closing a project, or loading a new project (old project destroyed, new one is loaded)
        /// </summary>
        public void Destroy() {
            // TODO: this is no good
            while (this.RenderManager.IsRendering)
                Thread.Sleep(1);
            using (this.RenderManager.SuspendRenderInvalidation()) {
                this.MainTimeline.Destroy();
                this.ResourceManager.ClearEntries();
            }

            this.RenderManager.Dispose();
        }

        internal static void OnOpened(VideoEditor editor, Project project) {
            project.Editor = editor;
        }

        internal static void OnClosed(VideoEditor editor, Project project) {
            project.Editor = null;
        }
    }
}