using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class ImageVideoClip : VideoClip, IResourceClip<ResourceImage> {
        public ResourceHelper<ResourceImage> ResourceHelper { get; }
        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;

        public ImageVideoClip() {
            this.ResourceHelper = new ResourceHelper<ResourceImage>(this);
            this.ResourceHelper.ResourceDataModified += this.ResourceHelperOnResourceDataModified;
        }

        private void ResourceHelperOnResourceDataModified(ResourceImage resource, string property) {
            switch (property) {
                case nameof(ResourceImage.FilePath):
                case nameof(ResourceImage.IsRawBitmapMode):
                    this.InvalidateRender();
                    break;
            }
        }

        public override Vector2? GetSize() {
            if (!this.ResourceHelper.HasPath || !this.ResourceHelper.ResourcePath.TryGetResource(out ResourceImage r) || r.image == null) {
                return null;
            }

            return new Vector2(r.image.Width, r.image.Height);
        }

        public override bool OnBeginRender(long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceImage resource))
                return false;
            return resource.image != null;
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceImage resource))
                return Task.CompletedTask;
            if (resource.image == null)
                return Task.CompletedTask;

            // this.ApplyTransformation(rc);
            rc.Canvas.DrawImage(resource.image, 0, 0);
            return Task.CompletedTask;
        }

        protected override Clip NewInstance() {
            return new ImageVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
            this.ResourceHelper.LoadDataIntoClone(((ImageVideoClip) clone).ResourceHelper);
        }
    }
}