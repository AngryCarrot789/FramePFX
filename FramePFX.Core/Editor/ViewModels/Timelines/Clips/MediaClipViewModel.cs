using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timelines.VideoClips;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips {
    public class MediaClipViewModel : VideoClipViewModel, IAcceptResourceDrop {
        public new MediaClip Model => (MediaClip) ((ClipViewModel) this).Model;

        public MediaClipViewModel(MediaClip model) : base(model) {
            
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