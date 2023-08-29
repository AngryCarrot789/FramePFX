using System;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.Rendering;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timelines.Effects.Video {
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