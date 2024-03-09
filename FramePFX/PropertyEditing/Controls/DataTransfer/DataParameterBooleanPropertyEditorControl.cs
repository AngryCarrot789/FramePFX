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
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing.Controls.DataTransfer
{
    public class DataParameterBooleanPropertyEditorControl : BaseDataParameterPropertyEditorControl
    {
        protected CheckBox checkBox;

        public new DataParameterBooleanPropertyEditorSlot SlotModel => (DataParameterBooleanPropertyEditorSlot) base.SlotControl.Model;

        public DataParameterBooleanPropertyEditorControl()
        {
        }

        static DataParameterBooleanPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DataParameterBooleanPropertyEditorControl), new FrameworkPropertyMetadata(typeof(DataParameterBooleanPropertyEditorControl)));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.checkBox = this.GetTemplateChild<CheckBox>("PART_CheckBox");
            RoutedEventHandler handler = (s, e) => this.OnControlValueChanged();
            this.checkBox.Checked += handler;
            this.checkBox.Unchecked += handler;
        }

        protected override void UpdateControlValue()
        {
            this.checkBox.IsChecked = this.SlotModel.Value;
        }

        protected override void UpdateModelValue()
        {
            this.SlotModel.Value = this.checkBox.IsChecked ?? false;
        }

        protected override void OnCanEditValueChanged(bool canEdit)
        {
            this.checkBox.IsEnabled = canEdit;
        }
    }
}