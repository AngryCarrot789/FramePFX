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
using FramePFX.Editors.DataTransfer;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing.Controls.DataTransfer
{
    public class DataParameterFloatPropertyEditorControl : BaseNumberDataParamPropEditorControl
    {
        public new DataParameterFloatPropertyEditorSlot SlotModel => (DataParameterFloatPropertyEditorSlot) base.SlotControl.Model;

        public DataParameterFloatPropertyEditorControl()
        {
        }

        static DataParameterFloatPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DataParameterFloatPropertyEditorControl), new FrameworkPropertyMetadata(typeof(DataParameterFloatPropertyEditorControl)));

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
            DataParameterFloatPropertyEditorSlot slot = this.SlotModel;
            DataParameterFloat param = slot.DataParameter;
            this.dragger.Minimum = param.Minimum;
            this.dragger.Maximum = param.Maximum;

            DragStepProfile profile = slot.StepProfile;
            this.dragger.TinyChange = profile.TinyStep;
            this.dragger.SmallChange = profile.SmallStep;
            this.dragger.LargeChange = profile.NormalStep;
            this.dragger.MassiveChange = profile.LargeStep;
        }
    }
}