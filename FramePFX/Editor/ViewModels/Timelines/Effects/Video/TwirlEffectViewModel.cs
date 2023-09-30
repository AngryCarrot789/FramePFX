using FramePFX.Editor.Timelines.Effects.Video;

namespace FramePFX.Editor.ViewModels.Timelines.Effects.Video {
    public class TwirlEffectViewModel : VideoEffectViewModel {
        public new TwirlEffect Model => (TwirlEffect) base.Model;

        public TwirlEffectViewModel(TwirlEffect model) : base(model) {
        }
    }
}