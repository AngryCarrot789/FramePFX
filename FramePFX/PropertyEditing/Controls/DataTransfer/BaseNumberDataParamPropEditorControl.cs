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
using FramePFX.Editors.Controls.Dragger;

namespace FramePFX.PropertyEditing.Controls.DataTransfer {
    public abstract class BaseNumberDataParamPropEditorControl : BaseDataParameterPropertyEditorControl {
        protected NumberDragger dragger;

        protected BaseNumberDataParamPropEditorControl() {
        }

        static BaseNumberDataParamPropEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseNumberDataParamPropEditorControl), new FrameworkPropertyMetadata(typeof(BaseNumberDataParamPropEditorControl)));

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.dragger = this.GetTemplateChild<NumberDragger>("PART_DraggerX");
            this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        }

        protected override void OnCanEditValueChanged(bool canEdit) {
            this.dragger.IsEnabled = canEdit;
        }
    }
}