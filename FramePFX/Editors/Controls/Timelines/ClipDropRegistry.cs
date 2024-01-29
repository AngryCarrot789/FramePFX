using System.Threading.Tasks;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity;

namespace FramePFX.Editors.Controls.Timelines {
    public static class ClipDropRegistry {
        public static DragDropRegistry<Clip> DropRegistry { get; }

        static ClipDropRegistry() {
            DropRegistry = new DragDropRegistry<Clip>();
            DropRegistry.Register<VideoClipShape, ResourceColour>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) => {
                clip.ColourKey.SetTargetResourceId(h.UniqueId);
                return Task.CompletedTask;
            });

            DropRegistry.Register<ImageVideoClip, ResourceImage>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) => {
                clip.ResourceImageKey.SetTargetResourceId(h.UniqueId);
                return Task.CompletedTask;
            });
        }
    }
}