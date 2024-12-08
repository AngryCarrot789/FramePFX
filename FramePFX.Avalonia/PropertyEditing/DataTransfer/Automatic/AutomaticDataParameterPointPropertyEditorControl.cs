// 
// Copyright (c) 2024-2024 REghZy
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
using FramePFX.PropertyEditing.DataTransfer.Automatic;
using SkiaSharp;

namespace FramePFX.Avalonia.PropertyEditing.DataTransfer.Automatic;

public class AutomaticDataParameterPointPropertyEditorControl : BaseDataParameterPropertyEditorControl
{
    internal static readonly IImmutableBrush MultipleValuesBrush = BaseNumberDraggerDataParamPropEditorControl.MultipleValuesBrush;

    public new AutomaticDataParameterPointPropertyEditorSlot? SlotModel => (AutomaticDataParameterPointPropertyEditorSlot?) base.SlotControl?.Model;

    protected NumberDragger draggerX;
    protected NumberDragger draggerY;
    protected Button resetButton;

    private readonly AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot> valueFormatterBinder;

    public AutomaticDataParameterPointPropertyEditorControl()
    {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot>(null, nameof(DataParameterFormattableNumberPropertyEditorSlot.ValueFormatterChanged), (x) =>
        {
            AutomaticDataParameterPointPropertyEditorControl editor = (AutomaticDataParameterPointPropertyEditorControl) x.Control;
            editor.draggerX.ValueFormatter = x.Model.ValueFormatter;
            editor.draggerY.ValueFormatter = x.Model.ValueFormatter;
        }, null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.draggerX = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerX");
        this.draggerY = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerY");
        this.resetButton = e.NameScope.GetTemplateChild<Button>("PART_ResetButton");
        this.draggerX.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.draggerY.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.resetButton.Click += this.ResetButtonOnClick;
        this.valueFormatterBinder.AttachControl(this);
        this.UpdateDraggerMultiValueState();
    }

    private void ResetButtonOnClick(object? sender, RoutedEventArgs e)
    {
        AutomaticDataParameterPointPropertyEditorSlot? slot = this.SlotModel;
        if (slot != null && slot.HasHandlers)
        {
            foreach (ITransferableData handler in slot.Handlers)
            {
                slot.IsAutomaticParameter.SetValue(handler, true);
            }
        }
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

        AutomaticDataParameterPointPropertyEditorSlot slot = this.SlotModel!;
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

    protected override void OnHandlersLoadedOverride(bool isLoaded)
    {
        base.OnHandlersLoadedOverride(isLoaded);
        if (isLoaded)
        {
            if (this.singleHandler != null)
                this.SlotModel!.IsAutomaticParameter.AddValueChangedHandler(this.singleHandler, this.OnIsAutomaticChanged);
        }
        else if (this.singleHandler != null)
        {
            this.SlotModel!.IsAutomaticParameter.RemoveValueChangedHandler(this.singleHandler, this.OnIsAutomaticChanged);
        }
    }

    private void OnIsAutomaticChanged(DataParameter parameter, ITransferableData owner)
    {
        this.UpdateTextPreview();
    }

    private void UpdateTextPreview()
    {
        if (this.singleHandler != null && this.SlotModel!.IsAutomaticParameter.GetValue(this.singleHandler))
        {
            this.draggerX.FinalPreviewStringFormat = this.draggerY.FinalPreviewStringFormat = "{0} (Auto)";
        }
        else
        {
            this.draggerX.FinalPreviewStringFormat = this.draggerY.FinalPreviewStringFormat = null;
        }
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
        this.UpdateTextPreview();
    }

    protected override void UpdateModelValue()
    {
        this.SlotModel!.Value = new SKPoint((float) this.draggerX.Value, (float) this.draggerY.Value);
    }
}