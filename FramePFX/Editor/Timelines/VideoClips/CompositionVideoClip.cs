using System.Threading;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class CompositionVideoClip : VideoClip, IResourceClip<ResourceCompositionSeq> {
        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;
        public ResourceHelper<ResourceCompositionSeq> ResourceHelper { get; }

        public CompositionVideoClip() {
            this.ResourceHelper = new ResourceHelper<ResourceCompositionSeq>(this);
        }

        public override bool BeginRender(long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceCompositionSeq resource) || resource.Timeline.Project == null) {
                return false;
            }

            return resource.Timeline.BeginCompositeRender(frame, CancellationToken.None);
        }

        public override Task EndRender(RenderContext rc, long frame) {
            Project project;
            if (!this.ResourceHelper.TryGetResource(out ResourceCompositionSeq resource) || (project = resource.Timeline.Project) == null) {
                return Task.CompletedTask;
            }

            CancellationTokenSource src = new CancellationTokenSource(project.IsExporting ? -1 : 3000);
            return resource.Timeline.EndCompositeRenderAsync(rc, frame, src.Token);
        }

        protected override Clip NewInstance() {
            return new CompositionVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
            CompositionVideoClip clip = (CompositionVideoClip) clone;
            this.ResourceHelper.LoadDataIntoClone(clip.ResourceHelper);
        }
    }
}