using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class ImageClipViewModel : VideoClipViewModel, IDropClipResource {
        public new ImageClipModel Model => (ImageClipModel) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageClipModel model) : base(model) {

        }

        public override bool CanDropResource(ResourceItemViewModel resource) {
            return resource is ResourceImageViewModel;
        }

        public override async Task OnDropResource(ResourceItemViewModel resource) {
            if (!(resource is ResourceImageViewModel image)) {
                await IoC.MessageDialogs.ShowMessageAsync("Incompatible resource", $"Image clips cannot accept {resource.GetType().Name}");
                return;
            }

            this.Model.SetTargetResourceId(image.UniqueId);
            this.Model.InvalidateRender();
        }
    }
}