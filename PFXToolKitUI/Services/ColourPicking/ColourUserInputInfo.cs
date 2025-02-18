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
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Utils.Accessing;
using SkiaSharp;

namespace PFXToolKitUI.Services.ColourPicking;

public class ColourUserInputInfo : UserInputInfo {
    public static readonly DataParameter<SKColor> ColourParameter = DataParameter.Register(new DataParameter<SKColor>(typeof(ColourUserInputInfo), nameof(Colour), SKColors.Empty, ValueAccessors.Reflective<SKColor>(typeof(ColourUserInputInfo), nameof(colour))));

    private SKColor colour;

    public SKColor Colour {
        get => this.colour;
        set => DataParameter.SetValueHelper(this, ColourParameter, ref this.colour, value);
    }

    public ColourUserInputInfo() {
        this.colour = ColourParameter.GetDefaultValue(this);
    }

    public override bool HasErrors() {
        return false;
    }

    public override void UpdateAllErrors() {
    }
}