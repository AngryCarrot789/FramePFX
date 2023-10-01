using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keys;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.RBC;
using FramePFX.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips
{
    public class ShapeSquareVideoClip : VideoClip, IResourceClip<ResourceColour>
    {
        public static readonly AutomationKeyFloat WidthKey = AutomationKey.RegisterFloat(nameof(ShapeSquareVideoClip), nameof(Width), 100f);
        public static readonly AutomationKeyFloat HeightKey = AutomationKey.RegisterFloat(nameof(ShapeSquareVideoClip), nameof(Height), 100f);

        // This isn't necessarily required, because the compiler will generate a hidden class with static variables
        // like this automatically when no closure allocation is required...
        private static readonly UpdateAutomationValueEventHandler UpdateWidth = (s, f) => ((ShapeSquareVideoClip) s.AutomationData.Owner).Width = s.GetFloatValue(f);
        private static readonly UpdateAutomationValueEventHandler UpdateHeight = (s, f) => ((ShapeSquareVideoClip) s.AutomationData.Owner).Height = s.GetFloatValue(f);

        public float Width;
        public float Height;

        public override bool UseCustomOpacityCalculation => true;

        public ResourceHelper<ResourceColour> ResourceHelper { get; }
        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;

        public ShapeSquareVideoClip()
        {
            this.AutomationData.AssignKey(WidthKey, (s, f) => ((ShapeSquareVideoClip) s.AutomationData.Owner).Width = s.GetFloatValue(f));
            this.AutomationData.AssignKey(HeightKey, (s, f) => ((ShapeSquareVideoClip) s.AutomationData.Owner).Height = s.GetFloatValue(f));
            this.ResourceHelper = new ResourceHelper<ResourceColour>(this);
            this.ResourceHelper.ResourceDataModified += this.ResourceHelperOnResourceDataModified;
        }

        private void ResourceHelperOnResourceDataModified(ResourceColour resource, string property)
        {
            switch (property)
            {
                case nameof(ResourceColour.Colour):
                    this.InvalidateRender();
                    break;
            }
        }

        public override void WriteToRBE(RBEDictionary data)
        {
            base.WriteToRBE(data);
            data.SetFloat(nameof(this.Width), this.Width);
            data.SetFloat(nameof(this.Height), this.Height);
        }

        public override void ReadFromRBE(RBEDictionary data)
        {
            base.ReadFromRBE(data);
            this.Width = data.GetFloat(nameof(this.Width));
            this.Height = data.GetFloat(nameof(this.Height));
        }

        public override Vector2? GetSize(RenderContext rc)
        {
            return new Vector2(this.Width, this.Height);
        }

        public override bool OnBeginRender(long frame)
        {
            if (!this.ResourceHelper.TryGetResource(out ResourceColour _))
            {
                return false;
            }

            return true;
        }

        public override Task OnEndRender(RenderContext rc, long frame)
        {
            if (this.ResourceHelper.TryGetResource(out ResourceColour r))
            {
                Matrix4x4 matrix = Matrix4x4.CreateScale(this.Width / 2f, this.Height / 2f, 1f) * rc.Matrix;
                Matrix4x4 mvp = matrix * rc.CameraView * rc.Projection;

                this.Track.Timeline.BasicShader.Use();
                this.Track.Timeline.BasicShader.SetUniformMatrix4("mvp", ref mvp);
                this.Track.Timeline.BasicShader.SetUniformVec4("in_colour", new Vector4(r.ScR, r.ScG, r.ScB, (float)this.Opacity));
                ((VideoTrack) this.Track).BasicRectangle.DrawTriangles();
            }

            return Task.CompletedTask;
        }

        protected override Clip NewInstanceForClone()
        {
            return new ShapeSquareVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone, ClipCloneFlags flags)
        {
            base.LoadDataIntoClone(clone, flags);
            ShapeSquareVideoClip clip = (ShapeSquareVideoClip) clone;
            this.ResourceHelper.LoadDataIntoClone(clip.ResourceHelper);
            clip.Width = this.Width;
            clip.Height = this.Height;
        }
    }
}