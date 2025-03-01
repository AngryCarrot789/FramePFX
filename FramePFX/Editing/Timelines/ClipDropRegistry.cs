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
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;

namespace FramePFX.Editing.Timelines;

public static class ClipDropRegistry {
    public static DragDropRegistry<Clip> DropRegistry { get; }

    static ClipDropRegistry() {
        DropRegistry = new DragDropRegistry<Clip>();
        DropRegistry.Register<Clip, EffectProviderEntry>((clip, x, dt, ctx) => {
            return clip.IsEffectTypeAccepted(x.EffectType) ? EnumDropType.Copy : EnumDropType.None;
        }, async (clip, x, dt, ctx) => {
            BaseEffect effect;
            try {
                effect = x.CreateEffect();
            }
            catch (Exception e) {
                await IMessageDialogService.Instance.ShowMessage("Error", "Failed to create effect from the dropped effect", e.GetToString());
                return;
            }

            if (!effect.IsObjectValidForOwner(clip)) {
                await IMessageDialogService.Instance.ShowMessage("Error", "This effect is not allowed to be placed in this clip");
                return;
            }

            clip.AddEffect(effect);
            clip.Timeline?.InvalidateRender();
        });

        DropRegistry.Register<VideoClipShape, ResourceColour>((clip, h, dt, ctx) => EnumDropType.Link, async (clip, h, dt, c) => {
            await clip.ResourceHelper.SetResourceHelper(VideoClipShape.ColourKey, h);
        });

        DropRegistry.Register<ImageVideoClip, ResourceImage>((clip, h, dt, ctx) => EnumDropType.Link, async (clip, h, dt, c) => {
            await clip.ResourceHelper.SetResourceHelper(ImageVideoClip.ResourceImageKey, h);
        });

        DropRegistry.Register<CompositionVideoClip, ResourceComposition>((clip, h, dt, ctx) => EnumDropType.Link, async (clip, h, dt, c) => {
            if (h.HasReachedResourceLimit()) {
                int count = h.ResourceLinkLimit;
                await IMessageDialogService.Instance.ShowMessage("Resource Limit", $"At the moment, composition timelines cannot be used by more than {count} clip{Lang.S(count)}");
                return;
            }

            clip.ResourceHelper.SetResource(CompositionVideoClip.ResourceCompositionKey, h);
        });
    }
}