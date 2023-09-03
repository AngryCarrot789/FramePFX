using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class ImageVideoClip : BaseResourceVideoClip<ResourceImage> {
        public ImageVideoClip() {
        }

        protected override void OnResourceDataModified(string property) {
            switch (property) {
                case nameof(ResourceImage.FilePath):
                case nameof(ResourceImage.IsRawBitmapMode):
                    this.InvalidateRender();
                    break;
            }
        }

        public override Vector2? GetSize() {
            if (this.ResourcePath == null || !this.ResourcePath.TryGetResource(out ResourceImage r) || r.image == null) {
                return null;
            }

            return new Vector2(r.image.Width, r.image.Height);
        }

        public override Task EndRender(RenderContext rc, long frame) {
            if (!this.TryGetResource(out ResourceImage resource))
                return Task.CompletedTask;
            if (resource.image == null)
                return Task.CompletedTask;

            this.Transform(rc);
            rc.Canvas.DrawImage(resource.image, 0, 0);
            return Task.CompletedTask;
        }

        protected override Clip NewInstance() {
            return new ImageVideoClip();
        }
    }
}