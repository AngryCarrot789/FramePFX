using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips
{
    public class AVMediaVideoClipViewModel : VideoClipViewModel
    {
        public new AVMediaVideoClip Model => (AVMediaVideoClip) ((ClipViewModel) this).Model;

        public AVMediaVideoClipViewModel(AVMediaVideoClip model) : base(model)
        {
        }
    }
}