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

using System.Numerics;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.NewResourceHelper;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines.Clips.Video;
using SkiaSharp;

namespace FramePFX.Editing.Timelines.Clips.Core;

public class ImageVideoClip : VideoClip {
    private readonly RenderLockedData<SKImage> lockedImage;

    public static readonly ResourceSlot<ResourceImage> ResourceImageKey = ResourceSlot.Register<ResourceImage>(typeof(ImageVideoClip), "ImageKey");

    public ImageVideoClip() {
        this.UsesCustomOpacityCalculation = true;
        ResourceImageKey.AddResourceChangedHandler(this, this.OnResourceChanged);
        this.lockedImage = new RenderLockedData<SKImage>();
    }

    static ImageVideoClip() {
        ResourceImageKey.ResourceChanged += (slot, owner, oldResource, newResource) => {
            ImageVideoClip clip = (ImageVideoClip) owner;
            clip.SignalDisposeImage();
            if (oldResource != null)
                ((ResourceImage) oldResource).ImageChanged -= clip.OnImageChanged;
            if (newResource != null)
                ((ResourceImage) newResource).ImageChanged += clip.OnImageChanged;
        };
    }

    public override Vector2? GetRenderSize() {
        if (ResourceImageKey.TryGetResource(this, out ResourceImage? res) && res.image != null) {
            return new Vector2(res.image.Width, res.image.Height);
        }

        return null;
    }

    private void SignalDisposeImage() => this.lockedImage.Dispose();

    private void OnResourceChanged(IResourceHolder owner, ResourceSlot slot, ResourceItem? oldItem, ResourceItem? newItem) {
    }

    private void OnImageChanged(BaseResource resource) => this.SignalDisposeImage();

    public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
        if (ResourceImageKey.TryGetResource(this, out ResourceImage? resource) && resource.image != null) {
            this.lockedImage.OnPrepareRender(resource.image);
            return true;
        }

        return false;
    }

    public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
        if (!this.lockedImage.OnRenderBegin(out SKImage image)) {
            return;
        }

        using (SKPaint paint = new SKPaint()) {
            paint.FilterQuality = rc.FilterQuality;
            paint.Color = RenderUtils.BlendAlpha(SKColors.White, this.RenderOpacity);
            rc.Canvas.DrawImage(image, 0, 0, paint);
        }

        renderArea = rc.TranslateRect(new SKRect(0, 0, image.Width, image.Height));
        this.lockedImage.OnRenderFinished();
    }
}