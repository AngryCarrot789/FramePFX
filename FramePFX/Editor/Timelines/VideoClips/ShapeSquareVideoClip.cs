using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keys;
using FramePFX.Editor.Rendering;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class ShapeSquareVideoClip : VideoClip {
        public static readonly AutomationKeyFloat WidthKey = AutomationKey.RegisterFloat(nameof(ShapeSquareVideoClip), nameof(Width), 100f);
        public static readonly AutomationKeyFloat HeightKey = AutomationKey.RegisterFloat(nameof(ShapeSquareVideoClip), nameof(Height), 100f);

        // This isn't necessarily required, because the compiler will generate a hidden class with static variables
        // like this automatically when no closure allocation is required...
        private static readonly UpdateAutomationValueEventHandler UpdateWidth = (s, f) => ((ShapeSquareVideoClip) s.AutomationData.Owner).Width = s.GetFloatValue(f);
        private static readonly UpdateAutomationValueEventHandler UpdateHeight = (s, f) => ((ShapeSquareVideoClip) s.AutomationData.Owner).Height = s.GetFloatValue(f);

        public float Width;
        public float Height;

        public override bool UseCustomOpacityCalculation => true;

        public IResourcePathKey<ResourceColour> ColourKey { get; }

        public ShapeSquareVideoClip() {
            this.AutomationData.AssignKey(WidthKey, this.CreateAssignment(WidthKey));
            this.AutomationData.AssignKey(HeightKey, this.CreateAssignment(HeightKey));
            this.ColourKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceColour>();
            this.ColourKey.ResourceDataModified += this.ResourceHelperOnResourceDataModified;
        }

        private void ResourceHelperOnResourceDataModified(IResourcePathKey<ResourceColour> key, ResourceColour resource, string property) {
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

        public override Vector2? GetFrameSize() {
            return new Vector2(this.Width, this.Height);
        }

        public override bool OnBeginRender(long frame) {
            if (!this.ColourKey.TryGetResource(out ResourceColour _)) {
                return false;
            }

            return true;
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            if (this.ColourKey.TryGetResource(out ResourceColour r)) {
                SKColor colour = RenderUtils.BlendAlpha(r.Colour, this.Opacity);
                using (SKPaint paint = new SKPaint() {Color = colour, IsAntialias = true}) {
                    rc.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
                }
            }

            return Task.CompletedTask;
        }

        protected override Clip NewInstanceForClone() {
            return new ShapeSquareVideoClip();
        }

        protected override void LoadUserDataIntoClone(Clip clone, ClipCloneFlags flags) {
            base.LoadUserDataIntoClone(clone, flags);
            ShapeSquareVideoClip clip = (ShapeSquareVideoClip) clone;
            clip.Width = this.Width;
            clip.Height = this.Height;
        }
    }
}