namespace FramePFX.Editors {
    /// <summary>
    /// An interface for an object that exists in a project, somewhere. Examples include timeline, track, clip, effect, resource, etc.
    /// </summary>
    public interface IHaveProject {
        /// <summary>
        /// Gets the project associated with this object. May return null if not associated with
        /// a project yet (e.g. clip not placed in a track)
        /// </summary>
        Project Project { get; }
    }
}