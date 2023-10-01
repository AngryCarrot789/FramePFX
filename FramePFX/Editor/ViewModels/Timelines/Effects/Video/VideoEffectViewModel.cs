using FramePFX.Automation.Events;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.Effects.Video
{
    public class VideoEffectViewModel : BaseEffectViewModel
    {
        public new VideoEffect Model => (VideoEffect) base.Model;

        public new VideoClipViewModel OwnerClip => (VideoClipViewModel) base.OwnerClip;

        public VideoEffectViewModel(VideoEffect model) : base(model)
        {
        }

        protected void InvalidateRenderForAutomationRefresh(in RefreshAutomationValueEventArgs e)
        {
            if (!e.IsDuringPlayback && !e.IsPlaybackTick)
            {
                this.Model.OwnerClip.InvalidateRender(true);
            }
        }
    }
}