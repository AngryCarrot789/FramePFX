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
using FramePFX.Editors.Controls.Dragger;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation {
    public abstract class BaseNumberParameterPropEditorControl : BaseParameterPropertyEditorControl {
        protected NumberDragger dragger;
        protected bool IsUpdatingControl;

        protected BaseNumberParameterPropEditorControl() {
        }

        static BaseNumberParameterPropEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseNumberParameterPropEditorControl), new FrameworkPropertyMetadata(typeof(BaseNumberParameterPropEditorControl)));

        protected abstract void UpdateControlValue();

        protected abstract void UpdateModelValue();

        private void OnModelValueChanged() {
            if (this.SlotModel != null) {
                this.IsUpdatingControl = true;
                try {
                    this.UpdateControlValue();
                }
                finally {
                    this.IsUpdatingControl = false;
                }
            }
        }

        private void OnControlValueChanged() {
            if (!this.IsUpdatingControl && this.SlotModel != null) {
                this.UpdateModelValue();
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.dragger = this.GetTemplateChild<NumberDragger>("PART_DraggerX");
            this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        }

        protected override void OnConnected() {
            base.OnConnected();
            ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel;
            slot.ValueChanged += this.OnSlotValueChanged;
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel;
            slot.ValueChanged -= this.OnSlotValueChanged;
        }

        private void OnSlotValueChanged(ParameterPropertyEditorSlot slot) {
            this.OnModelValueChanged();
        }
    }
}