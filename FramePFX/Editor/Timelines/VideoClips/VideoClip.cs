using System;
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
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public abstract class VideoClip : Clip {
        public static readonly AutomationKeyDouble OpacityKey = AutomationKey.RegisterDouble(nameof(VideoClip), nameof(Opacity), 1d, 0d, 1d);
        private SKMatrix __internalTransformationMatrix;

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
        private bool isMatrixDirty;

        /// <summary>
        /// This video clip's transformation matrix, which is applied before it is rendered (if
        /// <see cref="OnBeginRender"/> returns true of course). This is calculated by one or
        /// more <see cref="MotionEffect"/> instances, where each instances' matrix is concatenated
        /// in their orders in our effect list
        /// </summary>
        public SKMatrix TransformationMatrix {
            get {
                if (this.isMatrixDirty)
                    this.CookTransformationMatrix();
                return this.__internalTransformationMatrix;
            }
        }

        public event RenderSizeChangedEventHandler RenderSizeChanged;

        protected VideoClip() {
            this.AutomationData.AssignKey(OpacityKey, this.CreateAssignment(OpacityKey));
            this.isMatrixDirty = true;
        }

        public void CookTransformationMatrix() {
            SKMatrix matrix = SKMatrix.Identity;
            foreach (BaseEffect effect in this.Effects) {
                if (effect is ITransformationEffect) {
                    matrix = matrix.PreConcat(((ITransformationEffect) effect).TransformationMatrix);
                }
            }

            this.__internalTransformationMatrix = matrix;
            this.isMatrixDirty = false;
        }

        /// <summary>
        /// Signals the video editor associated with this clip to render the current frame again. Optionally allows the
        /// re-render to be scheduled, making it happen at some point in the very near future
        /// </summary>
        public virtual void InvalidateRender() {
            this.RenderInvalidated?.Invoke(this);
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
        /// Gets the raw amount of space this clip takes up on screen, unaffected by standard transformation matrices.
        /// If the value is unavailable, then typically the render viewport's width and height are used as a fallback
        /// </summary>
        /// <returns>The size, if applicable, otherwise null</returns>
        public virtual Vector2? GetFrameSize() => null;

        /// <summary>
        /// Fires the <see cref="RenderSizeChanged"/> event, and invalidates the clip render, causing the timeline to be rendered.
        /// This should not be called during an export
        /// </summary>
        public virtual void OnRenderSizeChanged() {
            if (this.Project.IsExporting) {
                throw new InvalidOperationException("Render size cannot change during an export");
            }

            this.RenderSizeChanged?.Invoke(this);
            this.InvalidateRender();
        }

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

        public override bool IsEffectTypeAllowed(Type effectType) {
            return effectType.instanceof(typeof(VideoEffect));
        }

        /// <summary>
        /// Called when our <see cref="TransformationMatrix"/> should be marked as dirty,
        /// meaning it is recalculated the next time it gets requested. This is typically called
        /// by a <see cref="MotionEffect"/> when one is added, removed or when one of it's parameters changes
        /// </summary>
        public void InvalidateTransformationMatrix() {
            this.isMatrixDirty = true;
        }
    }
}