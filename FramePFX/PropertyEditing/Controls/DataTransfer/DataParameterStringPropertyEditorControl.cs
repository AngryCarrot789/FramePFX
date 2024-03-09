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
    public class DataParameterStringPropertyEditorControl : BaseDataParameterPropertyEditorControl
    {
        protected TextBox textBox;

        public new DataParameterStringPropertyEditorSlot SlotModel => (DataParameterStringPropertyEditorSlot) base.SlotControl.Model;

        public DataParameterStringPropertyEditorControl()
        {
        }

        static DataParameterStringPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DataParameterStringPropertyEditorControl), new FrameworkPropertyMetadata(typeof(DataParameterStringPropertyEditorControl)));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.textBox = this.GetTemplateChild<TextBox>("PART_TextBox");
            this.textBox.TextChanged += (sender, args) => this.OnControlValueChanged();
        }

        protected override void UpdateControlValue()
        {
            this.textBox.Text = this.SlotModel.Value;
        }

        protected override void UpdateModelValue()
        {
            this.SlotModel.Value = this.textBox.Text;
        }

        protected override void OnCanEditValueChanged(bool canEdit)
        {
            this.textBox.IsEnabled = canEdit;
        }
    }
}