using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class ImageClipViewModel : VideoClipViewModel {
        public new ImageClipModel Model => (ImageClipModel) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageClipModel model) : base(model) {

        }
    }
}