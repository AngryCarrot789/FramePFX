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
using FramePFX.Editors.Automation.Params;
using FramePFX.Utils;

namespace FramePFX.Editors.DataTransfer {
    public sealed class DataParameterBoolean : DataParameterGeneric<bool> {
        public DataParameterBoolean(Type ownerType, string key, ValueAccessor<bool> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, false, accessor, flags) {

        }

        public DataParameterBoolean(Type ownerType, string key, bool defValue, ValueAccessor<bool> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, key, defValue, accessor, flags) {
        }

        public override void SetValue(ITransferableData owner, bool value) {
            // Allow optimised boxing of boolean
            if (this.isObjectAccessPreferred) {
                base.SetObjectValue(owner, value.Box());
            }
            else {
                base.SetValue(owner, value);
            }
        }
    }
}