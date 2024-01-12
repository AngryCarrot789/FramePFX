using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Interactivity;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class ImageVideoClipViewModel : VideoClipViewModel {
        public new ImageVideoClip Model => (ImageVideoClip) ((ClipViewModel) this).Model;

        public ImageVideoClipViewModel(ImageVideoClip model) : base(model) {
        }

        static ImageVideoClipViewModel() {
            DropRegistry.Register<ImageVideoClipViewModel, ResourceImageViewModel>((clip, h, dt, ctx) => {
                return EnumDropType.Link;
            }, (clip, h, dt, c) => {
                IResourcePathKey<ResourceImage> key = clip.Model.ResourceImageKey;
                if (key.ActiveLink != null && key.ActiveLink.ResourceId != h.UniqueId) {
                    key.SetTargetResourceId(h.UniqueId);
                    clip.OnInvalidateRender();
                }

                return Task.CompletedTask;
            });
        }
    }
}