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
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.DataTransfer;

public class DataParameterDoublePropertyEditorSlot : DataParameterFormattableNumberPropertyEditorSlot {
    private double value;

    public double Value {
        get => this.value;
        set {
            double oldVal = this.value;
            this.value = value;
            bool useAddition = false; //this.IsMultiHandler; TODO: Fix with new NumberDragger
            double change = value - oldVal;
            DataParameterDouble parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                ITransferableData obj = (ITransferableData) this.Handlers[i];
                double newValue = parameter.Clamp(useAddition ? (parameter.GetValue(obj) + change) : value);
                parameter.SetValue(obj, newValue);
            }

            this.OnValueChanged(this.lastQueryHasMultipleValues && useAddition, true);
        }
    }

    public new DataParameterDouble Parameter => (DataParameterDouble) base.Parameter;

    public DragStepProfile StepProfile { get; }

    public DataParameterDoublePropertyEditorSlot(DataParameterDouble parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
        this.StepProfile = stepProfile;
    }

    public DataParameterDoublePropertyEditorSlot(DataParameterDouble parameter, DataParameter<bool> isEditableParameter, bool invertIsEditable, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
        this.StepProfile = stepProfile;
        this.IsEditableDataParameter = isEditableParameter;
        this.InvertIsEditableForParameter = invertIsEditable;
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
        if (this.HasMultipleValues)
            this.value = Math.Abs(this.Parameter.Maximum - this.Parameter.Minimum) / 2.0D;
    }
}