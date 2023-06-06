using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class ShapeClipModel : BaseResourceClip<ResourceColour> {
        public float Width { get; set; }

        public float Height { get; set; }

        public ShapeClipModel() {

        }

        protected override void OnResourceDataModified(string property) {
            switch (property) {
                case nameof(ResourceColour.A):
                case nameof(ResourceColour.R):
                case nameof(ResourceColour.G):
                case nameof(ResourceColour.B):
                    this.InvalidateRender();
                    break;
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetFloat(nameof(this.Width), this.Width);
            data.SetFloat(nameof(this.Height), this.Height);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Width = data.GetFloat(nameof(this.Width));
            this.Height = data.GetFloat(nameof(this.Height));
        }

        public override Vector2 GetSize() {
            return new Vector2(this.Width, this.Height);
        }

        public override void Render(RenderContext render, long frame, SKColorFilter alphaFilter) {
            if (!this.TryGetResource(out ResourceColour r)) {
                return;
            }

            this.Transform(render.Canvas, out Rect rect, out SKMatrix oldMatrix);
            render.Canvas.DrawRect(rect.X1, rect.Y1, rect.Width, rect.Height, new SKPaint() {
                Color = new SKColor(r.ByteR, r.ByteG, r.ByteB, r.ByteA),
                ColorFilter = alphaFilter
            });

            render.Canvas.SetMatrix(oldMatrix);
        }
    }
}