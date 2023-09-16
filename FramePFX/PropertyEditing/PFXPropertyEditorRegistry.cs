using System;
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

        public FixedPropertyGroupViewModel ClipInfo { get; }

        public DynamicPropertyGroupViewModel EffectInfo { get; }

        public FixedPropertyGroupViewModel ResourceInfo { get; }

        private PFXPropertyEditorRegistry() {
            this.ClipInfo = this.CreateRootGroup(typeof(ClipViewModel), "Clip Info");
            this.ClipInfo.AddPropertyEditor("ClipDataEditor", new ClipDataEditorViewModel());
            this.ClipInfo.AddPropertyEditor("VideoClipDataEditor_Single", new VideoClipDataSingleEditorViewModel());
            this.ClipInfo.AddPropertyEditor("VideoClipDataEditor_Multi", new VideoClipDataMultipleEditorViewModel());

            this.ResourceInfo = this.Root.CreateFixedSubGroup(typeof(BaseResourceObjectViewModel), "Resource Info");

            this.EffectInfo = this.Root.CreateDynamicSubGroup(typeof(BaseEffectViewModel), "Effects");
            this.EffectInfo.RegisterType(typeof(MotionEffectViewModel), "Motion", (single) => {
                FixedPropertyGroupViewModel motion = new FixedPropertyGroupViewModel(typeof(MotionEffectViewModel)) {
                    IsExpanded = true
                };

                if (!single.HasValue || single.Value) {
                    motion.AddPropertyEditor("MotionEffect_Single", new MotionEffectDataSingleEditorViewModel());
                }

                if (!single.HasValue || single.Value == false) {
                    motion.AddPropertyEditor("MotionEffect_Multi", new MotionEffectDataMultiEditorViewModel());
                }

                return motion;
            });
        }

        public void OnClipsSelected(List<ClipViewModel> clips) {
            // List<BaseEffectViewModel> effects = clips.SelectMany(clip => clip.Effects).ToList();
            this.Root.ClearHierarchyState();
            this.ClipInfo.SetupHierarchyState(clips);
            this.EffectInfo.SetupHierarchyStateExtended(clips.Select(x => x.Effects).ToList());
        }
    }
}