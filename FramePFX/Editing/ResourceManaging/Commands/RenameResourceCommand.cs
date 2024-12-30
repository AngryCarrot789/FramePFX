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

using FramePFX.CommandSystem;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity.Contexts;
using FramePFX.Services.UserInputs;

namespace FramePFX.Editing.ResourceManaging.Commands;

public class RenameResourceCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (DataKeys.ResourceNodeUIKey.IsPresent(e.ContextData))
            return Executability.Valid;
        if (DataKeys.ResourceObjectKey.IsPresent(e.ContextData))
            return Executability.Valid;
        return Executability.Invalid;
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (DataKeys.ResourceNodeUIKey.TryGetContext(e.ContextData, out IResourceTreeNodeElement? resource)) {
            resource.EditNameState = true;
        }
        else if (DataKeys.ResourceObjectKey.TryGetContext(e.ContextData, out BaseResource? baseResource)) {
            SingleUserInputInfo info = new SingleUserInputInfo("Rename resource", "Resource name", baseResource.DisplayName) {
                ConfirmText = "Rename", DefaultButton = true
            };

            if (await IUserInputDialogService.Instance.ShowInputDialogAsync(info) == true) {
                baseResource.DisplayName = info.Text ?? "";
            }
        }
    }
}