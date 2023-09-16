using PFXEditor.Automation;
using PFXEditor.Editor.Timelines.Effects.Video;
using PFXEditor.Editor.ViewModels.Timelines.VideoClips;

namespace PFXEditor.Editor.ViewModels.Timelines.Effects.Video {
    public class VideoEffectViewModel : BaseEffectViewModel {
        public new VideoEffect Model => (VideoEffect) base.Model;

        public new VideoClipViewModel OwnerClip => (VideoClipViewModel) base.OwnerClip;

        public VideoEffectViewModel(VideoEffect model) : base(model) {
        }

        protected void InvalidateRenderForAutomationRefresh(in RefreshAutomationValueEventArgs e) {
            if (!e.IsDuringPlayback && !e.IsPlaybackTick) {
                this.Model.OwnerClip.InvalidateRender(true);
            }
        }
    }
}