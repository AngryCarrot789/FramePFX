using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.VideoClips {
    public class ShapeClipModel : BaseResourceClip<ResourceColour> {
        public float Width { get; set; }

        public float Height { get; set; }

        public override bool UseCustomOpacityCalculation => true;

        public ShapeClipModel() {

        }

        protected override void OnResourceDataModified(string property) {
            switch (property) {
                case nameof(ResourceColour.ScA):
                case nameof(ResourceColour.ScR):
                case nameof(ResourceColour.ScG):
                case nameof(ResourceColour.ScB):
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

        public override Vector2? GetSize() {
            return new Vector2(this.Width, this.Height);
        }

        public override void Render(RenderContext render, long frame) {
            if (!this.TryGetResource(out ResourceColour r)) {
                return;
            }

            this.Transform(render);
            SKColor colour = r.Colour;
            colour = colour.WithAlpha((byte) Maths.Clamp(((colour.Alpha / 255d) * this.Opacity) * 255, 0, 255));
            using (SKPaint paint = new SKPaint() {Color = colour}) {
                render.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
            }
        }

        protected override ClipModel NewInstance() {
            return new ShapeClipModel();
        }

        protected override void LoadDataIntoClone(ClipModel clone) {
            base.LoadDataIntoClone(clone);
            ShapeClipModel clip = (ShapeClipModel) clone;
            clip.Width = this.Width;
            clip.Height = this.Height;
        }
    }
}