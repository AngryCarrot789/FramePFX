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

using System.Threading.Tasks;
using FramePFX.Avalonia.Services;
using FramePFX.Services.InputStrokes;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.Avalonia.Shortcuts.Dialogs;

public class InputStrokeDialogsImpl : IInputStrokeQueryDialogService {
    public async Task<KeyStroke?> ShowGetKeyStrokeDialog(KeyStroke? keyStroke) {
        KeyStrokeUserInputInfo info = new KeyStrokeUserInputInfo() {
            KeyStroke = keyStroke, Caption = "Key Input Stroke"
        };

        return await InputDialogServiceImpl.ShowDialogAsync(info) == true ? info.KeyStroke : default;
    }

    public async Task<MouseStroke?> ShowGetMouseStrokeDialog(MouseStroke? mouseStroke) {
        MouseStrokeUserInputInfo info = new MouseStrokeUserInputInfo() {
            MouseStroke = mouseStroke, Caption = "Mouse Input Stroke"
        };

        return await InputDialogServiceImpl.ShowDialogAsync(info) == true ? info.MouseStroke : default;
    }
}