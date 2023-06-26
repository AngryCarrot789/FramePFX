using FramePFX.Core.Editor.Timeline.AudioClips;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class AudioClipViewModel : ClipViewModel {
        public new AudioClipModel Model => (AudioClipModel) base.Model;

        public AudioClipViewModel(AudioClipModel model) : base(model) {

        }
    }
}