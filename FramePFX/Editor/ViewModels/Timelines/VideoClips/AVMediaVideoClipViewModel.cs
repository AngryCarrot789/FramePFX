using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Interactivity;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class AVMediaVideoClipViewModel : VideoClipViewModel {
        public new AVMediaVideoClip Model => (AVMediaVideoClip) ((ClipViewModel) this).Model;

        public AVMediaVideoClipViewModel(AVMediaVideoClip model) : base(model) {
        }

        static AVMediaVideoClipViewModel() {
            DropRegistry.Register<AVMediaVideoClipViewModel, ResourceAVMediaViewModel>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) => {
                clip.Model.ResourceAVMediaKey.SetTargetResourceId(h.UniqueId);
                return Task.CompletedTask;
            });
        }
    }
}