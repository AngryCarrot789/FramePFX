using FramePFX.Editor.Timelines;

namespace FramePFX.Editor.ViewModels.Timelines {
    public class CompositionTimelineViewModel : TimelineViewModel {
        public new CompositionTimeline Model => (CompositionTimeline) base.Model;

        public CompositionTimelineViewModel(CompositionTimeline model) : base(model) {
        }
    }
}