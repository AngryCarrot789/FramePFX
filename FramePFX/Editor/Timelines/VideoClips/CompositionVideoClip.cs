using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class CompositionVideoClip : VideoClip, IResourceClip<ResourceComposition> {
        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;
        public ResourceHelper<ResourceComposition> ResourceHelper { get; }

        private CancellationTokenSource tokenSource;

        private long relativeRenderFrame;
        private long relativePeriodicFrame;

        public CompositionVideoClip() {
            this.ResourceHelper = new ResourceHelper<ResourceComposition>(this);
        }

        public override Vector2? GetSize(RenderContext rc) {
            return rc.FrameSize;
        }

        public override bool OnBeginRender(long frame) {
            Project project;
            if (!this.ResourceHelper.TryGetResource(out ResourceComposition resource) || (project = resource.Timeline.Project) == null) {
                return false;
            }

            this.tokenSource = new CancellationTokenSource(project.IsExporting ? -1 : 3000);
            this.relativeRenderFrame = this.GetRelativeFrame(frame);
            this.relativePeriodicFrame = this.relativeRenderFrame % resource.Timeline.LargestFrameInUse;
            return resource.Timeline.BeginCompositeRender(this.relativePeriodicFrame, CancellationToken.None);
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceComposition resource)) {
                return Task.CompletedTask;
            }

            return resource.Timeline.EndCompositeRenderAsync(rc, this.relativePeriodicFrame, this.tokenSource.Token);
        }

        public override void OnRenderCompleted(long frame, bool isCancelled) {
            base.OnRenderCompleted(frame, isCancelled);
            this.tokenSource?.Dispose();
            this.tokenSource = null;
            this.relativeRenderFrame = 0;
            this.relativePeriodicFrame = 0;
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