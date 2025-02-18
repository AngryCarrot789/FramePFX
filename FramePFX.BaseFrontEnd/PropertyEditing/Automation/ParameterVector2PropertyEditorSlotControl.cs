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
using Avalonia.Controls.Primitives;
using FramePFX.Editing.Automation.Params;
using FramePFX.PropertyEditing.Automation;
using PFXToolKitUI.Avalonia.AvControls.Dragger;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.PropertyEditing.DataTransfer;

namespace FramePFX.BaseFrontEnd.PropertyEditing.Automation;

public class ParameterVector2PropertyEditorSlotControl : BaseParameterPropertyEditorSlotControl {
    protected NumberDragger draggerX;
    protected NumberDragger draggerY;
    protected bool IsUpdatingControl;

    public new ParameterVector2PropertyEditorSlot? SlotModel => (ParameterVector2PropertyEditorSlot?) base.SlotControl?.Model;

    private readonly AutoUpdateAndEventPropertyBinder<ParameterVector2PropertyEditorSlot> valueFormatterBinder;

    public ParameterVector2PropertyEditorSlotControl() {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<ParameterVector2PropertyEditorSlot>(null, nameof(ParameterVector2PropertyEditorSlot.ValueFormatterChanged), (x) => {
            ParameterVector2PropertyEditorSlotControl editorSlot = (ParameterVector2PropertyEditorSlotControl) x.Control;
            editorSlot.draggerX.ValueFormatter = x.Model.ValueFormatter;
            editorSlot.draggerY.ValueFormatter = x.Model.ValueFormatter;
        }, null);
        this.valueFormatterBinder.AttachControl(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.draggerX = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerX");
        this.draggerY = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerY");
        this.draggerX.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.draggerY.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.UpdateDraggerMultiValueState();
    }

    private void UpdateDraggerMultiValueState() {
        if (!this.IsConnected) {
            return;
        }

        bool flag = this.SlotModel!.HasMultipleValues, flag2 = this.SlotModel!.HasProcessedMultipleValuesSinceSetup;
        BaseNumberDraggerDataParamPropEditorSlotControl.UpdateNumberDragger(this.draggerX, flag, flag2);
        BaseNumberDraggerDataParamPropEditorSlotControl.UpdateNumberDragger(this.draggerY, flag, flag2);
    }

    protected void UpdateControlValue() {
        Vector2 value = this.SlotModel!.Value;
        this.draggerX.Value = value.X;
        this.draggerY.Value = value.Y;
    }

    protected void UpdateModelValue() {
        this.SlotModel!.Value = new Vector2((float) this.draggerX.Value, (float) this.draggerY.Value);
    }

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

    protected override void OnConnected() {
        ParameterVector2PropertyEditorSlot slot = this.SlotModel!;
        this.valueFormatterBinder.AttachModel(slot);
        base.OnConnected();
        slot.ValueChanged += this.OnSlotValueChanged;
        slot.HasMultipleValuesChanged += this.OnHasMultipleValuesChanged;
        slot.HasProcessedMultipleValuesChanged += this.OnHasProcessedMultipleValuesChanged;

        ParameterDescriptorVector2 desc = slot.Parameter.Descriptor;
        this.draggerX.Minimum = desc.Minimum.X;
        this.draggerX.Maximum = desc.Maximum.X;
        this.draggerY.Minimum = desc.Minimum.Y;
        this.draggerY.Maximum = desc.Maximum.Y;

        DragStepProfile profile = slot.StepProfile;
        this.draggerX.TinyChange = this.draggerY.TinyChange = profile.TinyStep;
        this.draggerX.SmallChange = this.draggerY.SmallChange = profile.SmallStep;
        this.draggerX.NormalChange = this.draggerY.NormalChange = profile.NormalStep;
        this.draggerX.LargeChange = this.draggerY.LargeChange = profile.LargeStep;
    }

    protected override void OnDisconnected() {
        this.valueFormatterBinder.DetachModel();
        base.OnDisconnected();
        ParameterVector2PropertyEditorSlot slot = this.SlotModel!;
        slot.ValueChanged -= this.OnSlotValueChanged;
        slot.HasMultipleValuesChanged += this.OnHasMultipleValuesChanged;
        slot.HasProcessedMultipleValuesChanged += this.OnHasProcessedMultipleValuesChanged;
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