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
using Avalonia.Input;
using FramePFX.Avalonia.Bindings;
using FramePFX.DataTransfer;
using FramePFX.Services.UserInputs;

namespace FramePFX.Avalonia.Services.Messages.Controls;

public partial class DoubleUserInputControl : UserControl, IUserInputContent
{
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> labelABinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBlock.TextProperty, DoubleUserInputInfo.LabelAParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> labelBBinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBlock.TextProperty, DoubleUserInputInfo.LabelBParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> textABinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBox.TextProperty, DoubleUserInputInfo.TextAParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputInfo> textBBinder = new DataParameterPropertyBinder<DoubleUserInputInfo>(TextBox.TextProperty, DoubleUserInputInfo.TextBParameter);
    private UserInputDialog? myDialog;
    private DoubleUserInputInfo? myData;

    public DoubleUserInputControl()
    {
        this.InitializeComponent();
        this.labelABinder.AttachControl(this.PART_LabelA);
        this.labelBBinder.AttachControl(this.PART_LabelB);
        this.textABinder.AttachControl(this.PART_TextBoxA);
        this.textBBinder.AttachControl(this.PART_TextBoxB);

        this.PART_TextBoxA.KeyDown += this.OnAnyTextFieldKeyDown;
        this.PART_TextBoxB.KeyDown += this.OnAnyTextFieldKeyDown;
    }

    private void OnAnyTextFieldKeyDown(object? sender, KeyEventArgs e)
    {
        if ((e.Key == Key.Escape || e.Key == Key.Enter) && this.myDialog != null)
        {
            this.myDialog.TryCloseDialog(e.Key != Key.Escape);
        }
    }

    public void Connect(UserInputDialog dialog, UserInputInfo info)
    {
        this.myDialog = dialog;
        this.myData = (DoubleUserInputInfo) info;
        this.labelABinder.AttachModel(this.myData);
        this.labelBBinder.AttachModel(this.myData);
        this.textABinder.AttachModel(this.myData);
        this.textBBinder.AttachModel(this.myData);
        DataParameter.AddMultipleHandlers(this.OnAnyTextChanged, DoubleUserInputInfo.TextAParameter, DoubleUserInputInfo.TextBParameter);
        DoubleUserInputInfo.LabelAParameter.AddValueChangedHandler(this.myData!, this.OnLabelAChanged);
        DoubleUserInputInfo.LabelBParameter.AddValueChangedHandler(this.myData!, this.OnLabelBChanged);
        this.myData.AllowEmptyTextAChanged += this.OnAllowEmptyTextChanged;
        this.myData.AllowEmptyTextBChanged += this.OnAllowEmptyTextChanged;
        this.UpdateLabelAVisibility();
        this.UpdateLabelBVisibility();
    }

    public void Disconnect()
    {
        this.labelABinder.DetachModel();
        this.labelBBinder.DetachModel();
        this.textABinder.DetachModel();
        this.textBBinder.DetachModel();
        DataParameter.AddMultipleHandlers(this.OnAnyTextChanged, DoubleUserInputInfo.TextAParameter, DoubleUserInputInfo.TextBParameter);
        DoubleUserInputInfo.LabelAParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelAChanged);
        DoubleUserInputInfo.LabelBParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelBChanged);
        this.myData!.AllowEmptyTextAChanged -= this.OnAllowEmptyTextChanged;
        this.myData!.AllowEmptyTextBChanged -= this.OnAllowEmptyTextChanged;

        this.myDialog = null;
        this.myData = null;
    }

    public bool FocusPrimaryInput()
    {
        this.PART_TextBoxA.Focus();
        this.PART_TextBoxA.SelectAll();
        return true;
    }

    private void UpdateLabelAVisibility() => this.PART_LabelA.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.LabelA);

    private void UpdateLabelBVisibility() => this.PART_LabelA.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.LabelA);

    private void OnLabelAChanged(DataParameter dataParameter, ITransferableData owner) => this.UpdateLabelAVisibility();
    private void OnLabelBChanged(DataParameter dataParameter, ITransferableData owner) => this.UpdateLabelBVisibility();

    private void OnAllowEmptyTextChanged(DoubleUserInputInfo sender) => this.myDialog!.InvalidateConfirmButton();
    private void OnAnyTextChanged(DataParameter dataParameter, ITransferableData owner) => this.myDialog!.InvalidateConfirmButton();
}