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