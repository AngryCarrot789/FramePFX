// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using FramePFX.Editing.EffectSource;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Effects;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.Editing.Timelines;

public static class ClipDropRegistry
{
    public static DragDropRegistry<Clip> DropRegistry { get; }

    static ClipDropRegistry()
    {
        DropRegistry = new DragDropRegistry<Clip>();
        DropRegistry.Register<Clip, EffectProviderEntry>((clip, x, dt, ctx) =>
        {
            return clip.IsEffectTypeAccepted(x.EffectType) ? EnumDropType.Copy : EnumDropType.None;
        }, async (clip, x, dt, ctx) =>
        {
            BaseEffect effect;
            try
            {
                effect = x.CreateEffect();
            }
            catch (Exception e)
            {
                await IoC.MessageService.ShowMessage("Error", "Failed to create effect from the dropped effect", e.GetToString());
                return;
            }

            if (!effect.IsObjectValidForOwner(clip))
            {
                await IoC.MessageService.ShowMessage("Error", "This effect is not allowed to be placed in this clip");
                return;
            }

            clip.AddEffect(effect);
            clip.Timeline?.InvalidateRender();
        });

        DropRegistry.Register<VideoClipShape, ResourceColour>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) =>
        {
            clip.ColourKey.SetTargetResourceId(h.UniqueId);
            return Task.CompletedTask;
        });

        DropRegistry.Register<ImageVideoClip, ResourceImage>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) =>
        {
            clip.ResourceImageKey.SetTargetResourceId(h.UniqueId);
            return Task.CompletedTask;
        });

        DropRegistry.Register<AVMediaVideoClip, ResourceAVMedia>((clip, h, dt, ctx) => EnumDropType.Link, async (clip, h, dt, c) =>
        {
            if (h.HasReachedResourceLimit())
            {
                int count = h.ResourceLinkLimit;
                await IoC.MessageService.ShowMessage("Resource Limit", $"This resource cannot be used by more than {count} clip{Lang.S(count)}");
                return;
            }

            clip.ResourceAVMediaKey.SetTargetResourceId(h.UniqueId);
            clip.ResourceAVMediaKey.TryLoadLink();
        });
    }
}