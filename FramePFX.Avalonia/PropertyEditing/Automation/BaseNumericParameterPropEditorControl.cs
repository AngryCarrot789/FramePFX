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

using Avalonia.Controls.Primitives;
using FramePFX.Avalonia.AvControls.Dragger;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.PropertyEditing.DataTransfer;
using FramePFX.Avalonia.Utils;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.Avalonia.PropertyEditing.Automation;

public abstract class BaseNumericParameterPropEditorControl : BaseParameterPropertyEditorControl {
    protected NumberDragger? dragger;
    protected bool IsUpdatingControl;
    private readonly AutoUpdateAndEventPropertyBinder<NumericParameterPropertyEditorSlot> valueFormatterBinder;

    public new NumericParameterPropertyEditorSlot? SlotModel => (NumericParameterPropertyEditorSlot?) base.SlotControl?.Model;

    protected BaseNumericParameterPropEditorControl() {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<NumericParameterPropertyEditorSlot>(NumberDragger.ValueFormatterProperty, nameof(NumericParameterPropertyEditorSlot.ValueFormatterChanged), (x) => ((NumberDragger) x.Control).ValueFormatter = x.Model.ValueFormatter, null);
    }

    protected abstract void UpdateControlValue();

    protected abstract void UpdateModelValue();

    private void OnModelValueChanged() {
        if (this.SlotModel != null) {
            this.IsUpdatingControl = true;
            try {
                this.UpdateControlValue();
            }
            finally {
                this.IsUpdatingControl = false;
            }
        }
    }

    private void OnControlValueChanged() {
        if (!this.IsUpdatingControl && this.SlotModel != null) {
            this.UpdateModelValue();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.dragger = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerX");
        this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.valueFormatterBinder.AttachControl(this.dragger);
        this.UpdateDraggerMultiValueState();
    }
    
    private void UpdateDraggerMultiValueState() {
        if (!this.IsConnected) {
            return;
        }

        BaseNumberDraggerDataParamPropEditorControl.UpdateNumberDragger(this.dragger!, this.SlotModel!.HasMultipleValues, this.SlotModel!.HasProcessedMultipleValuesSinceSetup);
    }

    protected override void OnConnected() {
        this.valueFormatterBinder.AttachModel(this.SlotModel!);
        base.OnConnected();
        NumericParameterPropertyEditorSlot slot = this.SlotModel!;
        slot.ValueChanged += this.OnSlotValueChanged;
        slot.HasMultipleValuesChanged += this.OnHasMultipleValuesChanged;
        slot.HasProcessedMultipleValuesChanged += this.OnHasProcessedMultipleValuesChanged;
        
        this.UpdateDraggerMultiValueState();
    }

    protected override void OnDisconnected() {
        this.valueFormatterBinder.DetachModel();
        base.OnDisconnected();
        NumericParameterPropertyEditorSlot slot = this.SlotModel!;
        slot.ValueChanged -= this.OnSlotValueChanged;
    }

    private void OnHasMultipleValuesChanged(ParameterPropertyEditorSlot slot) {
        this.UpdateDraggerMultiValueState();
    }
    
    private void OnHasProcessedMultipleValuesChanged(ParameterPropertyEditorSlot slot) {
        this.UpdateDraggerMultiValueState();
    }
    
    private void OnSlotValueChanged(ParameterPropertyEditorSlot slot) {
        this.OnModelValueChanged();
    }
}