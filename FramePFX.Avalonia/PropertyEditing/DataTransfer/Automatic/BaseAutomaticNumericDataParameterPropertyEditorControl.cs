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

using FramePFX.Avalonia.AvControls.Dragger;
using FramePFX.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer.Automatic;
using FramePFX.Utils;

namespace FramePFX.Avalonia.PropertyEditing.DataTransfer.Automatic;

public abstract class BaseAutomaticNumericDataParameterPropertyEditorControl<T> : BaseNumberDraggerDataParamPropEditorControl {
    public new BaseAutomaticNumericDataParameterPropertyEditorSlot<T>? SlotModel => (BaseAutomaticNumericDataParameterPropertyEditorSlot<T>?) base.SlotControl?.Model;

    public BaseAutomaticNumericDataParameterPropertyEditorControl() {
        
    }

    protected override void UpdateControlValue() {
        base.UpdateControlValue();
        this.UpdateTextPreview();
    }

    protected override void OnConnected() {
        base.OnConnected();
        BaseAutomaticNumericDataParameterPropertyEditorSlot<T> slot = this.SlotModel!;
        DragStepProfile profile = slot.StepProfile;
        this.dragger.TinyChange = profile.TinyStep;
        this.dragger.SmallChange = profile.SmallStep;
        this.dragger.NormalChange = profile.NormalStep;
        this.dragger.LargeChange = profile.LargeStep;
        
        this.dragger.InvalidInputEntered += this.PartDraggerOnInvalidInputEntered;
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.dragger.InvalidInputEntered -= this.PartDraggerOnInvalidInputEntered;
    }

    protected override void OnHandlersLoadedOverride(bool isLoaded) {
        base.OnHandlersLoadedOverride(isLoaded);
        if (isLoaded) {
            if (this.singleHandler != null)
                this.SlotModel!.IsAutomaticParameter.AddValueChangedHandler(this.singleHandler, this.OnIsAutomaticChanged);
        }
        else if (this.singleHandler != null) {
            this.SlotModel!.IsAutomaticParameter.RemoveValueChangedHandler(this.singleHandler, this.OnIsAutomaticChanged);
        }
    }

    private void OnIsAutomaticChanged(DataParameter parameter, ITransferableData owner) {
        this.UpdateTextPreview();
    }
    
    private void UpdateTextPreview() {
        if (this.singleHandler != null && this.SlotModel!.IsAutomaticParameter.GetValue(this.singleHandler) && !this.SlotModel.HasMultipleValues) {
            this.dragger.FinalPreviewStringFormat = "{0} (Auto)";
        }
        else {
            this.dragger.FinalPreviewStringFormat = null;
        }
    }
    
    private void PartDraggerOnInvalidInputEntered(object? sender, InvalidInputEnteredEventArgs e) {
        BaseAutomaticNumericDataParameterPropertyEditorSlot<T>? model = this.SlotModel;
        if (model == null || !model.IsCurrentlyApplicable) {
            return;
        }
        
        if (("auto".EqualsIgnoreCase(e.Input) || "automatic".EqualsIgnoreCase(e.Input) || "\"auto\"".EqualsIgnoreCase(e.Input))) {
            foreach (object handler in model.Handlers) {
                model.IsAutomaticParameter.SetValue((ITransferableData) handler, true);
            }
        }
    }

    protected override void ResetValue() {
        BaseAutomaticNumericDataParameterPropertyEditorSlot<T>? slot = this.SlotModel;
        if (slot == null) {
            return;
        }
        
        foreach (ITransferableData handler in slot.Handlers) {
            slot.IsAutomaticParameter.SetValue(handler, true);
        }
    }

    protected override void OnHasMultipleValuesChanged(DataParameterPropertyEditorSlot sender) {
        base.OnHasMultipleValuesChanged(sender);
        this.UpdateTextPreview();
    }
}