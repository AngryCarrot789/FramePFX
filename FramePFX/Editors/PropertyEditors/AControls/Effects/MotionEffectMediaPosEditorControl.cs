using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Controls.Dragger;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.Editors.PropertyEditors.Effects.Motion;
using FramePFX.PropertyEditing.Controls;

namespace FramePFX.Editors.PropertyEditors.AControls.Effects {
    public class MotionEffectMediaPosEditorControl : BasePropEditControlContent {
        public MotionEffectMediaPosPropertyEditorSlot SlotModel => (MotionEffectMediaPosPropertyEditorSlot) base.Slot.Model;

        private NumberDragger sliderX;
        private NumberDragger sliderY;

        private readonly GetSetAutoPropertyBinder<MotionEffectMediaPosPropertyEditorSlot> posXBinder = new GetSetAutoPropertyBinder<MotionEffectMediaPosPropertyEditorSlot>(RangeBase.ValueProperty, nameof(MotionEffectMediaPosPropertyEditorSlot.MediaPositionXChanged), binder => (double) binder.Model.PosX, (binder, v) => binder.Model.PosX = (float) (double) v);
        private readonly GetSetAutoPropertyBinder<MotionEffectMediaPosPropertyEditorSlot> posYBinder = new GetSetAutoPropertyBinder<MotionEffectMediaPosPropertyEditorSlot>(RangeBase.ValueProperty, nameof(MotionEffectMediaPosPropertyEditorSlot.MediaPositionYChanged), binder => (double) binder.Model.PosY, (binder, v) => binder.Model.PosY = (float) (double) v);

        public MotionEffectMediaPosEditorControl() {
        }

        static MotionEffectMediaPosEditorControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MotionEffectMediaPosEditorControl), new FrameworkPropertyMetadata(typeof(MotionEffectMediaPosEditorControl)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.sliderX = this.GetTemplateChild<NumberDragger>("PART_DraggerX");
            this.sliderY = this.GetTemplateChild<NumberDragger>("PART_DraggerY");
            this.sliderX.ValueChanged += (sender, args) => this.posXBinder.OnControlValueChanged();
            this.sliderY.ValueChanged += (sender, args) => this.posYBinder.OnControlValueChanged();
        }

        protected override void OnConnected() {
            this.posXBinder.Attach(this.sliderX, this.SlotModel);
            this.posYBinder.Attach(this.sliderY, this.SlotModel);
        }

        protected override void OnDisconnected() {
            this.posXBinder.Detatch();
            this.posYBinder.Detatch();
        }
    }
}