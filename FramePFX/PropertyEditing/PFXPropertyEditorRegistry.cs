using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.Timelines.Effects.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.PropertyEditing.Editor.Editor;
using FramePFX.PropertyEditing.Editor.Effects;

namespace FramePFX.PropertyEditing {
    public class PFXPropertyEditorRegistry : PropertyEditorRegistry {
        public static PFXPropertyEditorRegistry Instance { get; } = new PFXPropertyEditorRegistry();

        public PropertyGroupViewModel ClipInfo { get; }

        public PropertyGroupViewModel EffectInfo { get; }

        public PropertyGroupViewModel ResourceInfo { get; }

        private PFXPropertyEditorRegistry() {
            {
                this.ClipInfo = this.CreateRootGroup(typeof(ClipViewModel), "Clip Info");
                this.ClipInfo.AddPropertyEditor("ClipDataEditor", new ClipDataEditorViewModel());
                this.ClipInfo.AddPropertyEditor("VideoClipDataEditor_Single", new VideoClipDataSingleEditorViewModel());
                this.ClipInfo.AddPropertyEditor("VideoClipDataEditor_Multi", new VideoClipDataMultipleEditorViewModel());
            }
            {
                this.EffectInfo = this.CreateRootGroup(typeof(BaseEffectViewModel), "Effects Info");
                {
                    PropertyGroupViewModel motion = this.EffectInfo.CreateSubGroup(typeof(MotionEffectViewModel), "Motion");
                    motion.AddPropertyEditor("MotionEffect_Single", new MotionEffectDataSingleEditorViewModel());
                    motion.AddPropertyEditor("MotionEffect_Multi", new MotionEffectDataMultiEditorViewModel());
                }
            }
            {
                this.ResourceInfo = this.CreateRootGroup(typeof(BaseResourceObjectViewModel), "Resource Info");
            }
        }
    }
}