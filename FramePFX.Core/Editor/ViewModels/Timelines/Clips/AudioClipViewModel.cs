using FramePFX.Core.Editor.Timelines.AudioClips;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips {
    public class AudioClipViewModel : ClipViewModel {
        public new AudioClip Model => (AudioClip) base.Model;

        public AudioClipViewModel(AudioClip model) : base(model) {
        }
    }
}