// 
// Copyright (c) 2024-2024 REghZy
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
using FramePFX.Services.UserInputs;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Utils.Accessing;

namespace FramePFX.Services.InputStrokes;

public class KeyStrokeUserInputInfo : UserInputInfo {
    public static readonly DataParameter<KeyStroke?> KeyStrokeParameter =
        DataParameter.Register(
            new DataParameter<KeyStroke?>(
                typeof(KeyStrokeUserInputInfo),
                nameof(KeyStroke), default(KeyStroke?),
                ValueAccessors.Reflective<KeyStroke?>(typeof(KeyStrokeUserInputInfo), nameof(keyStroke))));

    private KeyStroke? keyStroke;

    public KeyStroke? KeyStroke {
        get => this.keyStroke;
        set => DataParameter.SetValueHelper(this, KeyStrokeParameter, ref this.keyStroke, value);
    }

    public KeyStrokeUserInputInfo() {
        this.keyStroke = KeyStrokeParameter.GetDefaultValue(this);
    }

    public override bool CanDialogClose() {
        return this.keyStroke.HasValue && this.keyStroke.Value != default;
    }
}