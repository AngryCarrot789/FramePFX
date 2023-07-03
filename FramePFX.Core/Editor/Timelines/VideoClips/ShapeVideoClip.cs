using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timelines.VideoClips {
    public class ShapeVideoClip : BaseResourceVideoClip<ResourceColour> {
        public float Width { get; set; }

        public float Height { get; set; }

        public override bool UseCustomOpacityCalculation => true;

        public ShapeVideoClip() {

        }

        protected override void OnResourceDataModified(string property) {
            switch (property) {
                case nameof(ResourceColour.Colour):
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

        public override void Render(RenderContext rc, long frame) {
            if (!this.TryGetResource(out ResourceColour r)) {
                return;
            }

            this.Transform(rc);
            SKColor colour = RenderUtils.BlendAlpha(r.Colour, this.Opacity);
            using (SKPaint paint = new SKPaint() {Color = colour}) {
                rc.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
            }
        }

        protected override Clip NewInstance() {
            return new ShapeVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
            ShapeVideoClip clip = (ShapeVideoClip) clone;
            clip.Width = this.Width;
            clip.Height = this.Height;
        }
    }
}