using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class AVMediaClipViewModel : VideoClipViewModel {
        public new AVMediaVideoClip Model => (AVMediaVideoClip) ((ClipViewModel) this).Model;

        public AVMediaClipViewModel(AVMediaVideoClip model) : base(model) {
        }
    }
}