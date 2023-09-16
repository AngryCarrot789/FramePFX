using System.Collections.Generic;
using System.Linq;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Effects;
using FramePFX.Editor.ViewModels.Timelines.Effects.Video;
using FramePFX.PropertyEditing.Editor.Editor.Clips;
using FramePFX.PropertyEditing.Editor.Editor.Effects;

namespace FramePFX.PropertyEditing {
    public class PFXPropertyEditorRegistry : PropertyEditorRegistry {
        public static PFXPropertyEditorRegistry Instance { get; } = new PFXPropertyEditorRegistry();

        public PropertyGroupViewModel ClipInfo { get; }

        public PropertyGroupViewModel EffectInfo { get; }

        public PropertyGroupViewModel ResourceInfo { get; }

        private PFXPropertyEditorRegistry() {
            this.ClipInfo = this.CreateRootGroup(typeof(ClipViewModel), "Clip Info");
            this.ClipInfo.AddPropertyEditor("ClipDataEditor", new ClipDataEditorViewModel());
            this.ClipInfo.AddPropertyEditor("VideoClipDataEditor_Single", new VideoClipDataSingleEditorViewModel());
            this.ClipInfo.AddPropertyEditor("VideoClipDataEditor_Multi", new VideoClipDataMultipleEditorViewModel());

            this.ResourceInfo = this.CreateRootGroup(typeof(BaseResourceObjectViewModel), "Resource Info");

            this.EffectInfo = this.CreateRootGroup(typeof(BaseEffectViewModel), "Effects");
            PropertyGroupViewModel motion = this.EffectInfo.CreateSubGroup(typeof(MotionEffectViewModel), "Motion");
            motion.AddPropertyEditor("MotionEffect_Single", new MotionEffectDataSingleEditorViewModel());
            motion.AddPropertyEditor("MotionEffect_Multi", new MotionEffectDataMultiEditorViewModel());
        }

        public void OnClipsSelected(List<ClipViewModel> clips) {
            List<BaseEffectViewModel> effects = clips.SelectMany(clip => clip.Effects).ToList();
            this.Root.ClearHierarchyState();
            this.ClipInfo.SetupHierarchyState(clips);
            this.EffectInfo.SetupHierarchyState(effects);
        }
    }
}