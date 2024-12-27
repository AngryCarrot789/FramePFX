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
using Avalonia.Interactivity;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer;

public class DataParameterBoolPropertyEditorSlotControl : BaseDataParameterPropertyEditorSlotControl {
    protected CheckBox checkBox;

    public new DataParameterBoolPropertyEditorSlot SlotModel => (DataParameterBoolPropertyEditorSlot) base.SlotControl.Model;

    public DataParameterBoolPropertyEditorSlotControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.checkBox = e.NameScope.GetTemplateChild<CheckBox>("PART_CheckBox");
        this.checkBox.IsCheckedChanged += this.CheckBoxOnChecked;
    }

    private void CheckBoxOnChecked(object? sender, RoutedEventArgs e) {
        this.OnControlValueChanged();
    }

    protected override void UpdateControlValue() {
        this.checkBox.IsChecked = this.SlotModel.Value;
    }

    protected override void UpdateModelValue() {
        this.SlotModel.Value = this.checkBox.IsChecked ?? false;
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.checkBox.IsEnabled = canEdit;
    }
}