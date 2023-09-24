using System.Threading;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class CompositionVideoClip : VideoClip, IResourceClip<ResourceCompositionSeq> {
        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;
        public ResourceHelper<ResourceCompositionSeq> ResourceHelper { get; }

        private CancellationTokenSource tokenSource;

        public CompositionVideoClip() {
            this.ResourceHelper = new ResourceHelper<ResourceCompositionSeq>(this);
        }

        public override bool OnBeginRender(long frame) {
            Project project;
            if (!this.ResourceHelper.TryGetResource(out ResourceCompositionSeq resource) || (project = resource.Timeline.Project) == null) {
                return false;
            }

            this.tokenSource = new CancellationTokenSource(project.IsExporting ? -1 : 3000);
            frame = this.GetRelativeFrame(frame);
            return resource.Timeline.BeginCompositeRender(frame, CancellationToken.None);
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceCompositionSeq resource)) {
                return Task.CompletedTask;
            }

            return resource.Timeline.EndCompositeRenderAsync(rc, frame, this.tokenSource.Token);
        }

        public override void OnRenderCompleted(long frame, bool isCancelled) {
            base.OnRenderCompleted(frame, isCancelled);
            this.tokenSource?.Dispose();
            this.tokenSource = null;
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