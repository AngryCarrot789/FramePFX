using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor {
    public interface ITimelineViewModelBound : IProjectViewModelBound {
        TimelineViewModel Timeline { get; }
    }
}