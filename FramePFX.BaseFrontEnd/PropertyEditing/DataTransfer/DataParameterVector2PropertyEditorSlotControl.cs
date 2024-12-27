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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using FramePFX.BaseFrontEnd.AvControls.Dragger;
using FramePFX.BaseFrontEnd.Bindings;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer;

public class DataParameterVector2PropertyEditorSlotControl : BaseDataParameterPropertyEditorSlotControl {
    internal static readonly IImmutableBrush MultipleValuesBrush = BaseNumberDraggerDataParamPropEditorSlotControl.MultipleValuesBrush;

    public new DataParameterVector2PropertyEditorSlot? SlotModel => (DataParameterVector2PropertyEditorSlot?) base.SlotControl?.Model;

    protected NumberDragger draggerX;
    protected NumberDragger draggerY;
    protected Button resetButton;

    private readonly AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot> valueFormatterBinder;

    public DataParameterVector2PropertyEditorSlotControl() {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot>(null, nameof(DataParameterFormattableNumberPropertyEditorSlot.ValueFormatterChanged), (x) => {
            DataParameterVector2PropertyEditorSlotControl editor = (DataParameterVector2PropertyEditorSlotControl) x.Control;
            editor.draggerX.ValueFormatter = x.Model.ValueFormatter;
            editor.draggerY.ValueFormatter = x.Model.ValueFormatter;
        }, null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.draggerX = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerX");
        this.draggerX.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.draggerY = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerY");
        this.draggerY.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.resetButton = e.NameScope.GetTemplateChild<Button>("PART_ResetButton");
        this.resetButton.Click += this.ResetButtonOnClick;
        this.valueFormatterBinder.AttachControl(this);
        this.UpdateDraggerMultiValueState();
    }

    private void ResetButtonOnClick(object? sender, RoutedEventArgs e) {
        this.SlotModel.Value = this.SlotModel.Parameter.DefaultValue;
    }

    private void UpdateDraggerMultiValueState() {
        if (!this.IsConnected) {
            return;
        }

        bool flag = this.SlotModel!.HasMultipleValues, flag2 = this.SlotModel!.HasProcessedMultipleValuesSinceSetup;
        BaseNumberDraggerDataParamPropEditorSlotControl.UpdateNumberDragger(this.draggerX, flag, flag2);
        BaseNumberDraggerDataParamPropEditorSlotControl.UpdateNumberDragger(this.draggerY, flag, flag2);
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.draggerX.IsEnabled = canEdit;
        this.draggerY.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        this.valueFormatterBinder.AttachModel(this.SlotModel!);
        base.OnConnected();

        DataParameterVector2PropertyEditorSlot slot = this.SlotModel;
        DataParameterVector2 param = slot.Parameter;
        this.draggerX.Minimum = param.Minimum.X;
        this.draggerY.Minimum = param.Minimum.Y;
        this.draggerX.Maximum = param.Maximum.X;
        this.draggerY.Maximum = param.Maximum.Y;

        DragStepProfile profile = slot.StepProfile;
        this.draggerX.TinyChange = profile.TinyStep;
        this.draggerX.SmallChange = profile.SmallStep;
        this.draggerX.NormalChange = profile.NormalStep;
        this.draggerX.LargeChange = profile.LargeStep;

        this.draggerY.TinyChange = profile.TinyStep;
        this.draggerY.SmallChange = profile.SmallStep;
        this.draggerY.NormalChange = profile.NormalStep;
        this.draggerY.LargeChange = profile.LargeStep;

        this.SlotModel!.HasMultipleValuesChanged += this.OnHasMultipleValuesChanged;
        this.UpdateDraggerMultiValueState();
    }

    protected override void OnDisconnected() {
        this.valueFormatterBinder.DetachModel();
        base.OnDisconnected();

        this.SlotModel!.HasMultipleValuesChanged -= this.OnHasMultipleValuesChanged;
    }

    private void OnHasMultipleValuesChanged(DataParameterPropertyEditorSlot sender) {
        this.UpdateDraggerMultiValueState();
    }

    protected override void UpdateControlValue() {
        Vector2 value = this.SlotModel!.Value;
        this.draggerX.Value = value.X;
        this.draggerY.Value = value.Y;
    }

    protected override void UpdateModelValue() {
        this.SlotModel!.Value = new Vector2((float) this.draggerX.Value, (float) this.draggerY.Value);
    }
}