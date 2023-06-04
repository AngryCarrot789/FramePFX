using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class ImageClipViewModel : BaseResourceVideoClipViewModel<ResourceImage, ResourceImageViewModel> {
        public new ImageClipModel Model => (ImageClipModel) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageClipModel model) : base(model) {

        }
    }
}