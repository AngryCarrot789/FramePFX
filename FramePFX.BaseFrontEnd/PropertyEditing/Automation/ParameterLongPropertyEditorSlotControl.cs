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

using FramePFX.Editing.Automation.Params;
using FramePFX.PropertyEditing.Automation;
using PFXToolKitUI.PropertyEditing.DataTransfer;

namespace FramePFX.BaseFrontEnd.PropertyEditing.Automation;

public class ParameterLongPropertyEditorSlotControl : BaseNumericParameterPropertyEditorSlotControl {
    public new ParameterLongPropertyEditorSlot SlotModel => (ParameterLongPropertyEditorSlot) base.SlotControl.Model;

    public ParameterLongPropertyEditorSlotControl() {
    }

    protected override void UpdateControlValue() {
        this.dragger!.Value = this.SlotModel.Value;
    }

    protected override void UpdateModelValue() {
        this.SlotModel.Value = (long) Math.Round(this.dragger!.Value);
    }

    protected override void OnConnected() {
        base.OnConnected();
        ParameterLongPropertyEditorSlot slot = this.SlotModel;
        ParameterDescriptorLong desc = slot.Parameter.Descriptor;
        this.dragger!.Minimum = desc.Minimum;
        this.dragger!.Maximum = desc.Maximum;

        DragStepProfile profile = slot.StepProfile;
        this.dragger.TinyChange = Math.Max(profile.TinyStep, 1.0);
        this.dragger.SmallChange = Math.Max(profile.SmallStep, 1.0);
        this.dragger.NormalChange = Math.Max(profile.NormalStep, 1.0);
        this.dragger.LargeChange = Math.Max(profile.LargeStep, 1.0);
    }
}