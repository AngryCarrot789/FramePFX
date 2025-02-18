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
using PFXToolKitUI.Interactivity.Formatting;

namespace FramePFX.PropertyEditing.Automation;

public delegate void SlotValueFormatterChangedEventHandler(NumericParameterPropertyEditorSlot sender, IValueFormatter? oldValueFormatter, IValueFormatter? newValueFormatter);

public abstract class NumericParameterPropertyEditorSlot : ParameterPropertyEditorSlot {
    private IValueFormatter? valueFormatter;

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

    public event SlotValueFormatterChangedEventHandler? ValueFormatterChanged;

    protected NumericParameterPropertyEditorSlot(Parameter parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName) {
    }
}