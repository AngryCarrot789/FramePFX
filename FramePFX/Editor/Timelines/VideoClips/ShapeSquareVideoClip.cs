using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keys;
using OpenTK.Graphics.OpenGL;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.RBC;
using FramePFX.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class ShapeSquareVideoClip : VideoClip, IResourceClip<ResourceColour> {
        public static readonly AutomationKeyFloat WidthKey = AutomationKey.RegisterFloat(nameof(ShapeSquareVideoClip), nameof(Width), 100f);
        public static readonly AutomationKeyFloat HeightKey = AutomationKey.RegisterFloat(nameof(ShapeSquareVideoClip), nameof(Height), 100f);

        // This isn't necessarily required, because the compiler will generate a hidden class with static variables
        // like this automatically when no closure allocation is required...
        private static readonly UpdateAutomationValueEventHandler UpdateWidth = (s, f) => ((ShapeSquareVideoClip) s.AutomationData.Owner).Width = s.GetFloatValue(f);
        private static readonly UpdateAutomationValueEventHandler UpdateHeight = (s, f) => ((ShapeSquareVideoClip) s.AutomationData.Owner).Height = s.GetFloatValue(f);

        public float Width { get; set; }

        public float Height { get; set; }

        public override bool UseCustomOpacityCalculation => true;

        public ResourceHelper<ResourceColour> ResourceHelper { get; }
        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;

        public ShapeSquareVideoClip() {
            this.ResourceHelper = new ResourceHelper<ResourceColour>(this);
            this.ResourceHelper.ResourceDataModified += this.ResourceHelperOnResourceDataModified;
        }

        private void ResourceHelperOnResourceDataModified(ResourceColour resource, string property) {
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

        public override Vector2? GetSize(RenderContext renderContext) {
            return new Vector2(this.Width, this.Height);
        }

        public override bool OnBeginRender(long frame) {
            return this.ResourceHelper.TryGetResource(out ResourceColour _);
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceColour r)) {
                return Task.CompletedTask;
            }

            SKColor colour = RenderUtils.BlendAlpha(r.Colour, this.Opacity);
            using (SKPaint paint = new SKPaint() {Color = colour, IsAntialias = true}) {
                rc.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
            }

            return Task.CompletedTask;
        }

        public void RenderGL(long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceColour r)) {
                return;
            }

            GL.Color3(0.2f, 0.3f, 0.7f);
            GL.Vertex3(-0.5f, -0.5f, 0f);
            GL.Vertex3(0.0f, 0.5f, 0f);
            GL.Vertex3(0.5f, -0.5f, 0f);

            // SKColor colour = RenderUtils.BlendAlpha(r.Colour, this.Opacity);
            // using (SKPaint paint = new SKPaint() {Color = colour}) {
            //     rc.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
            // }
        }

        protected override Clip NewInstance() {
            return new ShapeSquareVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
            ShapeSquareVideoClip clip = (ShapeSquareVideoClip) clone;
            this.ResourceHelper.LoadDataIntoClone(clip.ResourceHelper);
            clip.Width = this.Width;
            clip.Height = this.Height;
        }
    }
}