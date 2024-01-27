using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.PropertyEditors.AControls.Clips;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing.Controls;

namespace FramePFX.Editors.PropertyEditors.AControls.Effects.Motion {
    public class MotionEffectPropertyEditorGroup : EffectPropertyEditorGroup {
        public new MotionEffect Effect => (MotionEffect) base.Effect;

        public MotionEffectPropertyEditorGroup() : base(typeof(MotionEffect)) {

        }
    }
}