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
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.PropertyEditing.Controls;

namespace FramePFX.Editors.PropertyEditors.AControls {
    public class TimecodeFontFamilyPropertyEditorControl : BasePropEditControlContent {
        public TimecodeFontFamilyPropertyEditorSlot SlotModel => (TimecodeFontFamilyPropertyEditorSlot) base.SlotControl.Model;

        private TextBox fontFamilyTextBox;

        private readonly GetSetAutoEventPropertyBinder<TimecodeFontFamilyPropertyEditorSlot> fontFamilyBinder = new GetSetAutoEventPropertyBinder<TimecodeFontFamilyPropertyEditorSlot>(TextBox.TextProperty, nameof(TimecodeFontFamilyPropertyEditorSlot.FontFamilyChanged), binder => binder.Model.FontFamily, (binder, v) => binder.Model.SetValue((string) v));

        public TimecodeFontFamilyPropertyEditorControl() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.fontFamilyTextBox = this.GetTemplateChild<TextBox>("PART_TextBox");
            // this.fontFamilyTextBox.TextChanged += (sender, args) => this.fontFamilyBinder.OnControlValueChanged();
        }

        static TimecodeFontFamilyPropertyEditorControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimecodeFontFamilyPropertyEditorControl), new FrameworkPropertyMetadata(typeof(TimecodeFontFamilyPropertyEditorControl)));
        }

        protected override void OnConnected() {
            this.fontFamilyBinder.Attach(this.fontFamilyTextBox, this.SlotModel);
        }

        protected override void OnDisconnected() {
            this.fontFamilyBinder.Detatch();
        }
    }
}