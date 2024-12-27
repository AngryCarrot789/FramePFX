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

using FramePFX.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer;

public class DataParameterFloatPropertyEditorSlotControl : BaseNumberDraggerDataParamPropEditorSlotControl {
    public new DataParameterFloatPropertyEditorSlot? SlotModel => (DataParameterFloatPropertyEditorSlot?) base.SlotControl?.Model;

    public override double SlotValue {
        get => this.SlotModel!.Value;
        set => this.SlotModel!.Value = (float) value;
    }

    public DataParameterFloatPropertyEditorSlotControl() {
    }

    protected override void OnConnected() {
        base.OnConnected();
        DataParameterFloatPropertyEditorSlot slot = this.SlotModel!;
        DataParameterFloat param = slot.Parameter;
        this.dragger!.Minimum = param.Minimum;
        this.dragger!.Maximum = param.Maximum;

        DragStepProfile profile = slot.StepProfile;
        this.dragger!.TinyChange = profile.TinyStep;
        this.dragger!.SmallChange = profile.SmallStep;
        this.dragger!.NormalChange = profile.NormalStep;
        this.dragger!.LargeChange = profile.LargeStep;
    }

    protected override void ResetValue() => this.SlotValue = this.SlotModel!.Parameter.DefaultValue;
}