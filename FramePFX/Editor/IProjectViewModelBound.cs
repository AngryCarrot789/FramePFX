using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor {
    /// <summary>
    /// An object that has a project associated with it
    /// </summary>
    public interface IProjectViewModelBound {
        /// <summary>
        /// Gets the project associated with this project
        /// </summary>
        ProjectViewModel Project { get; }
    }
}