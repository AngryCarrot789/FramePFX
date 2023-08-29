using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.Effects.Video {
    public abstract class VideoEffect : BaseEffect {
        /// <summary>
        /// The video clip that owns this effect
        /// </summary>
        public new VideoClip OwnerClip {
            get => (VideoClip) base.OwnerClip;
            set => base.OwnerClip = value;
        }

        protected VideoEffect() {

        }

        public virtual void ProcessFrame(RenderContext rc) {
        }
    }
}