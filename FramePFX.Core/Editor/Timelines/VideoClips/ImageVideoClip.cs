using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;

namespace FramePFX.Core.Editor.Timelines.VideoClips {
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

        public override void Render(RenderContext rc, long frame) {
            if (!this.TryGetResource(out ResourceImage resource))
                return;
            if (resource.image == null)
                return;

            this.Transform(rc);
            rc.Canvas.DrawImage(resource.image, 0, 0);
        }

        protected override Clip NewInstance() {
            return new ImageVideoClip();
        }
    }
}