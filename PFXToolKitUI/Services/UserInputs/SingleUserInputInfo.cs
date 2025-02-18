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

using System.Collections.Immutable;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.Services.UserInputs;

public delegate void SingleUserInputDataEventHandler(SingleUserInputInfo sender);

public class SingleUserInputInfo : BaseTextUserInputInfo {
    public static readonly DataParameterString TextParameter = DataParameter.Register(new DataParameterString(typeof(SingleUserInputInfo), nameof(Text), "", ValueAccessors.Reflective<string?>(typeof(SingleUserInputInfo), nameof(text)), isNullable: false));
    public static readonly DataParameterString LabelParameter = DataParameter.Register(new DataParameterString(typeof(SingleUserInputInfo), nameof(Label), null, ValueAccessors.Reflective<string?>(typeof(SingleUserInputInfo), nameof(label))));

    private string text;
    private string? label = LabelParameter.DefaultValue;
    private IImmutableList<string>? textErrors;

    /// <summary>
    /// Gets the value the user have typed into the text field
    /// </summary>
    public string Text {
        get => this.text;
        set => DataParameter.SetValueHelper<string?>(this, TextParameter, ref this.text!, value ?? "");
    }

    /// <summary>
    /// Gets the label placed right above the text field
    /// </summary>
    public string? Label {
        get => this.label;
        set => DataParameter.SetValueHelper(this, LabelParameter, ref this.label, value);
    }

    /// <summary>
    /// A validation function that is given the current text and a list. If there's problems
    /// with the text, then error messages should be added to the list. 
    /// </summary>
    public Action<string, List<string>>? Validate { get; set; }

    /// <summary>
    /// Gets the current list of errors present. This value will either be null, or it will have at least one element
    /// </summary>
    public IImmutableList<string>? TextErrors {
        get => this.textErrors;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(this.textErrors, value)) {
                this.textErrors = value;
                this.TextErrorsChanged?.Invoke(this);
                this.RaiseHasErrorsChanged();
            }
        }
    }

    public event SingleUserInputDataEventHandler? TextErrorsChanged;

    public SingleUserInputInfo(string? defaultText) {
        this.text = defaultText ?? "";
    }

    public SingleUserInputInfo(string? caption, string? label, string? defaultText) : base(caption, null) {
        this.label = label;
        this.text = defaultText ?? "";
    }

    public SingleUserInputInfo(string? caption, string? message, string? label, string? defaultText) : base(caption, message) {
        this.label = label;
        this.text = defaultText ?? "";
    }

    static SingleUserInputInfo() {
        TextParameter.ValueChanged += (p, o) => ((SingleUserInputInfo) o).UpdateTextError();
    }

    private void UpdateTextError() {
        this.TextErrors = GetErrors(this.Text, this.Validate);
    }

    public override bool HasErrors() => this.TextErrors != null;

    public override void UpdateAllErrors() {
        this.UpdateTextError();
    }

    public static ImmutableList<string>? GetErrors(string text, Action<string, List<string>>? validate) {
        if (validate == null) {
            return null;
        }

        List<string> list = new List<string>();
        validate(text, list);
        return list.Count > 0 ? list.ToImmutableList() : null;
    }
}