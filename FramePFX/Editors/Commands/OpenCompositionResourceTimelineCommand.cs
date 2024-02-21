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
using FramePFX.Editors.Contextual;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.Commands {
    public class OpenCompositionResourceTimelineCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetSingleSelection(e.ContextData, out BaseResource resource))
                return resource == null ? ExecutabilityState.Invalid : ExecutabilityState.ValidButCannotExecute;
            // if the composition tl is already active, just say cannot execute
            if (!(resource is ResourceComposition composition) || composition.Manager.Project.ActiveTimeline == composition.Timeline)
                return ExecutabilityState.ValidButCannotExecute;
            return ExecutabilityState.Executable;
        }

        public override void Execute(CommandEventArgs e) {
            if (ResourceContextRegistry.GetSingleSelection(e.ContextData, out BaseResource resource) && resource is ResourceComposition composition) {
                composition.Manager.Project.ActiveTimeline = composition.Timeline;
            }
        }
    }
}