using FramePFX.Editor.ResourceManaging;

namespace FramePFX.Editor.Timelines.ResourceHelpers
{
    /// <summary>
    /// An interface for a clip that has a single resource associated with it
    /// </summary>
    public interface IResourceClip<T> : IBaseResourceClip where T : ResourceItem
    {
        /// <summary>
        /// Gets the resource helper for this clip, which manages a resource state
        /// </summary>
        new ResourceHelper<T> ResourceHelper { get; }
    }
}