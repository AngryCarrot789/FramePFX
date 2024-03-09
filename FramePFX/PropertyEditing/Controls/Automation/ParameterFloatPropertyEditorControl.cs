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
using FramePFX.Editors.Automation.Params;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation
{
    public class ParameterFloatPropertyEditorControl : BaseNumberParameterPropEditorControl
    {
        public new ParameterFloatPropertyEditorSlot SlotModel => (ParameterFloatPropertyEditorSlot) base.SlotControl.Model;

        public ParameterFloatPropertyEditorControl()
        {
        }

        static ParameterFloatPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ParameterFloatPropertyEditorControl), new FrameworkPropertyMetadata(typeof(ParameterFloatPropertyEditorControl)));

        protected override void UpdateControlValue()
        {
            this.dragger.Value = this.SlotModel.Value;
        }

        protected override void UpdateModelValue()
        {
            this.SlotModel.Value = (float) this.dragger.Value;
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            ParameterFloatPropertyEditorSlot slot = this.SlotModel;
            ParameterDescriptorFloat desc = slot.Parameter.Descriptor;
            this.dragger.Minimum = desc.Minimum;
            this.dragger.Maximum = desc.Maximum;

            DragStepProfile profile = slot.StepProfile;
            this.dragger.TinyChange = profile.TinyStep;
            this.dragger.SmallChange = profile.SmallStep;
            this.dragger.LargeChange = profile.NormalStep;
            this.dragger.MassiveChange = profile.LargeStep;
        }
    }
}