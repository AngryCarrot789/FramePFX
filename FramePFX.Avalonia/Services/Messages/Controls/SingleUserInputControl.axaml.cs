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

public partial class SingleUserInputControl : UserControl, IUserInputContent {
    private readonly DataParameterPropertyBinder<SingleUserInputInfo> labelBinder = new DataParameterPropertyBinder<SingleUserInputInfo>(TextBlock.TextProperty, SingleUserInputInfo.LabelParameter);
    private readonly DataParameterPropertyBinder<SingleUserInputInfo> textBinder = new DataParameterPropertyBinder<SingleUserInputInfo>(TextBox.TextProperty, SingleUserInputInfo.TextParameter);
    private UserInputDialog? myDialog;
    private SingleUserInputInfo? myData;

    public SingleUserInputControl() {
        this.InitializeComponent();
        this.labelBinder.AttachControl(this.PART_Label);
        this.textBinder.AttachControl(this.PART_TextBox);

        this.PART_TextBox.KeyDown += this.OnTextFieldKeyDown;
    }

    private void OnTextFieldKeyDown(object? sender, KeyEventArgs e) {
        if ((e.Key == Key.Escape || e.Key == Key.Enter) && this.myDialog != null) {
            this.myDialog.TryCloseDialog(e.Key != Key.Escape);
        }
    }

    public void Connect(UserInputDialog dialog, UserInputInfo info) {
        this.myDialog = dialog;
        this.myData = (SingleUserInputInfo) info;
        this.labelBinder.AttachModel(this.myData);
        this.textBinder.AttachModel(this.myData);
        SingleUserInputInfo.TextParameter.AddValueChangedHandler(info, this.OnTextChanged);
        SingleUserInputInfo.LabelParameter.AddValueChangedHandler(info, this.OnLabelChanged);

        this.myData.AllowEmptyTextChanged += this.OnAllowEmptyTextChanged;
        this.UpdateLabelVisibility();
    }

    public void Disconnect() {
        this.labelBinder.DetachModel();
        this.textBinder.DetachModel();
        SingleUserInputInfo.TextParameter.RemoveValueChangedHandler(this.myData!, this.OnTextChanged);
        SingleUserInputInfo.LabelParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelChanged);

        this.myData!.AllowEmptyTextChanged -= this.OnAllowEmptyTextChanged;
        this.myDialog = null;
        this.myData = null;
    }

    public bool FocusPrimaryInput() {
        this.PART_TextBox.Focus();
        this.PART_TextBox.SelectAll();
        return true;
    }

    private void UpdateLabelVisibility() => this.PART_Label.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.Label);

    private void OnLabelChanged(DataParameter parameter, ITransferableData owner) => this.UpdateLabelVisibility();

    private void OnAllowEmptyTextChanged(SingleUserInputInfo sender) => this.myDialog!.InvalidateConfirmButton();

    private void OnTextChanged(DataParameter parameter, ITransferableData owner) => this.myDialog!.InvalidateConfirmButton();
}