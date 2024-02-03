using System.Windows;
using FramePFX.Editors.Controls.Dragger;

namespace FramePFX.PropertyEditing.Controls.DataTransfer {
    public abstract class BaseNumberDataParamPropEditorControl : BaseDataParameterPropertyEditorControl {
        protected NumberDragger dragger;

        protected BaseNumberDataParamPropEditorControl() {

        }

        static BaseNumberDataParamPropEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseNumberDataParamPropEditorControl), new FrameworkPropertyMetadata(typeof(BaseNumberDataParamPropEditorControl)));

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.dragger = this.GetTemplateChild<NumberDragger>("PART_DraggerX");
            this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        }

        protected override void OnCanEditValueChanged(bool canEdit) {
            this.dragger.IsEnabled = canEdit;
        }
    }
}