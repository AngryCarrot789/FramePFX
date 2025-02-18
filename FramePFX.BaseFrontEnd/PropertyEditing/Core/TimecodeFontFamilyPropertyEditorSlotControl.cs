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
using FramePFX.Editing.PropertyEditors;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.PropertyEditing;
using PFXToolKitUI.Avalonia.Utils;

namespace FramePFX.BaseFrontEnd.PropertyEditing.Core;

public class TimecodeFontFamilyPropertyEditorSlotControl : BasePropertyEditorSlotControl {
    public TimecodeFontFamilyPropertyEditorSlot? SlotModel => (TimecodeFontFamilyPropertyEditorSlot?) base.SlotControl?.Model;

    private TextBox? fontFamilyTextBox;

    private readonly IBinder<TimecodeFontFamilyPropertyEditorSlot> fontFamilyBinder = new GetSetAutoUpdateAndEventPropertyBinder<TimecodeFontFamilyPropertyEditorSlot>(TextBox.TextProperty, nameof(TimecodeFontFamilyPropertyEditorSlot.FontFamilyChanged), binder => binder.Model.FontFamily, (binder, v) => binder.Model.SetValue((string) v));

    public TimecodeFontFamilyPropertyEditorSlotControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.fontFamilyTextBox = e.NameScope.GetTemplateChild<TextBox>("PART_TextBox");
        // this.fontFamilyTextBox.TextChanged += (sender, args) => this.fontFamilyBinder.OnControlValueChanged();
    }

    protected override void OnConnected() {
        this.fontFamilyBinder.Attach(this.fontFamilyTextBox!, this.SlotModel!);
    }

    protected override void OnDisconnected() {
        this.fontFamilyBinder.Detach();
    }
}