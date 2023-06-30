using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timelines.VideoClips;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips {
    public class ImageClipViewModel : VideoClipViewModel {
        public new ImageClip Model => (ImageClip) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageClip model) : base(model) {

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