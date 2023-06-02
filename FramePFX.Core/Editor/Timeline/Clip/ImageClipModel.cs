using System;
using System.Numerics;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.ResourceManaging;
using FramePFX.Core.ResourceManaging.Resources;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class ImageClipModel : VideoClipModel {
        public string ImageResourceId { get; set; }

        public bool IsResourceOffline { get; set; }

        private ImageResourceItem cachedItem;

        public ImageClipModel() {
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            if (data.TryGetString(nameof(this.ImageResourceId), out string id)) {
                this.ImageResourceId = id;
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
        }

        public bool TryGetResource(out ImageResourceItem resource) {
            if (this.IsResourceOffline) {
                resource = default;
                return false;
            }

            if (this.cachedItem != null) {
                resource = this.cachedItem;
                return true;
            }

            if (this.ImageResourceId == null || this.Layer == null) {
                resource = null;
                return false;
            }

            // The exceptional cases should never be thrown. When the resource manager and clip are all loaded, there
            // should be a function that runs to detect missing resource ids, and offer to replace, them or remove the id
            ResourceManager manager = this.Layer.Timeline.Project.ResourceManager;
            if (!manager.TryGetResource(this.ImageResourceId, out ResourceItem resItem))
                throw new Exception($"Could not find resources ({this.ImageResourceId})");
            if (!(resItem is ImageResourceItem imgRes))
                throw new Exception($"Resource was an image ({this.ImageResourceId})");
            this.cachedItem = resource = imgRes;
            manager.AddHandler(this.ImageResourceId);
            return true;
        }

        public override Vector2 GetSize() {
            if (!this.TryGetResource(out ImageResourceItem resource)) {
                return Vector2.Zero;
            }

            SKImage img = resource.image;
            return img == null ? Vector2.Zero : new Vector2(img.Width, img.Height);
        }

        public override void Render(RenderContext ctx, long frame) {
            if (!this.TryGetResource(out ImageResourceItem resource)) {
                return;
            }

            if (resource.image == null) {
                return;
            }

            this.Transform(ctx.Canvas, out _, out var matrix);
            ctx.Canvas.DrawImage(resource.image, 0, 0);
            ctx.Canvas.SetMatrix(matrix);
        }
    }
}