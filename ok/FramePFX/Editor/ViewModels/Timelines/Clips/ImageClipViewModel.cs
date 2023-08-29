using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.Clips {
    public class ImageClipViewModel : VideoClipViewModel {
        public new ImageVideoClip Model => (ImageVideoClip) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageVideoClip model) : base(model) {
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