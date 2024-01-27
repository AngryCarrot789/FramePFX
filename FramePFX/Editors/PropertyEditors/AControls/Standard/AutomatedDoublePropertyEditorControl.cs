using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Controls.Dragger;
using FramePFX.Editors.PropertyEditors.Standard;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.Controls;

namespace FramePFX.Editors.PropertyEditors.AControls.Standard {
    public class AutomatedDoublePropertyEditorControl : BasePropEditControlContent {
        public AutomatedDoublePropertyEditorSlot SlotModel => (AutomatedDoublePropertyEditorSlot) base.SlotControl.Model;

        private NumberDragger dragger;
        private TextBlock displayName;
        private KeyFrameToolsControl keyFrameTools;
        private readonly EventInfo eventInfo;
        private readonly Delegate handlerInternal;
        private bool IsUpdatingControl;
        private IAutomatable singleHandler;

        public AutomatedDoublePropertyEditorControl() {
            this.eventInfo = typeof(AutomatedDoublePropertyEditorSlot).GetEvent(nameof(AutomatedDoublePropertyEditorSlot.ValueChanged), BindingFlags.Public | BindingFlags.Instance);
            if (this.eventInfo == null)
                throw new Exception("Could not find event by name: " + nameof(AutomatedDoublePropertyEditorSlot) + "." + nameof(AutomatedDoublePropertyEditorSlot.ValueChanged));
            this.handlerInternal = BinderUtils.CreateDelegateToInvokeActionFromEvent(this.eventInfo.EventHandlerType, this.OnModelValueChanged);
        }

        static AutomatedDoublePropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(AutomatedDoublePropertyEditorControl), new FrameworkPropertyMetadata(typeof(AutomatedDoublePropertyEditorControl)));

        private void OnModelValueChanged() {
            AutomatedDoublePropertyEditorSlot slot = this.SlotModel;
            if (slot == null) {
                return;
            }

            this.IsUpdatingControl = true;
            try {
                this.dragger.Value = slot.Value;
                // update control
            }
            finally {
                this.IsUpdatingControl = false;
            }
        }

        private void OnControlValueChanged() {
            if (this.IsUpdatingControl) {
                return;
            }

            AutomatedDoublePropertyEditorSlot slot = this.SlotModel;
            if (slot == null) {
                return;
            }

            slot.Value = this.dragger.Value;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.dragger = this.GetTemplateChild<NumberDragger>("PART_DraggerX");
            this.displayName = this.GetTemplateChild<TextBlock>("PART_DisplayName");
            this.keyFrameTools = this.GetTemplateChild<KeyFrameToolsControl>("PART_KeyFrameTools");
            this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        }

        private void OnHandlerListChanged(bool connect) {
            AutomatedDoublePropertyEditorSlot slot = this.SlotModel;
            if (connect && slot != null && slot.Handlers.Count == 1) {
                this.singleHandler = (IAutomatable) slot.Handlers[0];
                this.keyFrameTools.Visibility = Visibility.Visible;
                this.keyFrameTools.AutomationSequence = this.singleHandler.AutomationData[slot.Parameter];
            }
            else {
                this.keyFrameTools.Visibility = Visibility.Collapsed;
                this.keyFrameTools.AutomationSequence = null;
            }
        }

        private void OnHandlersChanged(PropertyEditorSlot sender) {
            this.OnHandlerListChanged(true);
        }

        protected override void OnConnected() {
            AutomatedDoublePropertyEditorSlot slot = this.SlotModel;
            this.eventInfo.AddEventHandler(slot, this.handlerInternal);
            slot.HandlersLoaded += this.OnHandlersChanged;
            slot.HandlersCleared += this.OnHandlersChanged;
            this.OnHandlerListChanged(true);
            this.displayName.Text = slot.DisplayName;

            ParameterDescriptorDouble desc = slot.Parameter.Descriptor;
            this.displayName.Text = slot.DisplayName;
            this.dragger.Minimum = desc.Minimum;
            this.dragger.Maximum = desc.Maximum;

            DragStepProfile profile = slot.StepProfile;
            this.dragger.TinyChange = profile.TinyStep;
            this.dragger.SmallChange = profile.SmallStep;
            this.dragger.LargeChange = profile.NormalStep;
            this.dragger.MassiveChange = profile.LargeStep;
        }

        protected override void OnDisconnected() {
            AutomatedDoublePropertyEditorSlot slot = this.SlotModel;
            this.eventInfo.RemoveEventHandler(slot, this.handlerInternal);
            slot.HandlersLoaded += this.OnHandlersChanged;
            slot.HandlersCleared += this.OnHandlersChanged;
            this.OnHandlerListChanged(false);
        }
    }
}