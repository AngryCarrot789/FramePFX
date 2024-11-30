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

using System.Threading.Tasks;
using Avalonia.Controls;
using FramePFX.Services.UserInputs;
using FramePFX.Utils;
using UserInputDialog = FramePFX.Avalonia.Services.Messages.Controls.UserInputDialog;

namespace FramePFX.Avalonia.Services;

public class InputDialogServiceImpl : IUserInputDialogService {
    public Task<bool?> ShowInputDialogAsync(SingleUserInputInfo info) {
        return ShowDialogAsync(info);
    }

    public Task<bool?> ShowInputDialogAsync(DoubleUserInputInfo info) {
        return ShowDialogAsync(info);
    }

    public static async Task<bool?> ShowDialogAsync(UserInputInfo info) {
        Validate.NotNull(info);

        if (App.RZApplicationImpl.TryGetActiveWindow(out Window? window)) {
            UserInputDialog dialog = new UserInputDialog {
                UserInputData = info
            };

            bool? result = await dialog.ShowDialog<bool?>(window);
            if (result == true && dialog.DialogResult == true) {
                return true;
            }

            return result;   
        }

        return null;
    }
}