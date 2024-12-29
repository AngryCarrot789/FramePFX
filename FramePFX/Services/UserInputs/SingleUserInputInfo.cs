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

public delegate void SingleUserInputDataEventHandler(SingleUserInputInfo sender);

public class SingleUserInputInfo : BaseTextUserInputInfo {
    public static readonly DataParameterString TextParameter = DataParameter.Register(new DataParameterString(typeof(SingleUserInputInfo), nameof(Text), null, ValueAccessors.Reflective<string?>(typeof(SingleUserInputInfo), nameof(text))));
    public static readonly DataParameterString LabelParameter = DataParameter.Register(new DataParameterString(typeof(SingleUserInputInfo), nameof(Label), null, ValueAccessors.Reflective<string?>(typeof(SingleUserInputInfo), nameof(label))));

    private string? text;
    private string? label = LabelParameter.DefaultValue;
    private bool allowEmptyText;
    private string? textError;

    /// <summary>
    /// Gets the value the user have typed into the text field
    /// </summary>
    public string? Text {
        get => this.text;
        set => DataParameter.SetValueHelper(this, TextParameter, ref this.text, value);
    }

    /// <summary>
    /// Gets the label placed right above the text field
    /// </summary>
    public string? Label {
        get => this.label;
        set => DataParameter.SetValueHelper(this, LabelParameter, ref this.label, value);
    }

    /// <summary>
    /// Gets or sets if the dialog can close successfully with the text property being an empty
    /// string. When this is true and the text is empty, the validation predicate is not called.
    /// However it is called if this value is false
    /// </summary>
    public bool AllowEmptyText {
        get => this.allowEmptyText;
        set {
            if (this.allowEmptyText == value)
                return;

            this.allowEmptyText = value;
            this.AllowEmptyTextChanged?.Invoke(this);
            this.UpdateTextError();
        }
    }

    public event SingleUserInputDataEventHandler? AllowEmptyTextChanged;

    /// <summary>
    /// A validation predicate that is invoked when our text changes, and is used to get an error message from
    /// the text to determine if the dialog can close successfully. This function should return a non-null value
    /// when there's an error (preventing the dialog from closing), and return null when there's no error
    /// </summary>
    public Func<string?, string?>? Validator { get; set; }
    
    public string? TextError {
        get => this.textError;
        private set {
            if (this.textError == value)
                return;

            this.textError = value;
            this.TextErrorChanged?.Invoke(this);
            this.RaiseHasErrorsChanged();
        }
    }

    public event SingleUserInputDataEventHandler? TextErrorChanged;

    public SingleUserInputInfo(string? defaultText) {
        this.text = defaultText;
    }

    public SingleUserInputInfo(string? caption, string? label, string? defaultText) : base(caption, null) {
        this.label = label;
        this.text = defaultText;
    }

    public SingleUserInputInfo(string? caption, string? message, string? label, string? defaultText) : base(caption, message) {
        this.label = label;
        this.text = defaultText;
    }

    static SingleUserInputInfo() {
        TextParameter.ValueChanged += (p, o) => ((SingleUserInputInfo) o).UpdateTextError();
    }

    private void UpdateTextError() {
        if (string.IsNullOrEmpty(this.Text) && !this.AllowEmptyText) {
            this.TextError = "Text cannot be an empty string";
        }
        else {
            this.TextError = this.Validator?.Invoke(this.Text);
        }
    }

    public override bool HasErrors() => this.TextError != null;
    
    public override void UpdateAllErrors() {
        this.UpdateTextError();
    }
}