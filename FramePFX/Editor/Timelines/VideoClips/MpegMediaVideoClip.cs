using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class MpegMediaVideoClip : BaseResourceVideoClip<ResourceMpegMedia> {
        public int StreamIndex { get; set; }

        public MpegMediaVideoClip() {
        }

        protected override Clip NewInstance() {
            return new MpegMediaVideoClip();
        }

        public override Vector2? GetSize() {
            return null;
        }
    }
}