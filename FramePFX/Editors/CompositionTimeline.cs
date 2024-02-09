using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors {
    public sealed class CompositionTimeline : Timeline {
        /// <summary>
        /// Gets the resource that owns this composition timeline
        /// </summary>
        public ResourceComposition Resource { get; private set; }

        public CompositionTimeline() {
        }

        internal static void InternalConstructCompositionTimeline(ResourceComposition resource) {
            resource.Timeline.Resource = resource;
        }
    }
}