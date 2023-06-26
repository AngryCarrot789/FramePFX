using FramePFX.Core.Editor.Timeline.AudioClips;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class SinewaveClipViewModel : AudioClipViewModel {
        public new SinewaveClipModel Model => (SinewaveClipModel) ((ClipViewModel) this).Model;

        public SinewaveClipViewModel(SinewaveClipModel model) : base(model) {

        }
    }
}