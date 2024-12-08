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

public class DoubleUserInputInfo : UserInputInfo
{
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

    public string? TextA
    {
        get => this.textA;
        set => DataParameter.SetValueHelper(this, TextAParameter, ref this.textA, value);
    }

    public string? TextB
    {
        get => this.textB;
        set => DataParameter.SetValueHelper(this, TextBParameter, ref this.textB, value);
    }

    public string? LabelA
    {
        get => this.labelA;
        set => DataParameter.SetValueHelper(this, LabelAParameter, ref this.labelA, value);
    }

    public string? LabelB
    {
        get => this.labelB;
        set => DataParameter.SetValueHelper(this, LabelBParameter, ref this.labelB, value);
    }

    public bool AllowEmptyTextA
    {
        get => this.allowEmptyTextA;
        set
        {
            if (this.allowEmptyTextA == value)
                return;

            this.allowEmptyTextA = value;
            this.AllowEmptyTextAChanged?.Invoke(this);
        }
    }

    public bool AllowEmptyTextB
    {
        get => this.allowEmptyTextB;
        set
        {
            if (this.allowEmptyTextB == value)
                return;

            this.allowEmptyTextB = value;
            this.AllowEmptyTextBChanged?.Invoke(this);
        }
    }

    public event DoubleUserInputDataEventHandler? AllowEmptyTextAChanged;
    public event DoubleUserInputDataEventHandler? AllowEmptyTextBChanged;

    /// <summary>
    /// A validation predicate that is invoked when either of our text properties change, and is used to determine if the dialog can close successfully
    /// </summary>
    public Func<string?, string?, bool>? Validate { get; set; }

    public DoubleUserInputInfo() {
    }

    public DoubleUserInputInfo(string? textA, string? textB)
    {
        this.textA = textA;
        this.textB = textB;
    }

    public override bool CanDialogClose()
    {
        if ((string.IsNullOrEmpty(this.TextA) && !this.AllowEmptyTextA) || (string.IsNullOrEmpty(this.TextA) && !this.AllowEmptyTextA))
            return false;

        return this.Validate == null || this.Validate(this.TextA, this.TextB);
    }
}