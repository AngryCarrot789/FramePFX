using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class ImageVideoClipViewModel : VideoClipViewModel {
        public new ImageVideoClip Model => (ImageVideoClip) ((ClipViewModel) this).Model;

        public ImageVideoClipViewModel(ImageVideoClip model) : base(model) {
        }
    }
}