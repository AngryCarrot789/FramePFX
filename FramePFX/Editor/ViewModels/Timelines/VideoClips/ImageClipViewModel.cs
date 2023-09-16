using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class ImageClipViewModel : VideoClipViewModel {
        public new ImageVideoClip Model => (ImageVideoClip) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageVideoClip model) : base(model) {
        }
    }
}