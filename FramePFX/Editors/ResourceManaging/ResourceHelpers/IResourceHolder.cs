namespace FramePFX.Editors.ResourceManaging.ResourceHelpers {
    /// <summary>
    /// An interface for a project-bound object that can have resources associated with it
    /// </summary>
    public interface IResourceHolder : IHaveProject {
        /// <summary>
        /// Gets the resource helper for this clip, which manages the resource states
        /// </summary>
        ResourceHelper ResourceHelper { get; }
    }
}