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

namespace FramePFX.PropertyEditing.DataTransfer;

public delegate void DataParameterPropertyEditorSlotEventHandler(DataParameterPropertyEditorSlot slot);

public delegate void SlotHasMultipleValuesChangedEventHandler(DataParameterPropertyEditorSlot sender);

public delegate void SlotHasProcessedMultipleValuesChangedEventHandler(DataParameterPropertyEditorSlot sender);

public abstract class DataParameterPropertyEditorSlot : PropertyEditorSlot {
    private string displayName;
    private bool isEditable;
    private bool hasMultipleValues;
    private bool hasProcessedMultipleValuesSinceSetup;
    protected bool lastQueryHasMultipleValues;

    protected ITransferableData SingleHandler => (ITransferableData) this.Handlers[0];

    /// <summary>
    /// Gets the value parameter used to communicate the value to/from our handlers
    /// </summary>
    public DataParameter Parameter { get; }

    public string DisplayName {
        get => this.displayName;
        set {
            if (this.displayName == value)
                return;
            this.displayName = value;
            this.DisplayNameChanged?.Invoke(this);
        }
    }

    public bool IsEditable {
        get => this.isEditable;
        set {
            if (this.isEditable == value)
                return;
            this.isEditable = value;
            DataParameter<bool>? p = this.IsEditableDataParameter;

            if (p != null) {
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    p.SetValue((ITransferableData) this.Handlers[i], value);
                }
            }

            this.IsEditableChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets whether the slot has multiple handlers and they all have different underlying values.
    /// This is used to present some sort of signal in the UI warning the user before they try to modify it.
    /// This state must be updated manually by derived classes when the values are no longer different
    /// </summary>
    public bool HasMultipleValues {
        get => this.hasMultipleValues;
        protected set {
            if (this.hasMultipleValues == value)
                return;

            if (value)
                this.HasProcessedMultipleValuesSinceSetup = false;
            this.hasMultipleValues = value;
            this.HasMultipleValuesChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets or sets whether the <see cref="HasMultipleValues"/> has been
    /// updated since <see cref="BasePropertyEditorItem.IsCurrentlyApplicable"/> became true
    /// </summary>
    public bool HasProcessedMultipleValuesSinceSetup {
        get => this.hasProcessedMultipleValuesSinceSetup;
        set {
            if (this.hasProcessedMultipleValuesSinceSetup == value)
                return;

            this.hasProcessedMultipleValuesSinceSetup = value;
            this.HasProcessedMultipleValuesChanged?.Invoke(this);
        }
    }

    public override bool IsSelectable => true;

    /// <summary>
    /// Gets or sets the parameter which determines if the value can be modified in the UI. This should
    /// only be set during the construction phase of the object and not during its lifetime
    /// </summary>
    public DataParameter<bool>? IsEditableDataParameter { get; set; }

    /// <summary>
    /// Gets or sets if the parameter's value is inverted between the parameter and checkbox in the UI.
    /// This should only be set during the construction phase of the object and not during its lifetime
    /// </summary>
    public bool InvertIsEditableForParameter { get; set; }

    /// <summary>
    /// An event fired when <see cref="HasMultipleValues"/> changes
    /// </summary>
    public event SlotHasMultipleValuesChangedEventHandler? HasMultipleValuesChanged;

    public event SlotHasProcessedMultipleValuesChangedEventHandler? HasProcessedMultipleValuesChanged;

    public event DataParameterPropertyEditorSlotEventHandler? IsEditableChanged;
    public event DataParameterPropertyEditorSlotEventHandler? DisplayNameChanged;
    public event DataParameterPropertyEditorSlotEventHandler? ValueChanged;

    protected DataParameterPropertyEditorSlot(DataParameter parameter, Type applicableType, string? displayName = null) : base(applicableType) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        this.displayName = displayName ?? parameter.Name;
    }

    protected override void OnHandlersLoaded() {
        base.OnHandlersLoaded();
        if (this.IsSingleHandler)
            this.Parameter.AddValueChangedHandler(this.SingleHandler, this.OnValueForSingleHandlerChanged);
        this.QueryValueFromHandlers();
        this.lastQueryHasMultipleValues = this.HasMultipleValues;
        DataParameter<bool>? p = this.IsEditableDataParameter;
        this.IsEditable = p == null || (!CollectionUtils.GetEqualValue(this.Handlers, h => p.GetValue((ITransferableData) h), out bool v) || v);
        this.OnValueChanged();
    }

    protected override void OnClearingHandlers() {
        base.OnClearingHandlers();
        if (this.IsSingleHandler)
            this.Parameter.RemoveValueChangedHandler(this.SingleHandler, this.OnValueForSingleHandlerChanged);
    }

    private void OnValueForSingleHandlerChanged(DataParameter parameter, ITransferableData owner) {
        this.QueryValueFromHandlers();
        this.lastQueryHasMultipleValues = this.HasMultipleValues;
        this.OnValueChanged();
    }

    public abstract void QueryValueFromHandlers();

    /// <summary>
    /// Raises the value changed event, and optionally updates the <see cref="HasMultipleValues"/> (e.g. for
    /// if the value of each handler was set to a new value, it can be set to false now)
    /// </summary>
    /// <param name="hasMultipleValues">The optional new value of <see cref="HasMultipleValues"/></param>
    protected void OnValueChanged(bool? hasMultipleValues = null, bool? hasProcessedMultiValueSinceSetup = null) {
        this.ValueChanged?.Invoke(this);
        if (hasMultipleValues.HasValue)
            this.HasMultipleValues = hasMultipleValues.Value;
        if (hasProcessedMultiValueSinceSetup.HasValue)
            this.HasProcessedMultipleValuesSinceSetup = hasProcessedMultiValueSinceSetup.Value;
    }
}