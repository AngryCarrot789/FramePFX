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
using FFmpeg.AutoGen;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.Exporting.FFmpeg;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.Avalonia.PropertyEditing.DataTransfer;

public class DataParameterAVCodedIdPropertyEditorControl : BaseDataParameterPropertyEditorControl
{
    protected ComboBox comboBox;

    public new DataParameterAVCodecIDPropertyEditorSlot? SlotModel => (DataParameterAVCodecIDPropertyEditorSlot?) base.SlotControl!.Model;

    public DataParameterAVCodedIdPropertyEditorControl() {
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

    protected override void UpdateControlValue() {
        DataParameterAVCodecIDPropertyEditorSlot slot = this.SlotModel!;
        if (slot.TranslationInfo != null && slot.TranslationInfo.EnumToText.TryGetValue(slot.Value, out string? text)) {
            this.comboBox.SelectedItem = text;
        }
        else {
            this.comboBox.SelectedItem = slot.Value.ToString();
        }
    }

    protected override void UpdateModelValue() {
        AVCodecID? codecId;
        DataParameterAVCodecIDPropertyEditorSlot slot = this.SlotModel!;
        if (slot.TranslationInfo != null) {
            if (this.comboBox.SelectedItem is string selectedText) {
                codecId = slot.TranslationInfo.TextToEnum[selectedText];
            }
            else {
                codecId = slot.DefaultValue;
            }
        }
        else if (this.comboBox.SelectedItem is AVCodecID AVCodecID) {
            codecId = AVCodecID;
        }
        else {
            codecId = slot.DefaultValue;
        }

        slot.Value = codecId ?? default;
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.comboBox.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        // Initialise list first so that UpdateControlValue has something to work on when base.OnConnected invokes it eventually 

        ItemCollection list = this.comboBox.Items;
        list.Clear();

        if (this.SlotModel!.TranslationInfo != null) {
            foreach (string value in this.SlotModel.TranslationInfo.TextList) {
                list.Add(value);
            }
        }
        else {
            foreach (AVCodecID codecId in this.SlotModel!.ValueEnumerable) {
                list.Add(codecId);
            }
        }

        base.OnConnected();
        this.comboBox.SelectionChanged += this.OnSelectionChanged;
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.comboBox.SelectionChanged -= this.OnSelectionChanged;
        this.comboBox.Items.Clear();
    }
}