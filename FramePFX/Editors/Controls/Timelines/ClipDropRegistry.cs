using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Timelines {
    public static class ClipDropRegistry {
        public static DragDropRegistry<Clip> DropRegistry { get; }

        static ClipDropRegistry() {
            DropRegistry = new DragDropRegistry<Clip>();
            DropRegistry.Register<VideoClipShape, ResourceColour>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) => {
                clip.ColourKey.SetTargetResourceId(h.UniqueId);
                return Task.CompletedTask;
            });
        }
    }
}