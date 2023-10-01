using FramePFX.Editor.Timelines.AudioClips;

namespace FramePFX.Editor.ViewModels.Timelines.AudioClips
{
    public class AudioClipViewModel : ClipViewModel
    {
        public new AudioClip Model => (AudioClip) base.Model;

        public AudioClipViewModel(AudioClip model) : base(model)
        {
        }
    }
}