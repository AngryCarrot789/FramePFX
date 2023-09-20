using System.Collections.Generic;
using System.Linq;
using FramePFX.Editor.PropertyEditors.Clips;
using FramePFX.Editor.PropertyEditors.Clips.Text;
using FramePFX.Editor.PropertyEditors.Effects;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Effects;
using FramePFX.Editor.ViewModels.Timelines.Effects.Video;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;

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

            {
                FixedPropertyGroupViewModel group = this.ClipInfo.CreateFixedSubGroup(typeof(TextClipViewModel), "Text Info");
                group.AddPropertyEditor("TextEditor", new TextClipDataEditorViewModel());
            }

            this.EffectInfo = this.ClipInfo.CreateDynamicSubGroup(typeof(BaseEffectViewModel), "Effects", isHierarchial:false);
            this.EffectInfo.IsHeaderBold = true;

            this.EffectInfo.RegisterType(typeof(MotionEffectViewModel), "Motion", (single) => {
                EffectPropertyGroupViewModel motion = new EffectPropertyGroupViewModel(typeof(MotionEffectViewModel)) {
                    IsExpanded = true, IsHeaderBold = true
                };

                if (!single.HasValue || single.Value) {
                    motion.AddPropertyEditor("MotionEffect_Single", new MotionEffectDataSingleEditorViewModel());
                }

                if (!single.HasValue || single.Value == false) {
                    motion.AddPropertyEditor("MotionEffect_Multi", new MotionEffectDataMultiEditorViewModel());
                }

                return motion;
            });

            this.Root.AddSeparator();

            this.ResourceInfo = this.Root.CreateFixedSubGroup(typeof(BaseResourceObjectViewModel), "Resource Info");
        }

        public void OnClipSelectionChanged(IReadOnlyList<ClipViewModel> clips) {
            // List<BaseEffectViewModel> effects = clips.SelectMany(clip => clip.Effects).ToList();
            this.ClipInfo.SetupHierarchyState(clips);
            foreach (IPropertyObject obj in this.ClipInfo.PropertyObjects) {
                if (obj is FixedPropertyGroupViewModel group && this.ClipInfo.IsDisconnectedFromHierarchy(group)) {
                    group.SetupHierarchyState(clips);
                }
            }

            this.EffectInfo.SetupHierarchyStateExtended(clips.Select(x => x.Effects).ToList());
            this.Root.CleanSeparators();
        }

        public void OnEffectCollectionChanged() {
            IReadOnlyList<object> clips = this.ClipInfo.Handlers;
            if (clips != null && clips.Count > 0) {
                this.EffectInfo.SetupHierarchyState(clips.Cast<ClipViewModel>().Select(x => x.Effects).ToList());
            }
            else {
                this.EffectInfo.ClearHierarchyState();
            }
        }

        public void OnResourcesSelectionChanged(IReadOnlyList<BaseResourceObjectViewModel> list) {
            this.ResourceInfo.SetupHierarchyState(list);
            this.Root.CleanSeparators();
        }
    }
}