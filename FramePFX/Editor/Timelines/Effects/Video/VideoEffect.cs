using FramePFX.Editor.Rendering;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.Timelines.Effects.Video {
    public abstract class VideoEffect : BaseEffect {
        /// <summary>
        /// The video clip that owns this effect
        /// </summary>
        public new VideoClip OwnerClip => (VideoClip) base.OwnerClip;

        protected VideoEffect() {
        }

        /// <summary>
        /// Called before a clip is rendered. This can be used to, for example, translate a clip's
        /// video frame location via the <see cref="RenderContext.Canvas"/> property
        /// </summary>
        /// <param name="frame">The time frame at which the owner clip is being rendered</param>
        /// <param name="rc">The rendering context</param>
        public virtual void PreProcessFrame(long frame, RenderContext rc) {
        }

        /// <summary>
        /// Called after <see cref="PreProcessFrame"/> and after a clip has been drawn to the <see cref="RenderContext"/>.
        /// This can be used to, for example, create some weird effects on the clip. This is where you'd actually do your frame modification
        /// </summary>
        /// <param name="frame">The time frame at which the owner clip is being rendered</param>
        /// <param name="rc">The rendering context</param>
        public virtual void PostProcessFrame(long frame, RenderContext rc) {
        }
    }
}