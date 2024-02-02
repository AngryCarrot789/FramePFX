using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing.Standard;

namespace FramePFX.Editors.PropertyEditors.Effects.Motion {
    public class MotionEffectPropertyEditorGroup : EffectPropertyEditorGroup {
        public new MotionEffect Effect => (MotionEffect) base.Effect;

        public MotionEffectPropertyEditorGroup() : base(typeof(MotionEffect)) {
            this.DisplayName = "Motion";
            this.AddItem(new ParameterVector2PropertyEditorSlot(MotionEffect.MediaPositionParameter, typeof(MotionEffect), "Pos", DragStepProfile.InfPixelRange));
            this.AddItem(new ParameterVector2PropertyEditorSlot(MotionEffect.MediaScaleParameter, typeof(MotionEffect), "Scale", DragStepProfile.UnitOne));
            this.AddItem(new ParameterVector2PropertyEditorSlot(MotionEffect.MediaScaleOriginParameter, typeof(MotionEffect), "Scale Origin", DragStepProfile.InfPixelRange));
            this.AddItem(new ParameterDoublePropertyEditorSlot(MotionEffect.MediaRotationParameter, typeof(MotionEffect), "Rotation", DragStepProfile.Rotation));
            this.AddItem(new ParameterVector2PropertyEditorSlot(MotionEffect.MediaRotationOriginParameter, typeof(MotionEffect), "Rotation Origin", DragStepProfile.InfPixelRange));
        }
    }
}