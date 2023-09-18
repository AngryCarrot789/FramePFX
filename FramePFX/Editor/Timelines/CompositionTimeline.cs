using FramePFX.Editor.ResourceManaging.Resources;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// A timeline that is used for a composition clip
    /// </summary>
    public class CompositionTimeline : Timeline {
        /// <summary>
        /// Gets or sets the resource clip that owns this composition timeline.
        /// This should not be null as it should be set as close after the constructor as possible
        /// </summary>
        public ResourceCompositionSeq Owner { get; set; }

        public CompositionTimeline() {
        }
    }
}