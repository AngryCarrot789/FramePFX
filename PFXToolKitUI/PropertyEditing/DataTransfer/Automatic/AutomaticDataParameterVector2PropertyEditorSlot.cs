// 
// Copyright (c) 2024-2024 REghZy
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
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.DataTransfer.Automatic;

public class AutomaticDataParameterVector2PropertyEditorSlot : DataParameterFormattableNumberPropertyEditorSlot {
    protected Vector2 myLocalValue;

    /// <summary>
    /// Gets or sets the value. Setting this will update the value for all of our handlers,
    /// and it will also set the <see cref="IsAutomaticParameter"/> for all parameters to false
    /// </summary>
    public Vector2 Value {
        get => this.myLocalValue;
        set {
            this.myLocalValue = value;
            DataParameterVector2 parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                ITransferableData obj = (ITransferableData) this.Handlers[i];
                // Set IsAutomatic to false so that the value is no longer auto-calculated by the handlers.
                // Though there ideally shouldn't be any issue setting it to false after setting the parameter
                this.IsAutomaticParameter.SetValue(obj, false);
                parameter.SetValue(obj, parameter.Clamp(value));
            }

            this.OnValueChanged(false, true);
        }
    }

    public new DataParameterVector2 Parameter => (DataParameterVector2) base.Parameter;

    public DataParameterBool IsAutomaticParameter { get; }

    public DragStepProfile StepProfile { get; init; }

    public AutomaticDataParameterVector2PropertyEditorSlot(DataParameter parameter, DataParameterBool isAutomaticParameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName) {
        this.IsAutomaticParameter = isAutomaticParameter;
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.myLocalValue);
        DataParameterVector2 param = this.Parameter;
        if (this.HasMultipleValues && param.HasExplicitRangeLimit) {
            Vector2 range = param.Maximum - param.Minimum;
            this.myLocalValue = new Vector2(Math.Abs(range.X) / 2.0F, Math.Abs(range.Y) / 2.0F);
        }
    }
}