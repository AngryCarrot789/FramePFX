using System;
using System.Threading.Tasks;
using FramePFX.AdvancedContextService;
using FramePFX.AdvancedContextService.NCSP;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Interactivity;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class CompositionVideoClipViewModel : VideoClipViewModel {
        public new CompositionVideoClip Model => (CompositionVideoClip) ((ClipViewModel) this).Model;

        public CompositionVideoClipViewModel(CompositionVideoClip model) : base(model) {
        }

        static CompositionVideoClipViewModel() {
            DropRegistry.Register<CompositionVideoClipViewModel, ResourceCompositionViewModel>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) => {
                clip.Model.ResourceCompositionKey.SetTargetResourceId(h.UniqueId);
                return Task.CompletedTask;
            });

            IContextRegistration reg = ContextRegistry.Instance.RegisterType(typeof(CompositionVideoClipViewModel));
            reg.AddEntry(new ActionContextEntry(null, "actions.timeline.OpenCompositionObjectsTimeline", "Open timeline"));
        }

        public bool TryGetResource(out ResourceCompositionViewModel resource) {
            if (this.Model.ResourceCompositionKey.TryGetResource(out ResourceComposition composition)) {
                resource = (ResourceCompositionViewModel) composition.ViewModel ?? throw new Exception("Invalid view model");
                return true;
            }

            resource = null;
            return false;
        }
    }
}