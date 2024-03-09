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
using FramePFX.Editors.Controls.Bindings;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.PropertyEditing.Controls;

namespace FramePFX.Editors.PropertyEditors.AControls
{
    public class DisplayNamePropertyEditorControl : BasePropEditControlContent
    {
        public DisplayNamePropertyEditorSlot SlotModel => (DisplayNamePropertyEditorSlot) base.SlotControl.Model;

        private TextBox displayNameBox;

        private readonly GetSetAutoEventPropertyBinder<DisplayNamePropertyEditorSlot> displayNameBinder = new GetSetAutoEventPropertyBinder<DisplayNamePropertyEditorSlot>(TextBox.TextProperty, nameof(DisplayNamePropertyEditorSlot.DisplayNameChanged), binder => binder.Model.DisplayName, (binder, v) => binder.Model.SetValue((string) v));

        public DisplayNamePropertyEditorControl()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.displayNameBox = this.GetTemplateChild<TextBox>("PART_TextBox");
        }

        static DisplayNamePropertyEditorControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DisplayNamePropertyEditorControl), new FrameworkPropertyMetadata(typeof(DisplayNamePropertyEditorControl)));
        }

        protected override void OnConnected()
        {
            this.displayNameBinder.Attach(this.displayNameBox, this.SlotModel);
        }

        protected override void OnDisconnected()
        {
            this.displayNameBinder.Detach();
        }
    }
}