using FramePFX.Editors.PropertyEditors.Standard;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Utils;

namespace FramePFX.Editors.PropertyEditors.Effects.Motion {
    public class MotionEffectPropertyEditorGroup : EffectPropertyEditorGroup {
        public new MotionEffect Effect => (MotionEffect) base.Effect;

        public MotionEffectPropertyEditorGroup() : base(typeof(MotionEffect)) {
            this.DisplayName = "Motion";
            this.AddItem(new AutomatedFloatPropertyEditorSlot(MotionEffect.MediaPositionXParameter, typeof(MotionEffect), "Pos X", DragStepProfile.HugeRange));
            this.AddItem(new AutomatedFloatPropertyEditorSlot(MotionEffect.MediaPositionYParameter, typeof(MotionEffect), "Pos Y", DragStepProfile.HugeRange));
            this.AddItem(new AutomatedFloatPropertyEditorSlot(MotionEffect.MediaScaleXParameter, typeof(MotionEffect), "Scale X", DragStepProfile.UnitOne));
            this.AddItem(new AutomatedFloatPropertyEditorSlot(MotionEffect.MediaScaleYParameter, typeof(MotionEffect), "Scale Y", DragStepProfile.UnitOne));
            this.AddItem(new AutomatedFloatPropertyEditorSlot(MotionEffect.MediaScaleOriginXParameter, typeof(MotionEffect), "Scale Origin X", DragStepProfile.HugeRange));
            this.AddItem(new AutomatedFloatPropertyEditorSlot(MotionEffect.MediaScaleOriginYParameter, typeof(MotionEffect), "Scale Origin Y", DragStepProfile.HugeRange));
            this.AddItem(new AutomatedDoublePropertyEditorSlot(MotionEffect.MediaRotationParameter, typeof(MotionEffect), "Rotation", DragStepProfile.Rotation));
            this.AddItem(new AutomatedFloatPropertyEditorSlot(MotionEffect.MediaRotationOriginXParameter, typeof(MotionEffect), "Rotation Origin Y", DragStepProfile.HugeRange));
            this.AddItem(new AutomatedFloatPropertyEditorSlot(MotionEffect.MediaRotationOriginYParameter, typeof(MotionEffect), "Rotation Origin Y", DragStepProfile.HugeRange));
        }
    }
}