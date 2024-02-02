using System.Numerics;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips {
    public class ImageVideoClip : VideoClip {
        public IResourcePathKey<ResourceImage> ResourceImageKey { get; }

        private SKFilterQuality renderQuality = SKFilterQuality.Medium;
        private double renderOpacity;

        private readonly RenderLockedData<SKImage> lockedImage;

        public ImageVideoClip() {
            this.ResourceImageKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceImage>();
            this.ResourceImageKey.ResourceChanged += this.OnResoureChanged;
            this.lockedImage = new RenderLockedData<SKImage>();
        }

        public override Vector2? GetRenderSize() {
            if (this.ResourceImageKey.TryGetResource(out ResourceImage res) && res.image != null) {
                return new Vector2(res.image.Width, res.image.Height);
            }

            return null;
        }

        private void SignalDisposeImageOnRender() => this.lockedImage.Dispose();

        private void OnResoureChanged(IResourcePathKey<ResourceImage> key, ResourceImage olditem, ResourceImage newitem) {
            this.SignalDisposeImageOnRender();
            if (olditem != null)
                olditem.ImageChanged -= this.OnImageChanged;
            if (newitem != null)
                newitem.ImageChanged += this.OnImageChanged;
        }

        private void OnImageChanged(BaseResource resource) {
            this.SignalDisposeImageOnRender();
        }

        public override bool PrepareRenderFrame(PreRenderContext ctx, long frame) {
            if (this.ResourceImageKey.TryGetResource(out ResourceImage resource) && resource.image != null) {
                this.renderOpacity = this.Opacity;
                this.lockedImage.OnPrepareRender(resource.image);
                return true;
            }

            return false;
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
            if (!this.lockedImage.OnRenderBegin(out SKImage image)) {
                return;
            }

            using (SKPaint paint = new SKPaint {FilterQuality = this.renderQuality, ColorF = RenderUtils.BlendAlpha(SKColors.White, this.renderOpacity)})
                rc.Canvas.DrawImage(image, 0, 0, paint);

            renderArea = rc.Canvas.TotalMatrix.MapRect(new SKRect(0, 0, image.Width, image.Height));
            this.lockedImage.OnRenderFinished();
        }
    }
}