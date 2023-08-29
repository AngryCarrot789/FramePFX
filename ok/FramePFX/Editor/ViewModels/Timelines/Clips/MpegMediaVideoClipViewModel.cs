using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.Clips {
    public class MpegMediaVideoClipViewModel : VideoClipViewModel, IAcceptResourceDrop {
        public new MpegMediaVideoClip Model => (MpegMediaVideoClip) ((ClipViewModel) this).Model;

        public MpegMediaVideoClipViewModel(MpegMediaVideoClip model) : base(model) {
        }

        public override bool CanDropResource(BaseResourceObjectViewModel resource) {
            return resource is ResourceMpegMediaViewModel;
        }

        public override async Task OnDropResource(BaseResourceObjectViewModel resource) {
            this.Model.SetTargetResourceId(((ResourceMpegMediaViewModel) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}