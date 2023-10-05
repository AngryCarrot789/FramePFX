namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// An interface for an object that has a project associated with it
    /// </summary>
    public interface IProjectBound {
        /// <summary>
        /// Gets the project associated with this object
        /// </summary>
        Project Project { get; }
    }
}