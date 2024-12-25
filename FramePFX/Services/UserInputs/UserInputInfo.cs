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

using FramePFX.DataTransfer;
using FramePFX.Utils.Accessing;

namespace FramePFX.Services.UserInputs;

/// <summary>
/// The base class for a user input dialog's model, which contains generic
/// properties suitable across any type of two-buttoned titlebar and message dialog
/// </summary>
public abstract class UserInputInfo : ITransferableData {
    public static readonly DataParameterString CaptionParameter = DataParameter.Register(new DataParameterString(typeof(UserInputInfo), nameof(Caption), null, ValueAccessors.Reflective<string?>(typeof(UserInputInfo), nameof(caption))));
    public static readonly DataParameterString MessageParameter = DataParameter.Register(new DataParameterString(typeof(UserInputInfo), nameof(Message), null, ValueAccessors.Reflective<string?>(typeof(UserInputInfo), nameof(message))));
    public static readonly DataParameterString ConfirmTextParameter = DataParameter.Register(new DataParameterString(typeof(UserInputInfo), nameof(ConfirmText), "OK", ValueAccessors.Reflective<string?>(typeof(UserInputInfo), nameof(confirmText))));
    public static readonly DataParameterString CancelTextParameter = DataParameter.Register(new DataParameterString(typeof(UserInputInfo), nameof(CancelText), "Cancel", ValueAccessors.Reflective<string?>(typeof(UserInputInfo), nameof(cancelText))));

    private string? caption = CaptionParameter.DefaultValue;
    private string? message = MessageParameter.DefaultValue;
    private string? confirmText = ConfirmTextParameter.DefaultValue;
    private string? cancelText = CancelTextParameter.DefaultValue;

    public TransferableData TransferableData { get; }

    /// <summary>
    /// Gets or sets the dialog's caption, displayed usually in the titlebar
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => DataParameter.SetValueHelper(this, CaptionParameter, ref this.caption, value);
    }

    /// <summary>
    /// Gets or sets the dialog's message, displayed above the input field(s) at the top of the dialog's content,
    /// typically in bolder text. This could be some general information about what the fields do or maybe some rules.
    /// See derived classes for properties such as labels or field descriptions, which may be more specific
    /// </summary>
    public string? Message {
        get => this.message;
        set => DataParameter.SetValueHelper(this, MessageParameter, ref this.message, value);
    }

    /// <summary>
    /// Gets or sets the text in the confirm button
    /// </summary>
    public string? ConfirmText {
        get => this.confirmText;
        set => DataParameter.SetValueHelper(this, ConfirmTextParameter, ref this.confirmText, value);
    }

    /// <summary>
    /// Gets or sets the text in the cancel button
    /// </summary>
    public string? CancelText {
        get => this.cancelText;
        set => DataParameter.SetValueHelper(this, CancelTextParameter, ref this.cancelText, value);
    }

    /// <summary>
    /// Gets or sets the button that is focused by default. True for the confirm
    /// button, False for the cancel button, null for none. Null is default
    /// </summary>
    public bool? DefaultButton { get; init; }

    public UserInputInfo() {
        this.TransferableData = new TransferableData(this);
    }

    public UserInputInfo(string? caption, string? message) : this() {
        this.caption = caption;
        this.message = message;
    }

    public abstract bool CanDialogClose();
}