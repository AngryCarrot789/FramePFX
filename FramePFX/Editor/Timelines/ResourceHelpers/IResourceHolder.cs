namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// An interface for a clip that can have multiple resources associated with it
    /// </summary>
    public interface IResourceHolder : IProjectBound {
        /// <summary>
        /// Gets the resource helper for this clip, which manages the resource states
        /// </summary>
        ResourceHelper ResourceHelper { get; }
    }
}