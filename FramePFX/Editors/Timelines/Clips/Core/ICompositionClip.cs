using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.Timelines.Clips.Core {
    /// <summary>
    /// An interface for a clip that is can be linked to a composition timeline
    /// </summary>
    public interface ICompositionClip {
        /// <summary>
        /// Gets the resource path key for the composition resource
        /// </summary>
        IResourcePathKey<ResourceComposition> ResourceCompositionKey { get; }
    }
}