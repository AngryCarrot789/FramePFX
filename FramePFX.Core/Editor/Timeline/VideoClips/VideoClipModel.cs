using System.Numerics;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.VideoClips {
    public abstract class VideoClipModel : ClipModel {
        public static readonly AutomationKey MediaPositionKey =    AutomationKey.RegisterVec2(nameof(VideoClipModel), nameof(MediaPosition), Vector2.Zero, Vectors.NegativeInfinity, Vectors.PositiveInfinity);
        public static readonly AutomationKey MediaScaleKey =       AutomationKey.RegisterVec2(nameof(VideoClipModel), nameof(MediaScale), Vector2.One, Vectors.NegativeInfinity, Vectors.PositiveInfinity);
        public static readonly AutomationKey MediaScaleOriginKey = AutomationKey.RegisterVec2(nameof(VideoClipModel), nameof(MediaScaleOrigin), new Vector2(0.5f, 0.5f), Vectors.NegativeInfinity, Vectors.PositiveInfinity);
        public static readonly AutomationKey OpacityKey =          AutomationKey.RegisterDouble(nameof(VideoClipModel), nameof(Opacity), 1d, 0d, 1d);

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition { get; set; }

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale { get; set; }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin { get; set; }

        /// <summary>
        /// The opacity; how much of this clip is visible when rendered. Ranges from 0 to 1
        /// </summary>
        public double Opacity { get; set; }

        /// <summary>
        /// An event invoked when this video clip changes in some way that affects its render. 
        /// Typically handled by the view model, which schedules the video editor window's view port to render at some point in the furture
        /// </summary>
        public event ClipRenderInvalidatedEventHandler RenderInvalidated;

        protected VideoClipModel() {
            this.MediaPosition = Vector2.Zero;
            this.MediaScale = Vector2.One;
            this.MediaScaleOrigin = new Vector2(0.5f, 0.5f);
            this.Opacity = 1d;
            this.AutomationData.AssignKey(MediaPositionKey);
            this.AutomationData.AssignKey(MediaScaleKey);
            this.AutomationData.AssignKey(MediaScaleOriginKey);
            this.AutomationData.AssignKey(OpacityKey);
        }

        public override void UpdateAutomationValues(long frame) {
            base.UpdateAutomationValues(frame);
            if (this.AutomationData[MediaPositionKey].IsAutomationInUse)
                this.MediaPosition = this.AutomationData[MediaPositionKey].GetVector2Value(frame);

            if (this.AutomationData[MediaScaleKey].IsAutomationInUse)
                this.MediaScale = this.AutomationData[MediaScaleKey].GetVector2Value(frame);

            if (this.AutomationData[MediaScaleOriginKey].IsAutomationInUse)
                this.MediaScaleOrigin = this.AutomationData[MediaScaleOriginKey].GetVector2Value(frame);

            if (this.AutomationData[OpacityKey].IsAutomationInUse)
                this.Opacity = this.AutomationData[OpacityKey].GetDoubleValue(frame);
        }

        public virtual void InvalidateRender(bool schedule = true) {
            this.RenderInvalidated?.Invoke(this, schedule);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
        }

        /// <summary>
        /// Gets the size of this video clip's media, which is used in the scale phase of the
        /// <see cref="Transform(FramePFX.Core.Rendering.RenderContext,out System.Nullable{System.Numerics.Vector2},out SkiaSharp.SKMatrix)"/> call.
        /// If the value is unavailable, then the render viewport's width and height are used as a fallback
        /// </summary>
        /// <returns></returns>
        public abstract Vector2? GetSize();

        public void Transform(RenderContext rc, out Vector2? size, out SKMatrix oldMatrix) {
            oldMatrix = rc.Canvas.TotalMatrix;
            this.Transform(rc, out size);
        }

        public void Transform(RenderContext rc, out Vector2? size) {
            Vector2 pos = this.MediaPosition, scale = this.MediaScale, origin = this.MediaScaleOrigin;

            size = this.GetSize();
            rc.Canvas.Translate(pos.X, pos.Y);
            if (size is Vector2 sz) {
                rc.Canvas.Scale(scale.X, scale.Y, sz.X * origin.X, sz.Y * origin.Y);
            }
            else {
                rc.Canvas.Scale(scale.X, scale.Y, rc.FrameInfo.Width, rc.FrameInfo.Height);
            }
        }

        public void Transform(RenderContext rc) {
            this.Transform(rc, out _);
        }

        public abstract void Render(RenderContext render, long frame);

        protected override void LoadDataIntoClone(ClipModel clone) {
            base.LoadDataIntoClone(clone);
        }
    }
}