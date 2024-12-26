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

using System.Numerics;
using FramePFX.DataTransfer;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.DataTransfer;

public class DataParameterVector2PropertyEditorSlot : DataParameterFormattableNumberPropertyEditorSlot {
    private Vector2 value;

    public Vector2 Value {
        get => this.value;
        set {
            Vector2 oldVal = this.value;
            this.value = value;
            bool useAddition = false; //this.IsMultiHandler; TODO: Fix with new NumberDragger
            Vector2 change = value - oldVal;
            DataParameterVector2 parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                ITransferableData obj = (ITransferableData) this.Handlers[i];
                Vector2 newValue = parameter.Clamp(useAddition ? (parameter.GetValue(obj) + change) : value);
                parameter.SetValue(obj, newValue);
            }

            this.OnValueChanged(this.lastQueryHasMultipleValues && useAddition, true);
        }
    }

    public new DataParameterVector2 Parameter => (DataParameterVector2) base.Parameter;

    public DragStepProfile StepProfile { get; init; }

    public DataParameterVector2PropertyEditorSlot(DataParameterVector2 parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
        if (this.HasMultipleValues) {
            Vector2 range = (this.Parameter.Maximum - this.Parameter.Minimum);
            this.value = new Vector2(Math.Abs(range.X) / 2.0F, Math.Abs(range.Y) / 2.0F);
        }
    }
}