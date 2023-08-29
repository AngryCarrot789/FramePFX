using System.Numerics;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class MpegMediaVideoClip : BaseResourceVideoClip<ResourceMpegMedia> {
        public int StreamIndex { get; set; }

        public MpegMediaVideoClip() {
        }

        public override void Render(RenderContext rc, long frame) {
            base.Render(rc, frame);
        }

        protected override Clip NewInstance() {
            return new MpegMediaVideoClip();
        }

        public override Vector2? GetSize() {
            return null;
        }
    }
}