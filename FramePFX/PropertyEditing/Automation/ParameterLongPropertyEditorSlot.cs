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

using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Automation;

public class ParameterLongPropertyEditorSlot : NumericParameterPropertyEditorSlot {
    private long value;

    public long Value {
        get => this.value;
        set {
            long oldVal = this.value;
            this.value = value;
            bool useAddition = this.IsMultiHandler;
            long change = value - oldVal;
            ParameterLong parameter = this.Parameter;
            ParameterDescriptorLong pdesc = parameter.Descriptor;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                IAutomatable obj = (IAutomatable) this.Handlers[i];
                long newValue = pdesc.Clamp(useAddition ? (parameter.GetCurrentValue(obj) + change) : value);
                AutomationUtils.SetDefaultKeyFrameOrAddNew(obj, parameter, newValue, (k, d, o) => k.SetLongValue(o, d));
            }

            this.OnValueChanged(this.lastQueryHasMultipleValues && useAddition, true);
        }
    }

    public new ParameterLong Parameter => (ParameterLong) base.Parameter;

    public DragStepProfile StepProfile { get; }

    public ParameterLongPropertyEditorSlot(ParameterLong parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
        this.StepProfile = stepProfile;
    }

    protected override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetCurrentValue((IAutomatable) x), out this.value);
    }
}