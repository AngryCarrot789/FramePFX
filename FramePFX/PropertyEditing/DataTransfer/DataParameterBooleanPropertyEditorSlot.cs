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

using System;
using FramePFX.Editors.DataTransfer;

namespace FramePFX.PropertyEditing.DataTransfer {
    public class DataParameterBooleanPropertyEditorSlot : DataParameterPropertyEditorSlot {
        private bool value;

        public bool Value {
            get => this.value;
            set {
                this.value = value;
                DataParameterBoolean parameter = this.DataParameter;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    parameter.SetValue((ITransferableData) this.Handlers[i], value);
                }

                this.OnValueChanged();
            }
        }

        public new DataParameterBoolean DataParameter => (DataParameterBoolean)base.DataParameter;

        public DataParameterBooleanPropertyEditorSlot(DataParameterBoolean parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
            
        }

        public override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.DataParameter.GetValue((ITransferableData) x), out bool d) ? d : default;
        }
    }
}