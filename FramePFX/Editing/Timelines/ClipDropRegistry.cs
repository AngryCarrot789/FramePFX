using FramePFX.Editing.EffectSource;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Effects;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.Editing.Timelines;

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

        DropRegistry.Register<AVMediaVideoClip, ResourceAVMedia>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) => {
            if (h.HasReachedResourecLimit()) {
                int count = h.ResourceLinkLimit;
                IoC.MessageService.ShowMessage("Resource Limit", $"This resource cannot be used by more than {count} clip{Lang.S(count)}");
                return Task.CompletedTask;
            }

            clip.ResourceAVMediaKey.SetTargetResourceId(h.UniqueId);
            clip.ResourceAVMediaKey.TryLoadLink();
            return Task.CompletedTask;
        });
    }
}