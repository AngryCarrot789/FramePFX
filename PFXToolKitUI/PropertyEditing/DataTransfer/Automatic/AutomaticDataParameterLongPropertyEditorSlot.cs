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

using PFXToolKitUI.DataTransfer;

namespace PFXToolKitUI.PropertyEditing.DataTransfer.Automatic;

public class AutomaticDataParameterLongPropertyEditorSlot : BaseAutomaticNumericDataParameterPropertyEditorSlot<long> {
    public new DataParameterLong Parameter => (DataParameterLong) base.Parameter;

    public AutomaticDataParameterLongPropertyEditorSlot(DataParameterLong parameter, DataParameterBool isAutomaticParameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, isAutomaticParameter, applicableType, displayName, stepProfile) {
    }

    public override void QueryValueFromHandlers() {
        base.QueryValueFromHandlers();
        DataParameterLong param = this.Parameter;
        if (this.HasMultipleValues && param.HasExplicitRangeLimit)
            this.myLocalValue = Math.Abs(param.Maximum - param.Minimum) / 2;
    }
}