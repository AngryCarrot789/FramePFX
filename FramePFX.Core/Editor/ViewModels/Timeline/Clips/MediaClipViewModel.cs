using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class MediaClipViewModel : VideoClipViewModel, IDropClipResource {
        public new MediaClipModel Model => (MediaClipModel) ((ClipViewModel) this).Model;

        public MediaClipViewModel(MediaClipModel model) : base(model) {
            
        }

        public override bool CanDropResource(ResourceItem resource) {
            return resource is ResourceMedia;
        }

        public override async Task OnDropResource(ResourceItem resource) {
            this.Model.SetTargetResourceId(((ResourceMedia) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}