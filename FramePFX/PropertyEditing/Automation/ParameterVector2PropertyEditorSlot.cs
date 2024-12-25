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
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Interactivity.Formatting;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Automation;

public delegate void ParameterVector2ValueFormatterChangedEventHandler(ParameterVector2PropertyEditorSlot sender, IValueFormatter? oldValueFormatter, IValueFormatter? newValueFormatter);

public class ParameterVector2PropertyEditorSlot : ParameterPropertyEditorSlot {
    private Vector2 value;
    private IValueFormatter? valueFormatter;

    public Vector2 Value {
        get => this.value;
        set {
            Vector2 oldVal = this.value;
            this.value = value;
            bool useAddition = this.IsMultiHandler;
            Vector2 change = value - oldVal;
            ParameterVector2 parameter = this.Parameter;
            ParameterDescriptorVector2 pdesc = parameter.Descriptor;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                IAutomatable obj = (IAutomatable) this.Handlers[i];
                Vector2 newValue = pdesc.Clamp(useAddition ? (parameter.GetCurrentValue(obj) + change) : value);
                AutomationUtils.SetDefaultKeyFrameOrAddNew(obj, parameter, newValue, (k, d, o) => k.SetVector2Value(o, d));
            }

            this.OnValueChanged(this.lastQueryHasMultipleValues && useAddition, true);
        }
    }

    public IValueFormatter? ValueFormatter {
        get => this.valueFormatter;
        set {
            IValueFormatter? oldValueFormatter = this.valueFormatter;
            if (oldValueFormatter == value)
                return;

            this.valueFormatter = value;
            this.ValueFormatterChanged?.Invoke(this, oldValueFormatter, value);
        }
    }

    public event ParameterVector2ValueFormatterChangedEventHandler? ValueFormatterChanged;

    public new ParameterVector2 Parameter => (ParameterVector2) base.Parameter;

    public DragStepProfile StepProfile { get; }

    public ParameterVector2PropertyEditorSlot(ParameterVector2 parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
        this.StepProfile = stepProfile;
    }

    protected override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetCurrentValue((IAutomatable) x), out this.value);
    }
}