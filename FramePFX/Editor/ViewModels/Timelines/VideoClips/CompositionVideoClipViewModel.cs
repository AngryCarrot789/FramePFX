using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class CompositionVideoClipViewModel : VideoClipViewModel {
        public new CompositionVideoClip Model => (CompositionVideoClip) ((ClipViewModel) this).Model;

        public CompositionVideoClipViewModel(CompositionVideoClip model) : base(model) {
        }
    }
}