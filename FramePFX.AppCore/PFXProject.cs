using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.New;
using FramePFX.ResourceManaging;

namespace FramePFX.Editor.Project {
    public class PFXProject {
        /// <summary>
        /// This project's render frame rate
        /// </summary>
        public double FrameRate { get; }

        /// <summary>
        /// This project's render resolution (in the viewport display)
        /// </summary>
        public Resolution RenderResolution { get; set; }

        /// <summary>
        /// The video editor associated with this project
        /// </summary>
        public PFXVideoEditor Editor { get; }

        /// <summary>
        /// This project's resource manager
        /// </summary>
        public ResourceManager Resources { get; }

        /// <summary>
        /// This project's timeline
        /// </summary>
        public PFXTimeline Timeline { get; }

        /// <summary>
        /// Whether this project is currently being rendered (e.g. to a file or video stream)
        /// </summary>
        public bool IsRendering { get; set; }

        public PFXProject(PFXVideoEditor editor) {
            this.Editor = editor;
            this.Resources = new ResourceManager();
            this.Timeline = new PFXTimeline(this);
        }
    }
}