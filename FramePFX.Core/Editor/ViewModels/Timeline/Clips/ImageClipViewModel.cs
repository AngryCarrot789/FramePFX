using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timeline.VideoClips;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class ImageClipViewModel : VideoClipViewModel {
        public new ImageClipModel Model => (ImageClipModel) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageClipModel model) : base(model) {

        }

        public override bool CanDropResource(BaseResourceObjectViewModel resource) {
            return resource is ResourceImageViewModel;
        }

        public override async Task OnDropResource(BaseResourceObjectViewModel resource) {
            this.Model.SetTargetResourceId(((ResourceImageViewModel) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}