using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing;
using FramePFX.Utils;

namespace FramePFX.Editors.PropertyEditors.Effects.Motion {
    public class MotionEffectPropertyEditorGroup : EffectPropertyEditorGroup {
        public new MotionEffect Effect => (MotionEffect) base.Effect;

        public MotionEffectPropertyEditorGroup() : base(typeof(MotionEffect)) {
            this.DisplayName = "Motion";
            this.AddItem(new MotionEffectMediaPosPropertyEditorSlot());
        }
    }

    public delegate void MotionEffectPropertyEditorEventHandler(MotionEffectMediaPosPropertyEditorSlot sender);

    public class MotionEffectMediaPosPropertyEditorSlot : PropertyEditorSlot {
        private float posX;
        private float posY;

        public MotionEffect Effect => (MotionEffect) this.Handlers[0];

        public float PosX {
            get => this.posX;
            set {
                this.posX = value;
                MotionEffect effect = this.Effect;
                AutomatedControlUtils.SetDefaultKeyFrameOrAddNew(effect, MotionEffect.MediaPositionXParameter, value);
                this.MediaPositionXChanged?.Invoke(this);
            }
        }

        public float PosY {
            get => this.posY;
            set {
                this.posY = value;
                MotionEffect effect = this.Effect;
                AutomatedControlUtils.SetDefaultKeyFrameOrAddNew(effect, MotionEffect.MediaPositionYParameter, value);
                this.MediaPositionYChanged?.Invoke(this);
            }
        }

        public override bool IsSelectable => true;

        public event MotionEffectPropertyEditorEventHandler MediaPositionXChanged;
        public event MotionEffectPropertyEditorEventHandler MediaPositionYChanged;

        public MotionEffectMediaPosPropertyEditorSlot() : base(typeof(MotionEffect)) {

        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            this.Effect.AutomationData[MotionEffect.MediaPositionXParameter].ParameterChanged += this.OnMediaPosChanged;
            this.Effect.AutomationData[MotionEffect.MediaPositionYParameter].ParameterChanged += this.OnMediaPosChanged;
            this.RequeryOpacityFromHandlers();
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            this.Effect.AutomationData[MotionEffect.MediaPositionXParameter].ParameterChanged -= this.OnMediaPosChanged;
            this.Effect.AutomationData[MotionEffect.MediaPositionYParameter].ParameterChanged -= this.OnMediaPosChanged;
        }

        public void RequeryOpacityFromHandlers() {
            this.posX = this.Effect.MediaPositionX;
            this.posY = this.Effect.MediaPositionY;
            this.MediaPositionXChanged?.Invoke(this);
            this.MediaPositionYChanged?.Invoke(this);
        }

        // Event handler only added for single selection
        private void OnMediaPosChanged(AutomationSequence sequence) {
            if (sequence.Parameter.Equals(MotionEffect.MediaPositionXParameter)) {
                this.posX = this.Effect.MediaPositionX;
                this.MediaPositionXChanged?.Invoke(this);
            }
            else {
                this.posY = this.Effect.MediaPositionY;
                this.MediaPositionYChanged?.Invoke(this);
            }
        }
    }
}