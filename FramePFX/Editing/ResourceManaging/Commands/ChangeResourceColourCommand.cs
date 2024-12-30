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
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Services.ColourPicking;
using SkiaSharp;

namespace FramePFX.Editing.ResourceManaging.Commands;

public class ChangeResourceColourCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!ResourceCommandUtils.GetSingleItem(e.ContextData, out BaseResource? resource))
            return Executability.Invalid;

        return resource is ResourceColour ? Executability.Valid : Executability.Invalid;
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!ResourceCommandUtils.GetSingleItem(e.ContextData, out BaseResource? resource)) {
            return;
        }

        if (resource is ResourceColour resourceColour) {
            SKColor? colour = await IColourPickerDialogService.Instance.PickColourAsync(resourceColour.Colour);
            if (colour.HasValue) {
                resourceColour.Colour = colour.Value;
            }
        }
    }
}