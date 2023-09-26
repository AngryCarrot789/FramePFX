using System.Numerics;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class AdjustmentVideoClip : VideoClip {
        public AdjustmentVideoClip() {
        }

        public override Vector2? GetSize(RenderContext rc) => rc.FrameSize;

        protected override Clip NewInstance() {
            return new AdjustmentVideoClip();
        }
    }
}