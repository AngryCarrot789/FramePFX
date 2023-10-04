using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips
{
    public class ImageVideoClip : VideoClip, IResourceHolder
    {
        public ResourceHelper ResourceHelper { get; }

        public IResourcePathKey<ResourceImage> ImageKey { get; set; }

        public ImageVideoClip()
        {
            this.ResourceHelper = new ResourceHelper(this);
            this.ImageKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceImage>();
            this.ImageKey.ResourceDataModified += this.ResourceHelperOnResourceDataModified;
        }

        private void ResourceHelperOnResourceDataModified(ResourceImage resourceImage, string property)
        {
            switch (property)
            {
                case nameof(ResourceImage.FilePath):
                case nameof(ResourceImage.IsRawBitmapMode):
                    this.InvalidateRender();
                    break;
            }
        }

        public override Vector2? GetSize(RenderContext rc)
        {
            if (!this.ImageKey.TryGetResource(out ResourceImage r) || r.image == null)
                return null;
            return new Vector2(r.image.Width, r.image.Height);
        }

        public override bool OnBeginRender(long frame)
        {
            if (!this.ImageKey.TryGetResource(out ResourceImage resource))
                return false;
            return resource.image != null;
        }

        public override Task OnEndRender(RenderContext rc, long frame)
        {
            if (!this.ImageKey.TryGetResource(out ResourceImage resource))
                return Task.CompletedTask;
            if (resource.image == null)
                return Task.CompletedTask;
            // rc.Canvas.DrawImage(resource.image, 0, 0);
            return Task.CompletedTask;
        }

        protected override Clip NewInstanceForClone()
        {
            return new ImageVideoClip();
        }
    }
}