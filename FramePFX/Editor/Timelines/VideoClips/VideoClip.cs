using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Automation.Keys;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.RBC;
using FramePFX.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public abstract class VideoClip : Clip {
        public static readonly AutomationKeyDouble OpacityKey = AutomationKey.RegisterDouble(nameof(VideoClip), nameof(Opacity), 1d, 0d, 1d);

        // saves using closure allocation for each clip
        private static readonly UpdateAutomationValueEventHandler UpdateOpacity = (s, f) => ((VideoClip) s.AutomationData.Owner).Opacity = s.GetDoubleValue(f);

        /// <summary>
        /// The opacity; how much of this clip is visible when rendered. Ranges from 0 to 1
        /// </summary>
        public double Opacity;

        public byte OpacityByte => RenderUtils.DoubleToByte(this.Opacity);

        /// <summary>
        /// Whether or not this clip handles it's own opacity calculation to help with render performance. Default
        /// value is false, meaning an <see cref="Opacity"/> value that isn't 1d requires a temporary bitmap to render the clip
        /// </summary>
        public virtual bool UseCustomOpacityCalculation { get => false; }

        /// <summary>
        /// An event invoked when this video clip changes in some way that affects its render. 
        /// Typically handled by the view model, which schedules the video editor window's view port to render at some point in the furture
        /// </summary>
        public event ClipRenderInvalidatedEventHandler RenderInvalidated;

        public MotionEffect Motion => (MotionEffect) this.Effects[0];

        protected VideoClip() {
            this.Effects.Add(new MotionEffect(this));
            this.Opacity = OpacityKey.Descriptor.DefaultValue;
            this.AutomationData.AssignKey(OpacityKey, UpdateOpacity);
        }

        /// <summary>
        /// Signals the video editor associated with this clip to render the current frame again. Optionally allows the
        /// re-render to be scheduled, making it happen at some point in the very near future
        /// </summary>
        /// <param name="schedule">Schedule for the future and not in the current call</param>
        public virtual void InvalidateRender(bool schedule = true) {
            this.RenderInvalidated?.Invoke(this, schedule);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetDouble(nameof(this.Opacity), this.Opacity);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Opacity = data.GetDouble(nameof(this.Opacity));
        }

        /// <summary>
        /// Gets the amount of space this clip takes up on screen (unaffected by <see cref="MediaPosition"/> or <see cref="MediaScale"/>).
        /// If the value is unavailable, then the render viewport's width and height are used as a fallback
        /// </summary>
        /// <returns>A nullable vector (null indicating to use the current view port size)</returns>
        public virtual Vector2? GetSize() => null;

        public void Transform(RenderContext rc, out Vector2? size, out SKMatrix oldMatrix) {
            oldMatrix = rc.Canvas.TotalMatrix;
            this.Transform(rc, out size);
        }

        public void Transform(RenderContext rc, out Vector2? size) {
            MotionEffect motion = this.Motion;
            Vector2 pos = motion.MediaPosition, scale = motion.MediaScale, origin = motion.MediaScaleOrigin;
            rc.Canvas.Translate(pos.X, pos.Y);
            Vector2 sz;
            size = this.GetSize();
            if (size.HasValue) {
                sz = size.Value;
            }
            else {
                size = sz = new Vector2(rc.FrameInfo.Width, rc.FrameInfo.Height);
            }

            if (motion.UseAbsoluteScaleOrigin) {
                rc.Canvas.Scale(scale.X, scale.Y, origin.X, origin.Y);
            }
            else {
                rc.Canvas.Scale(scale.X, scale.Y, sz.X * origin.X, sz.Y * origin.Y);
            }
        }

        public void Transform(RenderContext rc) {
            this.Transform(rc, out _);
        }

        /// <summary>
        /// Setup a new render for this clip at the given frame. This is called before anything is drawn.
        /// <para>
        /// This function may get called multiple times before <see cref="EndRender"/> if, for example, a render gets cancelled
        /// </para>
        /// </summary>
        /// <param name="frame">The frame being rendered</param>
        /// <returns>Whether or not this clip can be rendered. False means <see cref="EndRender"/> will not be called</returns>
        public virtual bool BeginRender(long frame) {
            return false;
        }

        /// <summary>
        /// Actually draw this video clip into the render context. <see cref="BeginRender"/> is always called before this method call
        /// </summary>
        /// <param name="rc">The rendering/drawing context</param>
        /// <param name="frame">The frame being rendered</param>
        /// <returns>A task to await for the render to complete</returns>
        public virtual Task EndRender(RenderContext rc, long frame) {
            return Task.CompletedTask;
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
        }
    }
}