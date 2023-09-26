using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class AdjustmentVideoClipViewModel : VideoClipViewModel {
        public new AdjustmentVideoClip Model => (AdjustmentVideoClip) base.Model;

        public AdjustmentVideoClipViewModel(AdjustmentVideoClip model) : base(model) {
        }
    }
}