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
using SkiaSharp;

namespace FramePFX.PropertyEditing.DataTransfer;

public class DataParameterColourPropertyEditorSlot : DataParameterFormattableNumberPropertyEditorSlot {
    private SKColor value;

    public SKColor Value {
        get => this.value;
        set {
            this.value = value;
            DataParameter<SKColor> parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                parameter.SetValue((ITransferableData) this.Handlers[i], value);
            }

            this.OnValueChanged(false, true);
        }
    }

    public new DataParameter<SKColor> Parameter => (DataParameter<SKColor>) base.Parameter;

    public DataParameterColourPropertyEditorSlot(DataParameter<SKColor> parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
    }

    public DataParameterColourPropertyEditorSlot(DataParameter<SKColor> parameter, DataParameter<bool> isEditableParameter, bool invertIsEditable, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
        this.IsEditableDataParameter = isEditableParameter;
        this.InvertIsEditableForParameter = invertIsEditable;
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
    }
}