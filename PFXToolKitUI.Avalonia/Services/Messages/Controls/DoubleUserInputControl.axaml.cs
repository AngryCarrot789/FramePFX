﻿// 
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
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.UserInputs;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Services.UserInputs;

namespace PFXToolKitUI.Avalonia.Services.Messages.Controls;

public partial class DoubleUserInputControl : UserControl, IUserInputContent {
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> labelABinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBlock.TextProperty, DoubleUserInputInfo.LabelAParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> labelBBinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBlock.TextProperty, DoubleUserInputInfo.LabelBParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> textABinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBox.TextProperty, DoubleUserInputInfo.TextAParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> textBBinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBox.TextProperty, DoubleUserInputInfo.TextBParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> footerBinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBlock.TextProperty, BaseTextUserInputInfo.FooterParameter);
    private UserInputDialog? myDialog;
    private DoubleUserInputInfo? myData;

    public DoubleUserInputControl() {
        this.InitializeComponent();
        this.labelABinder.AttachControl(this.PART_LabelA);
        this.labelBBinder.AttachControl(this.PART_LabelB);
        this.textABinder.AttachControl(this.PART_TextBoxA);
        this.textBBinder.AttachControl(this.PART_TextBoxB);
        this.footerBinder.AttachControl(this.PART_FooterTextBlock);

        this.PART_TextBoxA.KeyDown += this.OnAnyTextFieldKeyDown;
        this.PART_TextBoxB.KeyDown += this.OnAnyTextFieldKeyDown;
    }

    private void OnAnyTextFieldKeyDown(object? sender, KeyEventArgs e) {
        if ((e.Key == Key.Escape || e.Key == Key.Enter) && this.myDialog != null) {
            this.myDialog.TryCloseDialog(e.Key != Key.Escape);
        }
    }

    public void Connect(UserInputDialog dialog, UserInputInfo info) {
        this.myDialog = dialog;
        this.myData = (DoubleUserInputInfo) info;
        this.labelABinder.AttachModel(this.myData);
        this.labelBBinder.AttachModel(this.myData);
        this.textABinder.AttachModel(this.myData);
        this.textBBinder.AttachModel(this.myData);
        this.footerBinder.AttachModel(this.myData);
        DoubleUserInputInfo.LabelAParameter.AddValueChangedHandler(this.myData!, this.OnLabelAChanged);
        DoubleUserInputInfo.LabelBParameter.AddValueChangedHandler(this.myData!, this.OnLabelBChanged);
        BaseTextUserInputInfo.FooterParameter.AddValueChangedHandler(this.myData!, this.OnFooterChanged);
        this.myData.TextErrorsAChanged += this.UpdateTextErrorsA;
        this.myData.TextErrorsBChanged += this.UpdateTextErrorsB;
        this.UpdateLabelAVisibility();
        this.UpdateLabelBVisibility();
        this.UpdateFooterVisibility();
        this.UpdateTextErrorsA(this.myData);
        this.UpdateTextErrorsB(this.myData);
    }

    public void Disconnect() {
        this.labelABinder.DetachModel();
        this.labelBBinder.DetachModel();
        this.textABinder.DetachModel();
        this.textBBinder.DetachModel();
        this.footerBinder.DetachModel();
        DoubleUserInputInfo.LabelAParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelAChanged);
        DoubleUserInputInfo.LabelBParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelBChanged);
        BaseTextUserInputInfo.FooterParameter.RemoveValueChangedHandler(this.myData!, this.OnFooterChanged);
        this.myData!.TextErrorsAChanged -= this.UpdateTextErrorsA;
        this.myData!.TextErrorsBChanged -= this.UpdateTextErrorsB;
        this.myDialog = null;
        this.myData = null;
    }

    private void UpdateTextErrorsA(DoubleUserInputInfo info) {
        SingleUserInputControl.SetErrorsOrClear(this.PART_TextBoxA, info.TextErrorsA);
    }

    private void UpdateTextErrorsB(DoubleUserInputInfo info) {
        SingleUserInputControl.SetErrorsOrClear(this.PART_TextBoxB, info.TextErrorsB);
    }

    public bool FocusPrimaryInput() {
        this.PART_TextBoxA.Focus();
        this.PART_TextBoxA.SelectAll();
        return true;
    }

    private void UpdateLabelAVisibility() => this.PART_LabelA.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.LabelA);
    private void UpdateLabelBVisibility() => this.PART_LabelA.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.LabelA);
    private void UpdateFooterVisibility() => this.PART_FooterTextBlock.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.Footer);

    private void OnLabelAChanged(DataParameter dataParameter, ITransferableData owner) => this.UpdateLabelAVisibility();
    private void OnLabelBChanged(DataParameter dataParameter, ITransferableData owner) => this.UpdateLabelBVisibility();
    private void OnFooterChanged(DataParameter dataParameter, ITransferableData owner) => this.UpdateFooterVisibility();
}