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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using FramePFX.Avalonia.AvControls.Dragger;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Utils;
using FramePFX.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer;
using SkiaSharp;

namespace FramePFX.Avalonia.PropertyEditing.DataTransfer;

public class DataParameterPointPropertyEditorControl : BaseDataParameterPropertyEditorControl
{
    internal static readonly IImmutableBrush MultipleValuesBrush = BaseNumberDraggerDataParamPropEditorControl.MultipleValuesBrush;

    public new DataParameterPointPropertyEditorSlot? SlotModel => (DataParameterPointPropertyEditorSlot?) base.SlotControl?.Model;

    protected NumberDragger draggerX;
    protected NumberDragger draggerY;
    protected Button resetButton;

    private readonly AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot> valueFormatterBinder;

    public DataParameterPointPropertyEditorControl()
    {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot>(null, nameof(DataParameterFormattableNumberPropertyEditorSlot.ValueFormatterChanged), (x) =>
        {
            DataParameterPointPropertyEditorControl editor = (DataParameterPointPropertyEditorControl) x.Control;
            editor.draggerX.ValueFormatter = x.Model.ValueFormatter;
            editor.draggerY.ValueFormatter = x.Model.ValueFormatter;
        }, null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
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

    private void ResetButtonOnClick(object? sender, RoutedEventArgs e)
    {
        this.SlotModel.Value = this.SlotModel.Parameter.DefaultValue;
    }

    private void UpdateDraggerMultiValueState()
    {
        if (!this.IsConnected)
        {
            return;
        }

        bool flag = this.SlotModel!.HasMultipleValues, flag2 = this.SlotModel!.HasProcessedMultipleValuesSinceSetup;
        BaseNumberDraggerDataParamPropEditorControl.UpdateNumberDragger(this.draggerX, flag, flag2);
        BaseNumberDraggerDataParamPropEditorControl.UpdateNumberDragger(this.draggerY, flag, flag2);
    }

    protected override void OnCanEditValueChanged(bool canEdit)
    {
        this.draggerX.IsEnabled = canEdit;
        this.draggerY.IsEnabled = canEdit;
    }

    protected override void OnConnected()
    {
        this.valueFormatterBinder.AttachModel(this.SlotModel!);
        base.OnConnected();

        DataParameterPointPropertyEditorSlot slot = this.SlotModel;
        DataParameterPoint param = slot.Parameter;
        this.draggerX.Minimum = param.Minimum.X;
        this.draggerY.Minimum = param.Minimum.Y;
        this.draggerX.Maximum = param.Maximum.X;
        this.draggerY.Maximum = param.Maximum.Y;

        DragStepProfile profileX = slot.StepProfileX;
        this.draggerX.TinyChange = profileX.TinyStep;
        this.draggerX.SmallChange = profileX.SmallStep;
        this.draggerX.NormalChange = profileX.NormalStep;
        this.draggerX.LargeChange = profileX.LargeStep;

        DragStepProfile profileY = slot.StepProfileY;
        this.draggerY.TinyChange = profileY.TinyStep;
        this.draggerY.SmallChange = profileY.SmallStep;
        this.draggerY.NormalChange = profileY.NormalStep;
        this.draggerY.LargeChange = profileY.LargeStep;

        this.SlotModel!.HasMultipleValuesChanged += this.OnHasMultipleValuesChanged;
        this.UpdateDraggerMultiValueState();
    }

    protected override void OnDisconnected()
    {
        this.valueFormatterBinder.DetachModel();
        base.OnDisconnected();

        this.SlotModel!.HasMultipleValuesChanged -= this.OnHasMultipleValuesChanged;
    }

    private void OnHasMultipleValuesChanged(DataParameterPropertyEditorSlot sender)
    {
        this.UpdateDraggerMultiValueState();
    }

    protected override void UpdateControlValue()
    {
        SKPoint value = this.SlotModel!.Value;
        this.draggerX.Value = value.X;
        this.draggerY.Value = value.Y;
    }

    protected override void UpdateModelValue()
    {
        this.SlotModel!.Value = new SKPoint((float) this.draggerX.Value, (float) this.draggerY.Value);
    }
}