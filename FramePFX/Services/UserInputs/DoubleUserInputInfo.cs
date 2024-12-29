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

public delegate void DoubleUserInputDataEventHandler(DoubleUserInputInfo sender);

public class DoubleUserInputInfo : BaseTextUserInputInfo {
    public static readonly DataParameterString TextAParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(TextA), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(textA))));
    public static readonly DataParameterString TextBParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(TextB), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(textB))));
    public static readonly DataParameterString LabelAParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(LabelA), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(labelA))));
    public static readonly DataParameterString LabelBParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(LabelB), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(labelB))));

    private string? textA = TextAParameter.DefaultValue;
    private string? textB = TextBParameter.DefaultValue;
    private string? labelA = LabelAParameter.DefaultValue;
    private string? labelB = LabelBParameter.DefaultValue;
    private bool allowEmptyTextA;
    private bool allowEmptyTextB;
    private string? textErrorA, textErrorB;
    private Func<string?, string?>? validatorA, validatorB;

    public string? TextA {
        get => this.textA;
        set => DataParameter.SetValueHelper(this, TextAParameter, ref this.textA, value);
    }

    public string? TextB {
        get => this.textB;
        set => DataParameter.SetValueHelper(this, TextBParameter, ref this.textB, value);
    }

    public string? LabelA {
        get => this.labelA;
        set => DataParameter.SetValueHelper(this, LabelAParameter, ref this.labelA, value);
    }

    public string? LabelB {
        get => this.labelB;
        set => DataParameter.SetValueHelper(this, LabelBParameter, ref this.labelB, value);
    }

    public bool AllowEmptyTextA {
        get => this.allowEmptyTextA;
        set {
            if (this.allowEmptyTextA == value)
                return;

            this.allowEmptyTextA = value;
            this.AllowEmptyTextAChanged?.Invoke(this);
            this.UpdateTextAError();
        }
    }

    public bool AllowEmptyTextB {
        get => this.allowEmptyTextB;
        set {
            if (this.allowEmptyTextB == value)
                return;

            this.allowEmptyTextB = value;
            this.AllowEmptyTextBChanged?.Invoke(this);
            this.UpdateTextBError();
        }
    }

    /// <summary>
    /// A validation function that is invoked when the A text changes. This converts the text into an error message.
    /// This function should return a non-null value when there's an error (preventing
    /// the dialog from closing), and return null when there's no error
    /// </summary>
    public Func<string?, string?>? ValidatorA {
        get => this.validatorA;
        set {
            this.validatorA = value;
            this.UpdateTextAError();
        }
    }

    /// <summary>
    /// A validation function that is invoked when the B text changes. This converts the text into an error message.
    /// This function should return a non-null value when there's an error (preventing
    /// the dialog from closing), and return null when there's no error
    /// </summary>
    public Func<string?, string?>? ValidatorB {
        get => this.validatorB;
        set {
            this.validatorB = value;
            this.UpdateTextBError();
        }
    }

    public string? TextErrorA {
        get => this.textErrorA;
        private set {
            if (this.textErrorA == value)
                return;

            this.textErrorA = value;
            this.TextErrorAChanged?.Invoke(this);
            this.RaiseHasErrorsChanged();
        }
    }

    public string? TextErrorB {
        get => this.textErrorB;
        private set {
            if (this.textErrorB == value)
                return;

            this.textErrorB = value;
            this.TextErrorBChanged?.Invoke(this);
            this.RaiseHasErrorsChanged();
        }
    }

    public event DoubleUserInputDataEventHandler? AllowEmptyTextAChanged;
    public event DoubleUserInputDataEventHandler? AllowEmptyTextBChanged;
    public event DoubleUserInputDataEventHandler? TextErrorAChanged;
    public event DoubleUserInputDataEventHandler? TextErrorBChanged;

    public DoubleUserInputInfo() {
    }

    public DoubleUserInputInfo(string? textA, string? textB) {
        this.textA = textA;
        this.textB = textB;
    }

    static DoubleUserInputInfo() {
        TextAParameter.ValueChanged += (p, o) => ((DoubleUserInputInfo) o).UpdateTextAError();
        TextBParameter.ValueChanged += (p, o) => ((DoubleUserInputInfo) o).UpdateTextBError();
    }

    private void UpdateTextAError() {
        if (string.IsNullOrEmpty(this.TextA) && !this.AllowEmptyTextA) {
            this.TextErrorA = "Text cannot be an empty string";
        }
        else {
            this.TextErrorA = this.ValidatorA?.Invoke(this.TextA);
        }
    }

    private void UpdateTextBError() {
        if (string.IsNullOrEmpty(this.TextB) && !this.AllowEmptyTextB) {
            this.TextErrorB = "Text cannot be an empty string";
        }
        else {
            this.TextErrorB = this.ValidatorB?.Invoke(this.TextB);
        }
    }

    public override bool HasErrors() => this.textErrorA != null || this.textErrorB != null;

    public override void UpdateAllErrors() {
        this.UpdateTextAError();
        this.UpdateTextBError();
    }
}