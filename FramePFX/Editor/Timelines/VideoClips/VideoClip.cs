using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Automation.Keys;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.RBC;
using FramePFX.Rendering;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.VideoClips {
    public abstract class VideoClip : Clip {
        public static readonly AutomationKeyDouble OpacityKey = AutomationKey.RegisterDouble(nameof(VideoClip), nameof(Opacity), 1d, 0d, 1d);

        /// <summary>
        /// The opacity; how much of this clip is visible when rendered. Ranges from 0 to 1
        /// </summary>
        public double Opacity;

        public byte OpacityByte {
            get => RenderUtils.DoubleToByte255(this.Opacity);
            set => this.Opacity = RenderUtils.Byte255ToDouble(value);
        }

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

        public new VideoTrack Track => (VideoTrack) base.Track;

        protected VideoClip() {
            // using `(VideoClip) s.AutomationData.Owner` instead of `this` saves closure allocation
            this.AutomationData.AssignKey(OpacityKey, (s, f) => ((VideoClip) s.AutomationData.Owner).Opacity = s.GetDoubleValue(f));
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
        /// <param name="rc">The rendering context, which contains frame information and a target drawing canvas</param>
        /// <returns>A nullable vector (null indicating to use the current view port size)</returns>
        public virtual Vector2? GetSize(RenderContext rc) => null;

        /// <summary>
        /// Prepares this clip for being rendered at the given frame. This is called before anything should
        /// be drawn (e.g. prepare decoder thread), and is also called before effects are processed.
        /// <para>
        /// This function may get called multiple times before <see cref="OnEndRender"/> if, for
        /// example, a render gets cancelled during the preparation phase. <see cref="OnRenderCompleted"/> will
        /// always get called after this function in that case though; there will never be a duplicate sequential call
        /// </para>
        /// </summary>
        /// <param name="frame">The frame being rendered</param>
        /// <returns>Whether or not this clip can be rendered. False means <see cref="OnEndRender"/> will not be called</returns>
        public virtual bool OnBeginRender(long frame) {
            return false;
        }

        /// <summary>
        /// Actually draw this video clip into the render context. This invocation always follows a call to <see cref="OnBeginRender"/>
        /// </summary>
        /// <param name="rc">The rendering/drawing context</param>
        /// <param name="frame">The frame being rendered</param>
        /// <returns>A task to await for the render to complete</returns>
        public virtual Task OnEndRender(RenderContext rc, long frame) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when an async render is finalized. This will only be called if <see cref="OnBeginRender"/> did not
        /// throw and returned true. This will ALWAYS follow a call to either <see cref="OnBeginRender"/> or <see cref="OnEndRender"/>
        /// </summary>
        /// <param name="frame">The frame being rendered</param>
        /// <param name="isCancelled">
        /// If the render was cancelled. This will always be false when this call is
        /// after the end render call, but may be true after beginning a render.
        /// Possible cancellation reasons are the render cancellation token expiring (timeline render took to long)
        /// or an exception occurred while rendering another clip, causing the remaining clip renders to be cancelled
        /// </param>
        public virtual void OnRenderCompleted(long frame, bool isCancelled) {
        }

        public override bool IsEffectTypeAllowed(BaseEffect effect) {
            return effect is VideoEffect;
        }
    }
}