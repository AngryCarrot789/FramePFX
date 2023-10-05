using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor {
    /// <summary>
    /// An object that has a timeline associated with it
    /// </summary>
    public interface ITimelineViewModelBound : IProjectViewModelBound {
        /// <summary>
        /// Gets the timeline associated with this object
        /// </summary>
        TimelineViewModel Timeline { get; }
    }
}