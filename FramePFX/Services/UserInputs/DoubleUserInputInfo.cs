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
using FramePFX.DataTransfer;
using FramePFX.Utils.Accessing;

namespace FramePFX.Services.UserInputs;

public delegate void DoubleUserInputDataEventHandler(DoubleUserInputInfo sender);

public class DoubleUserInputInfo : BaseTextUserInputInfo {
    public static readonly DataParameterString TextAParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(TextA), "", ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(textA)), isNullable:false));
    public static readonly DataParameterString TextBParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(TextB), "", ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(textB)), isNullable:false));
    public static readonly DataParameterString LabelAParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(LabelA), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(labelA))));
    public static readonly DataParameterString LabelBParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(LabelB), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(labelB))));

    private string textA = TextAParameter.DefaultValue!;
    private string textB = TextBParameter.DefaultValue!;
    private string? labelA = LabelAParameter.DefaultValue;
    private string? labelB = LabelBParameter.DefaultValue;
    private IImmutableList<string>? textErrorsA, textErrorsB;

    public string TextA {
        get => this.textA;
        set => DataParameter.SetValueHelper<string?>(this, TextAParameter, ref this.textA!, value ?? "");
    }

    public string TextB {
        get => this.textB;
        set => DataParameter.SetValueHelper<string?>(this, TextBParameter, ref this.textB!, value ?? "");
    }

    public string? LabelA {
        get => this.labelA;
        set => DataParameter.SetValueHelper(this, LabelAParameter, ref this.labelA, value);
    }

    public string? LabelB {
        get => this.labelB;
        set => DataParameter.SetValueHelper(this, LabelBParameter, ref this.labelB, value);
    }

    public Action<string, List<string>>? ValidateA { get; set; }
    
    public Action<string, List<string>>? ValidateB { get; set; }
    
    public IImmutableList<string>? TextErrorsA {
        get => this.textErrorsA;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(this.textErrorsA, value)) {
                this.textErrorsA = value;
                this.TextErrorsAChanged?.Invoke(this);
                this.RaiseHasErrorsChanged();
            }
        }
    }
    
    public IImmutableList<string>? TextErrorsB {
        get => this.textErrorsB;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(this.textErrorsB, value)) {
                this.textErrorsB = value;
                this.TextErrorsBChanged?.Invoke(this);
                this.RaiseHasErrorsChanged();
            }
        }
    }

    public event DoubleUserInputDataEventHandler? TextErrorsAChanged;
    public event DoubleUserInputDataEventHandler? TextErrorsBChanged;

    public DoubleUserInputInfo() {
    }

    public DoubleUserInputInfo(string? textA, string? textB) {
        this.textA = textA ?? "";
        this.textB = textB ?? "";
    }

    static DoubleUserInputInfo() {
        TextAParameter.ValueChanged += (p, o) => ((DoubleUserInputInfo) o).UpdateTextAError();
        TextBParameter.ValueChanged += (p, o) => ((DoubleUserInputInfo) o).UpdateTextBError();
    }

    private void UpdateTextAError() {
        this.TextErrorsA = SingleUserInputInfo.GetErrors(this.TextA, this.ValidateA);
    }

    private void UpdateTextBError() {
        this.TextErrorsB = SingleUserInputInfo.GetErrors(this.TextB, this.ValidateB);
    }

    public override bool HasErrors() => this.TextErrorsA != null || this.TextErrorsB != null;

    public override void UpdateAllErrors() {
        this.UpdateTextAError();
        this.UpdateTextBError();
    }
}