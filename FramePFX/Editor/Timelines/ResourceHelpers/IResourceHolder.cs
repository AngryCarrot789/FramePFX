namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// An interface for a project-bound object that can have resources associated with it
    /// </summary>
    public interface IResourceHolder : IProjectBound {
        /// <summary>
        /// Gets the resource helper for this clip, which manages the resource states
        /// </summary>
        ResourceHelper ResourceHelper { get; }
    }
}