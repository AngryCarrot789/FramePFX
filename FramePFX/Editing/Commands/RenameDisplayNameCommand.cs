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
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Services.UserInputs;

namespace FramePFX.Editing.Commands;

public abstract class RenameDisplayNameCommand : AsyncCommand {
    protected override Executability CanExecuteOverride(CommandEventArgs e) {
        return e.ContextData.TryGetContext(this.DataKey.Id, out object? value) && value is IDisplayName ? Executability.Valid : Executability.Invalid;
    }

    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (e.ContextData.TryGetContext(this.DataKey.Id, out object? obj) && obj is IDisplayName element) {
            SingleUserInputInfo info = new SingleUserInputInfo("Rename", this.Label, element.DisplayName) {
                ConfirmText = "Rename", DefaultButton = true
            };

            if (await IoC.UserInputService.ShowInputDialogAsync(info) == true) {
                element.DisplayName = info.Text ?? "";
            }
        }
    }

    protected abstract DataKey DataKey { get; }

    protected abstract string Label { get; }
}

public class RenameClipCommand : RenameDisplayNameCommand {
    protected override DataKey DataKey => DataKeys.ClipKey;
    protected override string Label => "Clip Name:";
}

public class RenameTrackCommand : RenameDisplayNameCommand {
    protected override DataKey DataKey => DataKeys.TrackKey;
    protected override string Label => "Track Name:";
}