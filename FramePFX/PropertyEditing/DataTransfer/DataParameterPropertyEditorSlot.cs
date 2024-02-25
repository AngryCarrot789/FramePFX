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
    public delegate void DataParameterPropertyEditorSlotEventHandler(DataParameterPropertyEditorSlot slot);

    public abstract class DataParameterPropertyEditorSlot : PropertyEditorSlot {
        private string displayName;

        protected ITransferableData SingleHandler => (ITransferableData) this.Handlers[0];

        public DataParameter DataParameter { get; }

        public string DisplayName {
            get => this.displayName;
            set {
                if (this.displayName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }

        private bool isEditable;

        public bool IsEditable {
            get => this.isEditable;
            set {
                if (this.isEditable == value)
                    return;
                this.isEditable = value;
                DataParameterGeneric<bool> p = this.IsEditableParameter;

                if (p != null) {
                    for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                        p.SetValue((ITransferableData) this.Handlers[i], value);
                    }
                }

                this.IsEditableChanged?.Invoke(this);
            }
        }

        public override bool IsSelectable => true;

        /// <summary>
        /// Gets or sets the parameter which determines if the value can be modified in the UI. This should
        /// only be set during the construction phase of the object and not during its lifetime
        /// </summary>
        public DataParameterGeneric<bool> IsEditableParameter { get; set; }

        /// <summary>
        /// Gets or sets if the parameter's value is inverted between the parameter and checkbox in the UI.
        /// This should only be set during the construction phase of the object and not during its lifetime
        /// </summary>
        public bool InvertIsEditableForParameter { get; set; }

        public event DataParameterPropertyEditorSlotEventHandler IsEditableChanged;
        public event DataParameterPropertyEditorSlotEventHandler DisplayNameChanged;
        public event DataParameterPropertyEditorSlotEventHandler ValueChanged;

        protected DataParameterPropertyEditorSlot(DataParameter parameter, Type applicableType, string displayName = null) : base(applicableType) {
            this.DataParameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            this.displayName = displayName ?? parameter.Key;
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.IsSingleHandler)
                this.SingleHandler.TransferableData.AddValueChangedHandler(this.DataParameter, this.OnValueForSingleHandlerChanged);
            this.QueryValueFromHandlers();
            DataParameterGeneric<bool> p = this.IsEditableParameter;
            this.IsEditable = p == null || (!GetEqualValue(this.Handlers, h => p.GetValue((ITransferableData) h), out bool v) || v);
            this.OnValueChanged();
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            if (this.IsSingleHandler)
                this.SingleHandler.TransferableData.RemoveValueChangedHandler(this.DataParameter, this.OnValueForSingleHandlerChanged);
        }

        private void OnValueForSingleHandlerChanged(DataParameter parameter, ITransferableData owner) {
            this.QueryValueFromHandlers();
            this.OnValueChanged();
        }

        public abstract void QueryValueFromHandlers();

        protected void OnValueChanged() {
            this.ValueChanged?.Invoke(this);
        }
    }
}