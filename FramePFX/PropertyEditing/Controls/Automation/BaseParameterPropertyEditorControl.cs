using System.Windows;
using FramePFX.Editors.Automation;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation {
    public abstract class BaseParameterPropertyEditorControl : BasePropEditControlContent {
        protected IAutomatable singleHandler;

        public ParameterPropertyEditorSlot SlotModel => (ParameterPropertyEditorSlot) base.SlotControl.Model;

        protected BaseParameterPropertyEditorControl() {
        }

        static BaseParameterPropertyEditorControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseParameterPropertyEditorControl), new FrameworkPropertyMetadata(typeof(BaseParameterPropertyEditorControl)));
        }

        protected override void OnConnected() {

        }

        protected override void OnDisconnected() {

        }
    }
}