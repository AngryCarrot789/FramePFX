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
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Effects;
using PFXToolKitUI.PropertyEditing;

namespace FramePFX.PropertyEditing.Automation;

public delegate void ParameterPropertyEditorSlotEventHandler(ParameterPropertyEditorSlot slot);

public abstract class ParameterPropertyEditorSlot : PropertyEditorSlot {
    private string displayName;
    private bool hasMultipleValues;
    private bool hasProcessedMultipleValuesSinceSetup;
    protected bool lastQueryHasMultipleValues;

    protected IAutomatable SingleHandler => (IAutomatable) this.Handlers[0];

    public Parameter Parameter { get; }

    public string DisplayName {
        get => this.displayName;
        set {
            if (this.displayName == value)
                return;
            this.displayName = value;
            this.DisplayNameChanged?.Invoke(this);
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

    public event ParameterPropertyEditorSlotEventHandler? DisplayNameChanged;
    public event ParameterPropertyEditorSlotEventHandler? ValueChanged;
    public event ParameterPropertyEditorSlotEventHandler? HasMultipleValuesChanged;
    public event ParameterPropertyEditorSlotEventHandler? HasProcessedMultipleValuesChanged;

    protected ParameterPropertyEditorSlot(Parameter parameter, Type applicableType, string? displayName = null) : base(applicableType) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        this.displayName = displayName ?? parameter.Key.ToString();
        this.IsSelectedChanged += this.OnIsSelectedChanged;
    }

    private void OnIsSelectedChanged(PropertyEditorSlot sender) {
        bool selected = this.IsSelected;
        foreach (IAutomatable automatable in this.Handlers) {
            if (automatable is Clip clip) {
                clip.ActiveSequence = selected ? clip.AutomationData[this.Parameter] : null;
            }
            else if (automatable is BaseEffect effect && effect.Owner is Clip effectClipOwner) {
                effectClipOwner.ActiveSequence = selected ? effect.AutomationData[this.Parameter] : null;
            }
        }
    }

    protected override void OnHandlersLoaded() {
        base.OnHandlersLoaded();
        if (this.IsSingleHandler)
            this.SingleHandler.AutomationData[this.Parameter].ParameterChanged += this.OnValueForSingleHandlerChanged;
        this.QueryValueFromHandlers();
        this.lastQueryHasMultipleValues = this.HasMultipleValues;
        this.OnValueChanged();
    }

    protected override void OnClearingHandlers() {
        base.OnClearingHandlers();
        if (this.IsSingleHandler)
            this.SingleHandler.AutomationData[this.Parameter].ParameterChanged -= this.OnValueForSingleHandlerChanged;
    }

    private void OnValueForSingleHandlerChanged(AutomationSequence sequence) {
        this.QueryValueFromHandlers();
        this.lastQueryHasMultipleValues = this.HasMultipleValues;
        this.OnValueChanged();
    }

    /// <summary>
    /// Sets the local value of this slot based on our handlers. This method does not need to
    /// call <see cref="OnValueChanged"/> as it is called automatically after this call
    /// </summary>
    protected abstract void QueryValueFromHandlers();

    protected void OnValueChanged(bool? hasMultipleValues = null, bool? hasProcessedMultiValueSinceSetup = null) {
        this.ValueChanged?.Invoke(this);
        if (hasMultipleValues.HasValue)
            this.HasMultipleValues = hasMultipleValues.Value;
        if (hasProcessedMultiValueSinceSetup.HasValue)
            this.HasProcessedMultipleValuesSinceSetup = hasProcessedMultiValueSinceSetup.Value;
    }
}