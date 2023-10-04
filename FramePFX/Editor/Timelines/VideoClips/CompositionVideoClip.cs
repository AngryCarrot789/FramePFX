using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips
{
    public class CompositionVideoClip : VideoClip, IResourceClip<ResourceComposition>
    {
        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;
        public ResourceHelper<ResourceComposition> ResourceHelper { get; }

        private CancellationTokenSource tokenSource;

        private long relativeRenderFrame;
        private long relativePeriodicFrame;

        public CompositionVideoClip()
        {
            this.ResourceHelper = new ResourceHelper<ResourceComposition>(this);
        }

        public override Vector2? GetSize(RenderContext rc) => rc.FrameSize;

        public override bool OnBeginRender(long frame)
        {
            Project project;
            if (!this.ResourceHelper.TryGetResource(out ResourceComposition resource) || (project = resource.Timeline.Project) == null)
            {
                return false;
            }

            long duration = resource.Timeline.LargestFrameInUse;
            if (duration < 1)
                return false;

            this.tokenSource = new CancellationTokenSource(project.IsExporting ? -1 : 3000);
            this.relativeRenderFrame = this.GetRelativeFrame(frame);
            this.relativePeriodicFrame = this.relativeRenderFrame % duration;
            return resource.Timeline.BeginCompositeRender(this.relativePeriodicFrame, CancellationToken.None);
        }

        public override Task OnEndRender(RenderContext rc, long frame)
        {
            if (!this.ResourceHelper.TryGetResource(out ResourceComposition resource))
            {
                return Task.CompletedTask;
            }

            return resource.Timeline.EndCompositeRenderAsync(rc, this.relativePeriodicFrame, CancellationToken.None);
        }

        public override void OnRenderCompleted(long frame, bool isCancelled)
        {
            base.OnRenderCompleted(frame, isCancelled);
            this.tokenSource?.Dispose();
            this.tokenSource = null;
            this.relativeRenderFrame = 0;
            this.relativePeriodicFrame = 0;
        }

        protected override Clip NewInstanceForClone()
        {
            return new CompositionVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone, ClipCloneFlags flags)
        {
            base.LoadDataIntoClone(clone, flags);
            CompositionVideoClip clip = (CompositionVideoClip) clone;
            this.ResourceHelper.LoadDataIntoClone(clip.ResourceHelper);
        }
    }
}