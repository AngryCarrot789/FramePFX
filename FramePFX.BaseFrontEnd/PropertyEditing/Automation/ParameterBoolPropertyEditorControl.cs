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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.BaseFrontEnd.PropertyEditing.Automation;

public class ParameterBoolPropertyEditorControl : BaseParameterPropertyEditorControl {
    private CheckBox valueCheckBox;
    protected bool IsUpdatingControl;

    public new ParameterBoolPropertyEditorSlot SlotModel => (ParameterBoolPropertyEditorSlot) base.SlotControl.Model;

    public ParameterBoolPropertyEditorControl() {
    }

    protected void UpdateControlValue() {
        this.valueCheckBox.IsChecked = this.SlotModel.Value;
    }

    protected void UpdateModelValue() {
        this.SlotModel.Value = this.valueCheckBox.IsChecked ?? false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.valueCheckBox = e.NameScope.GetTemplateChild<CheckBox>("PART_ValueCheckBox");
        this.valueCheckBox.IsCheckedChanged += (s, ex) => this.OnControlValueChanged();
    }

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

    protected override void OnConnected() {
        base.OnConnected();
        this.SlotModel.ValueChanged += this.SlotOnValueChanged;
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.SlotModel.ValueChanged += this.SlotOnValueChanged;
    }

    private void SlotOnValueChanged(ParameterPropertyEditorSlot slot) {
        this.OnModelValueChanged();
    }
}