using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Effects {
    public abstract class VideoEffect : BaseEffect {
        /// <summary>
        /// Casts <see cref="Owner"/> to a <see cref="VideoClip"/>
        /// </summary>
        public new VideoClip OwnerClip => (VideoClip) this.Owner;

        /// <summary>
        /// Casts <see cref="Owner"/> to a <see cref="VideoTrack"/>
        /// </summary>
        public new VideoTrack OwnerTrack => (VideoTrack) this.Owner;

        /// <summary>
        /// The render area rect that a clip generated via its <see cref="VideoClip.RenderFrame"/> method.
        /// As this is only set after the clip is rendered, it is only valid in our <see cref="PostProcessFrame"/>
        /// method. . This value is set for both track and clip effects, just in case the track
        /// effect wants to do further optimisation  needs to know
        /// </summary>
        public SKRect ClipRenderArea { get; set; }

        protected VideoEffect() {
        }

        public override bool IsObjectValidForOwner(IHaveEffects owner) => owner is VideoClip || owner is VideoTrack;

        /// <summary>
        /// Called after a video clip's <see cref="VideoClip.PrepareRenderFrame"/> method when it returns true
        /// <para>
        /// This method is called on the application main thread
        /// </para>
        /// </summary>
        /// <param name="ctx">The pre-render setup context information</param>
        /// <param name="frame">The frame, relative to the clip, being rendered</param>
        public virtual void PrepareRender(PreRenderContext ctx, long frame) {

        }

        /// <summary>
        /// Called before a clip is rendered. This can be used to, for example, translate a clip's
        /// video frame location via the <see cref="RenderContext.Canvas"/> property.
        /// <para>
        /// This is called on a rendering thread
        /// </para>
        /// </summary>
        /// <param name="rc">The rendering context</param>
        public virtual void PreProcessFrame(RenderContext rc) {
        }

        /// <summary>
        /// Called after <see cref="PreProcessFrame"/> and after a clip has been drawn to the
        /// <see cref="RenderContext"/>. This can be used to, for example, create some weird effects
        /// on the clip. This is where you'd actually do your frame modification
        /// <para>
        /// This is called on a rendering thread
        /// </para>
        /// </summary>
        /// <param name="rc">The rendering context</param>
        /// <param name="renderArea">
        /// The render area rect that a clip generated via its <see cref="VideoClip.RenderFrame"/> method.
        /// The rect contains the actual area that was rendered into, but may default to the full frame
        /// if the clip is unoptimised. This value is valid for both clip and track effects just in case
        /// they need it, and can be modified by both in case they affect the final rendering area (e.g.
        /// liquify effect).
        /// <para>
        /// <see cref="MotionEffect"/> is an exception where it does affect the render area but doesn't modify
        /// this value, because the clip calculates its render area from the current matrix. Therefore,
        /// effects extending <see cref="ITransformationEffect"/> shouldn't really need to modify this
        /// </para>
        /// </param>
        public virtual void PostProcessFrame(RenderContext rc, ref SKRect renderArea) {
        }
    }
}