using System;
using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class ImageClipModel : VideoClipModel {
        public ResourcePath<ResourceImage> ResourcePath { get; private set; }

        public ImageClipModel() {

        }

        protected override void OnAddedToLayer(TimelineLayerModel oldLayer, TimelineLayerModel newLayer) {
            base.OnAddedToLayer(oldLayer, newLayer);
            this.ResourcePath?.SetManager(newLayer?.Timeline.Project.ResourceManager);
        }

        public void SetTargetResourceId(string id) {
            if (this.ResourcePath != null) {
                this.ResourcePath.ResourceChanged -= this.ResourceChanged;
                this.ResourcePath.Dispose();
            }

            this.ResourcePath = new ResourcePath<ResourceImage>(this.Layer?.Timeline.Project.ResourceManager, id);
            this.ResourcePath.ResourceChanged += this.ResourceChanged;
        }

        private void ResourceChanged(ResourceImage olditem, ResourceImage newitem) {
            if (olditem != null)
                olditem.DataModified -= this.OnResourceDataModified;
            if (newitem != null)
                newitem.DataModified += this.OnResourceDataModified;
            this.OnRenderInvalidated();
        }

        private void OnResourceDataModified(ResourceItem sender, string property) {
            if (this.ResourcePath == null)
                throw new InvalidOperationException("Expected resource path to be non-null");
            if (!this.ResourcePath.IsCachedItemEqualTo(sender))
                throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
            switch (property) {
                case nameof(ResourceImage.FilePath):
                case nameof(ResourceImage.IsRawBitmapMode):
                    this.OnRenderInvalidated();
                    break;
            }
        }

        public override Vector2 GetSize() {
            if (this.ResourcePath == null || !this.ResourcePath.TryGetResource(out ResourceImage r)) {
                return Vector2.Zero;
            }

            SKImage img = r.image;
            return img == null ? Vector2.Zero : new Vector2(img.Width, img.Height);
        }

        public override void Render(RenderContext render, long frame) {
            if (this.ResourcePath == null || !this.ResourcePath.TryGetResource(out ResourceImage resource))
                return;
            if (resource.image == null)
                return;

            this.Transform(render.Canvas, out _, out var matrix);
            render.Canvas.DrawImage(resource.image, 0, 0);
            render.Canvas.SetMatrix(matrix);
        }

        protected override void DisporeCore(ExceptionStack stack) {
            base.DisporeCore(stack);
            try {
                this.ResourcePath?.Dispose();
            }
            catch (Exception e) {
                stack.Push(e);
            }
        }
    }
}