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
using Avalonia.Media.Immutable;
using FramePFX.BaseFrontEnd.AvControls.Dragger;
using FramePFX.BaseFrontEnd.Bindings;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer;

public abstract class BaseNumberDraggerDataParamPropEditorSlotControl : BaseDataParameterPropertyEditorSlotControl {
    internal static readonly IImmutableBrush MultipleValuesBrush;

    public new DataParameterFormattableNumberPropertyEditorSlot? SlotModel => (DataParameterFormattableNumberPropertyEditorSlot?) base.SlotControl?.Model;

    /// <summary>
    /// Gets or sets the slot value as a double suitable for the number dragger
    /// </summary>
    public abstract double SlotValue { get; set; }

    protected NumberDragger? dragger;
    protected Button? resetButton;
    private readonly AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot> valueFormatterBinder;

    protected BaseNumberDraggerDataParamPropEditorSlotControl() {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot>(NumberDragger.ValueFormatterProperty, nameof(DataParameterFormattableNumberPropertyEditorSlot.ValueFormatterChanged), (x) => ((NumberDragger) x.Control).ValueFormatter = x.Model.ValueFormatter, null);
    }

    static BaseNumberDraggerDataParamPropEditorSlotControl() {
        MultipleValuesBrush = new ImmutableSolidColorBrush(Brushes.OrangeRed.Color, 0.7);
    }

    protected override void UpdateControlValue() {
        this.dragger.Value = this.SlotValue;
    }

    protected override void UpdateModelValue() {
        this.SlotValue = this.dragger.Value;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.dragger = e.NameScope.GetTemplateChild<NumberDragger>("PART_Dragger");
        this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.resetButton = e.NameScope.GetTemplateChild<Button>("PART_ResetButton");
        this.resetButton.Click += this.OnResetClick;
        this.valueFormatterBinder.AttachControl(this.dragger);
        this.UpdateDraggerMultiValueState();
    }

    private void OnResetClick(object? sender, RoutedEventArgs e) {
        if (this.IsConnected) {
            this.ResetValue();
        }
    }

    protected virtual void ResetValue() {
    }

    private void UpdateDraggerMultiValueState() {
        if (!this.IsConnected) {
            return;
        }

        UpdateNumberDragger(this.dragger, this.SlotModel!.HasMultipleValues, this.SlotModel!.HasProcessedMultipleValuesSinceSetup);
    }

    public static void UpdateNumberDragger(NumberDragger dragger, bool hasMultipleValues, bool hasUsedAdditionSinceSetup) {
        // TODO: really need to make a derived NumberDragger specifically for this case
        // Not going to use hasUsedAdditionSinceSetup for now. Because it's
        // quite finicky when the dragger's minimum is 0.0.
        // And also if we instead set the value to the midway between the dragger's
        // min and max, we need to initially preview it as 0 and prefix with + or -
        // to represent addition/subtraction from the original values.
        // It's just confusing, especially if the value has a narrow range (e.g. 0.0 to 1.0)
        // because eventually, all selected objects' values will become the same if you
        // keep dragging, unless we disallow dragging once one object reaches the min/max
        // which is possible but is this even a good idea though???
        // Definitely need a derived NumberDragger too but it's just past midnight :'(
        if (hasMultipleValues /* && !hasUsedAdditionSinceSetup */) {
            dragger.TextPreviewOverride = "<<Multiple Values>>";
        }
        else {
            dragger.TextPreviewOverride = null;
        }

        if (hasMultipleValues) {
            dragger.SetCurrentValue(BackgroundProperty, MultipleValuesBrush);
            dragger.SetCurrentValue(ToolTip.TipProperty, "This dragger currently has multiple values present. Modifying this value will change the underlying value for all selected objects");
        }
        else {
            dragger.ClearValue(BackgroundProperty);
            dragger.ClearValue(ToolTip.TipProperty);
        }
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.dragger.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        this.valueFormatterBinder.AttachModel(this.SlotModel!);
        base.OnConnected();

        this.SlotModel!.HasMultipleValuesChanged += this.OnHasMultipleValuesChanged;
        this.SlotModel!.HasProcessedMultipleValuesChanged += this.OnHasProcessedMultipleValuesChanged;
        this.UpdateDraggerMultiValueState();
    }

    protected override void OnDisconnected() {
        this.valueFormatterBinder.DetachModel();
        base.OnDisconnected();

        this.SlotModel!.HasMultipleValuesChanged -= this.OnHasMultipleValuesChanged;
        this.SlotModel!.HasProcessedMultipleValuesChanged -= this.OnHasProcessedMultipleValuesChanged;
    }

    protected virtual void OnHasMultipleValuesChanged(DataParameterPropertyEditorSlot sender) {
        this.UpdateDraggerMultiValueState();
    }

    protected virtual void OnHasProcessedMultipleValuesChanged(DataParameterPropertyEditorSlot sender) {
        this.UpdateDraggerMultiValueState();
    }
}