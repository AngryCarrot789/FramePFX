using System;
using System.Diagnostics;
using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class SquareClipModel : VideoClipModel {
        public float Width { get; set; }

        public float Height { get; set; }

        public ResourcePath<ResourceARGB> ResourcePath { get; private set; }

        public SquareClipModel() {

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

            this.ResourcePath = new ResourcePath<ResourceARGB>(this.Layer?.Timeline.Project.ResourceManager, id);
            this.ResourcePath.ResourceChanged += this.ResourceChanged;
        }

        private void ResourceChanged(ResourceARGB olditem, ResourceARGB newitem) {
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
                case nameof(ResourceARGB.A):
                case nameof(ResourceARGB.R):
                case nameof(ResourceARGB.G):
                case nameof(ResourceARGB.B):
                    this.OnRenderInvalidated();
                    break;
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (this.ResourcePath != null)
                ResourcePath<ResourceARGB>.WriteToRBE(this.ResourcePath, data.GetOrCreateDictionaryElement(nameof(this.ResourcePath)));
            data.SetFloat(nameof(this.Width), this.Width);
            data.SetFloat(nameof(this.Height), this.Height);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            if (data.TryGetElement(nameof(this.ResourcePath), out RBEDictionary resource))
                this.ResourcePath = ResourcePath<ResourceARGB>.ReadFromRBE(this.Layer?.Timeline.Project.ResourceManager, resource);
            this.Width = data.GetFloat(nameof(this.Width));
            this.Height = data.GetFloat(nameof(this.Height));
        }

        public override Vector2 GetSize() {
            return new Vector2(this.Width, this.Height);
        }

        public override void Render(RenderContext render, long frame) {
            if (this.ResourcePath == null || !this.ResourcePath.TryGetResource(out ResourceARGB r)) {
                return;
            }

            this.Transform(render.Canvas, out Rect rect, out SKMatrix oldMatrix);
            render.Canvas.DrawRect(rect.X1, rect.Y1, rect.Width, rect.Height, new SKPaint() {
                Color = new SKColor(r.ByteR, r.ByteG, r.ByteB, r.ByteA)
            });

            render.Canvas.SetMatrix(oldMatrix);
        }

        protected override void DisporeCore(ExceptionStack stack) {
            base.DisporeCore(stack);
            try {
                // this shouldn't throw unless it was already disposed for some reason. Might as well handle that case
                this.ResourcePath?.Dispose();
            }
            catch (Exception e) {
                stack.Push(e);
            }
        }
    }
}