using FramePFX.Editor.Timelines.AudioClips;

namespace FramePFX.Editor.ViewModels.Timelines.AudioClips
{
    public class SinewaveClipViewModel : AudioClipViewModel
    {
        public new SinewaveClip Model => (SinewaveClip) ((ClipViewModel) this).Model;

        public SinewaveClipViewModel(SinewaveClip model) : base(model)
        {
        }
    }
}