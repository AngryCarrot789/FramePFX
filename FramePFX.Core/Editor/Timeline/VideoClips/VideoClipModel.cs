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
        public Vector2 MediaPosition => this.AutomationData[MediaPositionKey].GetVector2Value(this.TimelinePlayhead);

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale => this.AutomationData[MediaScaleKey].GetVector2Value(this.TimelinePlayhead);

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin => this.AutomationData[MediaScaleOriginKey].GetVector2Value(this.TimelinePlayhead);

        /// <summary>
        /// The opacity; how much of this clip is visible when rendered. Ranges from 0 to 1
        /// </summary>
        public double Opacity => this.AutomationData[OpacityKey].GetDoubleValue(this.TimelinePlayhead);

        /// <summary>
        /// An event invoked when this video clip changes in some way that affects its render. 
        /// Typically handled by the view model, which schedules the video editor window's view port to render at some point in the furture
        /// </summary>
        public event ClipRenderInvalidatedEventHandler RenderInvalidated;

        protected VideoClipModel() {
            this.AutomationData.AssignKey(MediaPositionKey);
            this.AutomationData.AssignKey(MediaScaleKey);
            this.AutomationData.AssignKey(MediaScaleOriginKey);
            this.AutomationData.AssignKey(OpacityKey);
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
        /// Gets the size of this video clip's media, which is used in the transformation phase
        /// using <see cref="MediaPosition"/>, <see cref="MediaScale"/> and <see cref="MediaScaleOrigin"/>
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetSize();

        public void Transform(SKCanvas canvas, out Vector2 size, out SKMatrix oldMatrix) {
            oldMatrix = canvas.TotalMatrix;
            this.Transform(canvas, out size);
        }

        public void Transform(SKCanvas canvas, out Vector2 size) {
            Vector2 pos = this.MediaPosition, scale = this.MediaScale, origin = this.MediaScaleOrigin;
            size = this.GetSize();
            canvas.Translate(pos.X, pos.Y);
            canvas.Scale(scale.X, scale.Y, size.X * origin.X, size.Y * origin.Y);
        }

        public void Transform(SKCanvas canvas) {
            this.Transform(canvas, out _);
        }

        public abstract void Render(RenderContext render, long frame);

        protected override void LoadDataIntoClone(ClipModel clone) {
            base.LoadDataIntoClone(clone);
        }
    }
}