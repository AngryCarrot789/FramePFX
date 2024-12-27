// 
// Copyright (c) 2024-2024 REghZy
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

namespace FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer.Enums;

public abstract class BaseEnumDataParameterPropertyEditorSlotControlEx : BaseDataParameterPropertyEditorSlotControl {
    protected ComboBox? comboBox;

    protected override Type StyleKeyOverride => typeof(BaseEnumDataParameterPropertyEditorSlotControlEx);

    protected BaseEnumDataParameterPropertyEditorSlotControlEx() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.comboBox = e.NameScope.GetTemplateChild<ComboBox>("PART_ComboBox");
        if (this.IsConnected)
            this.comboBox.SelectionChanged += this.OnSelectionChanged;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        this.OnControlValueChanged();
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.comboBox!.SelectionChanged += this.OnSelectionChanged;
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.comboBox!.SelectionChanged -= this.OnSelectionChanged;
        this.comboBox!.Items.Clear();
    }
}