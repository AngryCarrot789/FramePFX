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
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer;

public class DataParameterStringPropertyEditorSlotControl : BaseDataParameterPropertyEditorSlotControl {
    protected TextBox? textBox;

    public new DataParameterStringPropertyEditorSlot? SlotModel => (DataParameterStringPropertyEditorSlot?) base.SlotControl?.Model;

    public DataParameterStringPropertyEditorSlotControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.textBox = e.NameScope.GetTemplateChild<TextBox>("PART_TextBox");
        this.textBox.TextChanged += (sender, args) => this.OnControlValueChanged();
        this.UpdateTextBoxHeight();
    }

    protected override void UpdateControlValue() {
        this.textBox!.Text = this.SlotModel!.Value;
    }

    protected override void UpdateModelValue() {
        this.SlotModel!.Value = this.textBox!.Text!;
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.textBox!.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.SlotModel!.AnticipatedLineCountChanged += this.OnAnticipatedLineCountChanged;
        this.UpdateTextBoxHeight();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.SlotModel!.AnticipatedLineCountChanged -= this.OnAnticipatedLineCountChanged;
    }

    private void OnAnticipatedLineCountChanged(DataParameterStringPropertyEditorSlot sender) {
        this.UpdateTextBoxHeight();
    }

    private void UpdateTextBoxHeight() {
        DataParameterStringPropertyEditorSlot? slot = this.SlotModel;
        if (slot != null) {
            int count = slot.AnticipatedLineCount;
            if (count == -1) {
                this.textBox!.ClearValue(TextBox.MinLinesProperty);
                this.textBox.AcceptsReturn = false;
            }
            else {
                this.textBox!.MinLines = count;
                this.textBox.AcceptsReturn = true;
            }
        }
    }
}