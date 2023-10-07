using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class ImageVideoClip : VideoClip, IResourceHolder {
        public ResourceHelper ResourceHelper { get; }

        public IResourcePathKey<ResourceImage> ResourceImageKey { get; set; }

        public override bool UseCustomOpacityCalculation => true;

        public ImageVideoClip() {
            this.ResourceHelper = new ResourceHelper(this);
            this.ResourceImageKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceImage>();
            this.ResourceImageKey.ResourceDataModified += this.ResourceHelperOnResourceDataModified;
        }

        private void ResourceHelperOnResourceDataModified(ResourceImage resourceImage, string property) {
            switch (property) {
                case nameof(ResourceImage.FilePath):
                case nameof(ResourceImage.IsRawBitmapMode):
                    this.InvalidateRender();
                    break;
            }
        }

        public override Vector2? GetSize(RenderContext rc) {
            if (!this.ResourceImageKey.TryGetResource(out ResourceImage r) || r.image == null)
                return null;
            return new Vector2(r.image.Width, r.image.Height);
        }

        public override bool OnBeginRender(long frame) {
            if (!this.ResourceImageKey.TryGetResource(out ResourceImage resource))
                return false;
            return resource.image != null;
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            if (!this.ResourceImageKey.TryGetResource(out ResourceImage resource))
                return Task.CompletedTask;
            SKImage img = resource.image;
            if (img == null)
                return Task.CompletedTask;
            using (SKPaint paint = new SKPaint {FilterQuality = rc.RenderFilterQuality, ColorF = new SKColorF(1f, 1f, 1f, (float) this.Opacity)})
                rc.Canvas.DrawImage(img, 0, 0, paint);

            return Task.CompletedTask;
        }

        protected override Clip NewInstanceForClone() {
            return new ImageVideoClip();
        }
    }
}