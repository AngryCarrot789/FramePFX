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
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.ResourceManaging.Commands;

public class SetResourcesOnlineCommand : AsyncCommand {
    protected override Executability CanExecuteOverride(CommandEventArgs e) {
        return DataKeys.ResourceTreeUIKey.IsPresent(e.ContextData) || DataKeys.ResourceListUIKey.IsPresent(e.ContextData) ? Executability.Valid : Executability.Invalid;
    }

    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (!ToggleOnlineStateCommand.GetTargetItems(e.ContextData, out List<ResourceItem>? items)) {
            return;
        }

        List<ResourceItem> enable = items.Where(x => !x.IsOnline).ToList();
        if (enable.Count > 0)
            await IoC.ResourceLoaderService.TryLoadResources(enable.Cast<BaseResource>().ToArray());
    }
}