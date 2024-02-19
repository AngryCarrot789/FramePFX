// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Controls.DataTransfer {
    public abstract class BaseDataParameterPropertyEditorControl : BasePropEditControlContent {
        public static readonly DependencyProperty IsCheckBoxToggleableProperty = DependencyProperty.Register("IsCheckBoxToggleable", typeof(bool), typeof(BaseDataParameterPropertyEditorControl), new PropertyMetadata(BoolBox.False));

        protected ITransferableData singleHandler;
        protected CheckBox displayNameCheckBox;
        protected bool IsUpdatingPrimaryControl;
        protected bool isUpdatingCheckBoxControl;

        public bool IsCheckBoxToggleable {
            get => (bool) this.GetValue(IsCheckBoxToggleableProperty);
            set => this.SetValue(IsCheckBoxToggleableProperty, value.Box());
        }

        public new DataParameterPropertyEditorSlot SlotModel => (DataParameterPropertyEditorSlot) base.SlotModel;

        protected BaseDataParameterPropertyEditorControl() {
        }

        static BaseDataParameterPropertyEditorControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseDataParameterPropertyEditorControl), new FrameworkPropertyMetadata(typeof(BaseDataParameterPropertyEditorControl)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.displayNameCheckBox = this.GetTemplateChild<CheckBox>("PART_DisplayNameCheckBox");
            this.displayNameCheckBox.Checked += this.OnDisplayNameCheckChanged;
            this.displayNameCheckBox.Unchecked += this.OnDisplayNameCheckChanged;
        }

        protected virtual void OnDisplayNameCheckChanged(object sender, RoutedEventArgs e) {
            if (!this.isUpdatingCheckBoxControl && this.SlotModel != null) {
                bool value = this.displayNameCheckBox.IsChecked ?? false;
                this.SlotModel.IsEditable = this.SlotModel.InvertIsEditableForParameter ? !value : value;
            }
        }

        protected abstract void UpdateControlValue();

        protected abstract void UpdateModelValue();

        protected void OnModelValueChanged() {
            if (this.SlotModel != null) {
                this.IsUpdatingPrimaryControl = true;
                try {
                    this.UpdateControlValue();
                }
                finally {
                    this.IsUpdatingPrimaryControl = false;
                }
            }
        }

        protected void OnControlValueChanged() {
            if (!this.IsUpdatingPrimaryControl && this.SlotModel != null) {
                this.UpdateModelValue();
            }
        }

        protected override void OnConnected() {
            DataParameterPropertyEditorSlot slot = this.SlotModel;
            slot.HandlersLoaded += this.OnHandlersChanged;
            slot.HandlersCleared += this.OnHandlersChanged;
            slot.DisplayNameChanged += this.OnSlotDisplayNameChanged;
            slot.ValueChanged += this.OnSlotValueChanged;
            slot.IsEditableChanged += this.SlotOnIsEditableChanged;
            this.displayNameCheckBox.Content = slot.DisplayName;
            this.IsCheckBoxToggleable = slot.IsEditableParameter != null;
            this.OnHandlerListChanged(true);
        }

        protected override void OnDisconnected() {
            DataParameterPropertyEditorSlot slot = this.SlotModel;
            slot.DisplayNameChanged -= this.OnSlotDisplayNameChanged;
            slot.HandlersLoaded -= this.OnHandlersChanged;
            slot.HandlersCleared -= this.OnHandlersChanged;
            slot.ValueChanged -= this.OnSlotValueChanged;
            this.SlotModel.IsEditableChanged -= this.SlotOnIsEditableChanged;
            this.OnHandlerListChanged(false);
        }

        private void SlotOnIsEditableChanged(DataParameterPropertyEditorSlot slot) {
            this.UpdateCanEdit();
        }

        private void UpdateCanEdit() {
            this.isUpdatingCheckBoxControl = true;
            DataParameterPropertyEditorSlot slot = this.SlotModel;
            bool value = this.SlotModel.IsEditable;
            value = slot.InvertIsEditableForParameter ? !value : value;
            this.displayNameCheckBox.IsChecked = value;
            this.isUpdatingCheckBoxControl = false;
            this.OnCanEditValueChanged(value);
        }

        protected abstract void OnCanEditValueChanged(bool canEdit);

        private void OnSlotValueChanged(DataParameterPropertyEditorSlot slot) {
            this.OnModelValueChanged();
        }

        private void OnSlotDisplayNameChanged(DataParameterPropertyEditorSlot slot) {
            if (this.displayNameCheckBox != null)
                this.displayNameCheckBox.Content = slot.DisplayName;
        }

        private void OnHandlerListChanged(bool connect) {
            DataParameterPropertyEditorSlot slot = this.SlotModel;
            if (connect) {
                if (slot != null && slot.Handlers.Count == 1) {
                    this.singleHandler = (ITransferableData) slot.Handlers[0];
                }

                this.OnHandlersLoadedOverride(true);
            }
            else {
                this.OnHandlersLoadedOverride(false);
                this.singleHandler = null;
            }
        }

        protected virtual void OnHandlersLoadedOverride(bool isLoaded) {
            this.UpdateCanEdit();
        }

        private void OnHandlersChanged(PropertyEditorSlot sender) {
            this.OnHandlerListChanged(sender.IsCurrentlyApplicable);
        }
    }
}