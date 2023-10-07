using System.Numerics;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class MpegMediaVideoClip : VideoClip, IResourceHolder {
        public int StreamIndex { get; set; }

        public ResourceHelper ResourceHelper { get; }

        public IResourcePathKey<ResourceMpegMedia> MpegMediaKey { get; }

        public MpegMediaVideoClip() {
            this.ResourceHelper = new ResourceHelper(this);
            this.MpegMediaKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceMpegMedia>();
        }

        protected override Clip NewInstanceForClone() {
            return new MpegMediaVideoClip();
        }

        public override Vector2? GetSize(RenderContext rc) {
            return null;
        }
    }
}