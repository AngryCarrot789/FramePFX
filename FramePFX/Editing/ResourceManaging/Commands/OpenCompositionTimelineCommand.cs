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

using FramePFX.Editing.ResourceManaging.Resources;
using PFXToolKitUI.CommandSystem;

namespace FramePFX.Editing.ResourceManaging.Commands;

public class OpenCompositionTimelineCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (DataKeys.ResourceObjectKey.TryGetContext(e.ContextData, out BaseResource? resource)) {
            if (resource.Manager == null)
                return Executability.Invalid;

            if (!(resource is ResourceComposition composition) || resource.Manager.Project.ActiveTimeline == composition.Timeline)
                return Executability.ValidButCannotExecute;

            return Executability.Valid;
        }

        return Executability.Invalid;
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (DataKeys.ResourceObjectKey.TryGetContext(e.ContextData, out BaseResource? resource)) {
            if (resource.Manager != null && resource is ResourceComposition composition) {
                resource.Manager.Project.ActiveTimeline = composition.Timeline;
            }
        }

        return Task.CompletedTask;
    }
}