using System.Numerics;
using FramePFX.Core.Rendering;

namespace FramePFX.Core.Editor.Timelines.VideoClips {
    public class CompositionClip : VideoClip {
        protected override Clip NewInstance() {
            return new CompositionClip();
        }

        public override Vector2? GetSize() {
            return null;
        }

        public override void Render(RenderContext rc, long frame) {
        }
    }
}