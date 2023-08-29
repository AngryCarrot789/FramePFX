using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.Clips {
    public class AVMediaClipViewModel : VideoClipViewModel, IAcceptResourceDrop {
        public new AVMediaVideoClip Model => (AVMediaVideoClip) ((ClipViewModel) this).Model;

        public AVMediaClipViewModel(AVMediaVideoClip model) : base(model) {
        }

        public override bool CanDropResource(BaseResourceObjectViewModel resource) {
            return resource is ResourceAVMediaViewModel;
        }

        public override async Task OnDropResource(BaseResourceObjectViewModel resource) {
            this.Model.SetTargetResourceId(((ResourceAVMediaViewModel) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}