using FramePFX.Automation;
using FramePFX.Editor.Timelines.Effects.Video;

namespace FramePFX.Editor.Timelines.Effects.ViewModels.Video {
    public class VideoEffectViewModel : BaseEffectViewModel {
        public new VideoEffect Model => (VideoEffect) base.Model;

        public VideoEffectViewModel(VideoEffect model) : base(model) {
        }

        protected void InvalidateRenderForAutomationRefresh(in RefreshAutomationValueEventArgs e) {
            if (!e.IsDuringPlayback && !e.IsPlaybackTick) {
                this.Model.OwnerClip.InvalidateRender(true);
            }
        }
    }
}