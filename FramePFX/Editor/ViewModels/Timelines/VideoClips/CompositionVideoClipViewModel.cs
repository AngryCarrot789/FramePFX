using System;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips
{
    public class CompositionVideoClipViewModel : VideoClipViewModel
    {
        public new CompositionVideoClip Model => (CompositionVideoClip) ((ClipViewModel) this).Model;

        public CompositionVideoClipViewModel(CompositionVideoClip model) : base(model)
        {
        }

        public bool TryGetResource(out ResourceCompositionViewModel resource)
        {
            if (this.Model.ResourceCompositionKey.TryGetResource(out ResourceComposition composition))
            {
                resource = (ResourceCompositionViewModel) composition.ViewModel ?? throw new Exception("Invalid view model");
                return true;
            }

            resource = null;
            return false;
        }
    }
}