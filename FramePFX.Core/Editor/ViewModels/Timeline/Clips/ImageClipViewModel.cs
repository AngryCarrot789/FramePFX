using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class ImageClipViewModel : VideoClipViewModel {
        public new ImageClipModel Model => (ImageClipModel) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageClipModel model) : base(model) {

        }

        public override bool CanDropResource(ResourceItem resource) {
            return resource is ResourceImage;
        }

        public override async Task OnDropResource(ResourceItem resource) {
            ResourceImage image = (ResourceImage) resource;
            this.Model.SetTargetResourceId(image.UniqueId);
            this.Model.InvalidateRender();
        }
    }
}