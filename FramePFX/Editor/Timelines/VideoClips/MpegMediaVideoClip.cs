using System.Numerics;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class MpegMediaVideoClip : VideoClip, IResourceClip<ResourceMpegMedia> {
        public int StreamIndex { get; set; }

        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;
        public ResourceHelper<ResourceMpegMedia> ResourceHelper { get; }

        public MpegMediaVideoClip() {
            this.ResourceHelper = new ResourceHelper<ResourceMpegMedia>(this);
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
            this.ResourceHelper.LoadDataIntoClone(((MpegMediaVideoClip) clone).ResourceHelper);
        }

        protected override Clip NewInstance() {
            return new MpegMediaVideoClip();
        }

        public override Vector2? GetSize(RenderContext renderContext) {
            return null;
        }
    }
}