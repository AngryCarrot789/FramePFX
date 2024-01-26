using System.Collections.Generic;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;

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

        protected VideoEffect() {
        }

        /// <summary>
        /// Called after a video clip's <see cref="VideoClip.PrepareRenderFrame"/> method when it returns true
        /// <para>
        /// This method is called on the application main thread
        /// </para>
        /// </summary>
        /// <param name="ctx">The pre-render context information</param>
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
        /// Called after <see cref="PreProcessFrame"/> and after a clip has been drawn to the <see cref="RenderContext"/>.
        /// This can be used to, for example, create some weird effects on the clip. This is where you'd actually do your frame modification
        /// <para>
        /// This is called on a rendering thread
        /// </para>
        /// </summary>
        /// <param name="rc">The rendering context</param>
        public virtual void PostProcessFrame(RenderContext rc) {
        }

        public static void ProcessEffectList(IList<BaseEffect> effects, long frame, RenderContext render, bool isPreProcess) {
            // pre-process clip effects, such as translation, scale, etc.
            int i, count = effects.Count;
            if (count == 0) {
                return;
            }

            BaseEffect effect;
            if (isPreProcess) {
                for (i = 0; i < count; i++) {
                    if ((effect = effects[i]) is VideoEffect) {
                        ((VideoEffect) effect).PreProcessFrame(render);
                    }
                }
            }
            else {
                for (i = count - 1; i >= 0; i--) {
                    if ((effect = effects[i]) is VideoEffect) {
                        ((VideoEffect) effect).PostProcessFrame(render);
                    }
                }
            }
        }
    }
}