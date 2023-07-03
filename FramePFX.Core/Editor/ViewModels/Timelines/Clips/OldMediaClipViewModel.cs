using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timelines.VideoClips;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips {
    public class OldMediaClipViewModel : VideoClipViewModel, IAcceptResourceDrop {
        public new OldMediaVideoClip Model => (OldMediaVideoClip) ((ClipViewModel) this).Model;

        public OldMediaClipViewModel(OldMediaVideoClip model) : base(model) {
            
        }

        public override bool CanDropResource(BaseResourceObjectViewModel resource) {
            return resource is ResourceOldMediaViewModel;
        }

        public override async Task OnDropResource(BaseResourceObjectViewModel resource) {
            this.Model.SetTargetResourceId(((ResourceOldMediaViewModel) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}