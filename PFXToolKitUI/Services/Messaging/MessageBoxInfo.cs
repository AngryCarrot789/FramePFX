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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.Services.Messaging;

public delegate void MessageBoxDataButtonsChangedEventHandler(MessageBoxInfo sender);

/// <summary>
/// A class for a basic message box class with a maximum of 3 buttons; Yes/OK, No and Cancel
/// </summary>
public class MessageBoxInfo : ITransferableData {
    public static readonly DataParameterString CaptionParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxInfo), nameof(Caption), "A message here", ValueAccessors.Reflective<string?>(typeof(MessageBoxInfo), nameof(caption))));
    public static readonly DataParameterString HeaderParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxInfo), nameof(Header), null, ValueAccessors.Reflective<string?>(typeof(MessageBoxInfo), nameof(header))));
    public static readonly DataParameterString MessageParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxInfo), nameof(Message), "Message", ValueAccessors.Reflective<string?>(typeof(MessageBoxInfo), nameof(message))));
    public static readonly DataParameterString YesOkTextParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxInfo), nameof(YesOkText), "OK", ValueAccessors.Reflective<string?>(typeof(MessageBoxInfo), nameof(yesOkText))));
    public static readonly DataParameterString NoTextParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxInfo), nameof(NoText), "No", ValueAccessors.Reflective<string?>(typeof(MessageBoxInfo), nameof(noText))));
    public static readonly DataParameterString CancelTextParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxInfo), nameof(CancelText), "Cancel", ValueAccessors.Reflective<string?>(typeof(MessageBoxInfo), nameof(cancelText))));

    private string? caption = CaptionParameter.DefaultValue;
    private string? header = HeaderParameter.DefaultValue;
    private string? message = MessageParameter.DefaultValue;
    private string? yesOkText = YesOkTextParameter.DefaultValue;
    private string? noText = NoTextParameter.DefaultValue;
    private string? cancelText = CancelTextParameter.DefaultValue;
    private MessageBoxButton buttons;

    public string? Caption {
        get => this.caption;
        set => DataParameter.SetValueHelper(this, CaptionParameter, ref this.caption, value);
    }

    public string? Header {
        get => this.header;
        set => DataParameter.SetValueHelper(this, HeaderParameter, ref this.header, value);
    }

    public string? Message {
        get => this.message;
        set => DataParameter.SetValueHelper(this, MessageParameter, ref this.message, value);
    }

    public string? YesOkText {
        get => this.yesOkText;
        set => DataParameter.SetValueHelper(this, YesOkTextParameter, ref this.yesOkText, value);
    }

    public string? NoText {
        get => this.noText;
        set => DataParameter.SetValueHelper(this, NoTextParameter, ref this.noText, value);
    }

    public string? CancelText {
        get => this.cancelText;
        set => DataParameter.SetValueHelper(this, CancelTextParameter, ref this.cancelText, value);
    }

    /// <summary>
    /// Gets or sets which buttons are shown
    /// </summary>
    public MessageBoxButton Buttons {
        get => this.buttons;
        set {
            if (this.buttons == value)
                return;

            this.buttons = value;
            this.ButtonsChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets or sets the type of button to automatically focus in the UI. Default is none
    /// </summary>
    public MessageBoxResult DefaultButton { get; init; }

    public event MessageBoxDataButtonsChangedEventHandler? ButtonsChanged;

    public TransferableData TransferableData { get; }

    public MessageBoxInfo() {
        this.TransferableData = new TransferableData(this);
    }

    public MessageBoxInfo(string? message) : this() {
        this.message = message;
    }

    public MessageBoxInfo(string? caption, string? message) : this() {
        this.caption = caption;
        this.message = message;
    }

    public MessageBoxInfo(string? caption, string? header, string? message) : this() {
        this.caption = caption;
        this.header = header;
        this.message = message;
    }

    public void SetDefaultButtonText() {
        switch (this.buttons) {
            case MessageBoxButton.OK: this.YesOkText = "OK"; break;
            case MessageBoxButton.OKCancel:
                this.YesOkText = "OK";
                this.CancelText = "Cancel";
            break;
            case MessageBoxButton.YesNoCancel:
                this.YesOkText = "Yes";
                this.NoText = "No";
                this.CancelText = "Cancel";
            break;
            case MessageBoxButton.YesNo:
                this.YesOkText = "Yes";
                this.NoText = "No";
            break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}