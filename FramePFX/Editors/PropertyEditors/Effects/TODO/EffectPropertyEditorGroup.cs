using System;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.PropertyEditors.Effects.TODO {
    public class EffectPropertyEditorGroup : SimplePropertyEditorGroup {
        public BaseEffect Effect { get; }

        public EffectPropertyEditorGroup(Type applicableType, BaseEffect effect) : base(applicableType) {
            this.Effect = effect;
        }
    }
}