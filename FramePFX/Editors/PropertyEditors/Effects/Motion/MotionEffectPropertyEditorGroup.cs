using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing.Standard;

namespace FramePFX.Editors.PropertyEditors.Effects.Motion {
    public class MotionEffectPropertyEditorGroup : EffectPropertyEditorGroup {
        public new MotionEffect Effect => (MotionEffect) base.Effect;

        public MotionEffectPropertyEditorGroup() : base(typeof(MotionEffect)) {
            this.DisplayName = "Motion";
            this.AddItem(new ParameterFloatPropertyEditorSlot(MotionEffect.MediaPositionXParameter, typeof(MotionEffect), "Pos X", DragStepProfile.HugeRange));
            this.AddItem(new ParameterFloatPropertyEditorSlot(MotionEffect.MediaPositionYParameter, typeof(MotionEffect), "Pos Y", DragStepProfile.HugeRange));
            this.AddItem(new ParameterFloatPropertyEditorSlot(MotionEffect.MediaScaleXParameter, typeof(MotionEffect), "Scale X", DragStepProfile.UnitOne));
            this.AddItem(new ParameterFloatPropertyEditorSlot(MotionEffect.MediaScaleYParameter, typeof(MotionEffect), "Scale Y", DragStepProfile.UnitOne));
            this.AddItem(new ParameterFloatPropertyEditorSlot(MotionEffect.MediaScaleOriginXParameter, typeof(MotionEffect), "Scale Origin X", DragStepProfile.HugeRange));
            this.AddItem(new ParameterFloatPropertyEditorSlot(MotionEffect.MediaScaleOriginYParameter, typeof(MotionEffect), "Scale Origin Y", DragStepProfile.HugeRange));
            this.AddItem(new ParameterDoublePropertyEditorSlot(MotionEffect.MediaRotationParameter, typeof(MotionEffect), "Rotation", DragStepProfile.Rotation));
            this.AddItem(new ParameterFloatPropertyEditorSlot(MotionEffect.MediaRotationOriginXParameter, typeof(MotionEffect), "Rotation Origin Y", DragStepProfile.HugeRange));
            this.AddItem(new ParameterFloatPropertyEditorSlot(MotionEffect.MediaRotationOriginYParameter, typeof(MotionEffect), "Rotation Origin Y", DragStepProfile.HugeRange));
        }
    }
}