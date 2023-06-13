using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class MediaClipViewModel : VideoClipViewModel, IAcceptResourceDrop {
        public new MediaClipModel Model => (MediaClipModel) ((ClipViewModel) this).Model;

        public MediaClipViewModel(MediaClipModel model) : base(model) {
            
        }

        public override bool CanDropResource(BaseResourceObjectViewModel resource) {
            return resource is ResourceMediaViewModel;
        }

        public override async Task OnDropResource(BaseResourceObjectViewModel resource) {
            this.Model.SetTargetResourceId(((ResourceMediaViewModel) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}