using System.Numerics;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class MpegMediaVideoClip : VideoClip {
        public int StreamIndex { get; set; }

        public IResourcePathKey<ResourceMpegMedia> MpegMediaKey { get; }

        public MpegMediaVideoClip() {
            this.MpegMediaKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceMpegMedia>();
        }

        protected override Clip NewInstanceForClone() {
            return new MpegMediaVideoClip();
        }

        public override Vector2? GetFrameSize() {
            return null;
        }
    }
}