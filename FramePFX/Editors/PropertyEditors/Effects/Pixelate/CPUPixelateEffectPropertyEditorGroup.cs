using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.Editors.PropertyEditors.Effects.Pixelate {
    public class CPUPixelateEffectPropertyEditorGroup : EffectPropertyEditorGroup {
        public new CPUPixelateEffect Effect => (CPUPixelateEffect) base.Effect;

        public CPUPixelateEffectPropertyEditorGroup() : base(typeof(CPUPixelateEffect)) {
            this.DisplayName = "CPU Pixelate";
            this.AddItem(new ParameterLongPropertyEditorSlot(CPUPixelateEffect.BlockSizeParameter, typeof(CPUPixelateEffect), "Block Size", DragStepProfile.InfPixelRange));
        }
    }
}