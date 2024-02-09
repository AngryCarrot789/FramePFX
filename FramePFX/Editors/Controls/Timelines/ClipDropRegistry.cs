using System;
using System.Threading.Tasks;
using FramePFX.Editors.EffectSource;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Timelines {
    public static class ClipDropRegistry {
        public static DragDropRegistry<Clip> DropRegistry { get; }

        static ClipDropRegistry() {
            DropRegistry = new DragDropRegistry<Clip>();
            DropRegistry.Register<Clip, EffectProviderEntry>((clip, x, dt, ctx) => {
                return clip.IsEffectTypeAccepted(x.EffectType) ? EnumDropType.Copy : EnumDropType.None;
            }, (clip, x, dt, ctx) => {
                BaseEffect effect;
                try {
                    effect = x.CreateEffect();
                }
                catch (Exception e) {
                    IoC.MessageService.ShowMessage("Error", "Failed to create effect from the dropped effect", e.GetToString());
                    return Task.CompletedTask;
                }

                if (!effect.IsObjectValidForOwner(clip)) {
                    IoC.MessageService.ShowMessage("Error", "This effect is not allowed to be placed in this clip");
                    return Task.CompletedTask;
                }

                clip.AddEffect(effect);
                clip.Timeline?.InvalidateRender();
                return Task.CompletedTask;
            });

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