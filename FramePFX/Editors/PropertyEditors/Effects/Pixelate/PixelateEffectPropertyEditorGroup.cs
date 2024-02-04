using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.Editors.PropertyEditors.Effects.Pixelate {
    public class PixelateEffectPropertyEditorGroup : EffectPropertyEditorGroup {
        public new PixelateEffect Effect => (PixelateEffect) base.Effect;

        public PixelateEffectPropertyEditorGroup() : base(typeof(PixelateEffect)) {
            this.DisplayName = "Pixelate";
            this.AddItem(new DataParameterDoublePropertyEditorSlot(PixelateEffect.BlockSizeParameter, typeof(PixelateEffect), "Block Size", DragStepProfile.InfPixelRange));
        }
    }
}