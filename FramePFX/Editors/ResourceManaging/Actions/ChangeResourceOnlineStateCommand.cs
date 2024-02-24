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

using System.Collections.Generic;
using FramePFX.CommandSystem;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public abstract class ChangeResourceOnlineStateCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return ResourceContextRegistry.CanGetTreeSelectionContext(e.ContextData);
        }

        protected static void DisableHierarchy(IEnumerable<BaseResource> resources) {
            foreach (BaseResource obj in resources) {
                if (obj is ResourceFolder folder) {
                    DisableHierarchy(folder.Items);
                }
                else if (obj is ResourceItem item && item.IsOnline) {
                    item.Disable(true);
                }
            }
        }
    }

    public class EnableResourcesCommand : ChangeResourceOnlineStateCommand {
        public override void Execute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetTreeSelectionContext(e.ContextData, out BaseResource[] items)) {
                return;
            }

            ResourceLoaderDialog.TryLoadResources(items);
        }
    }

    public class DisableResourcesCommand : ChangeResourceOnlineStateCommand {
        public override void Execute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetTreeSelectionContext(e.ContextData, out BaseResource[] items)) {
                return;
            }

            DisableHierarchy(items);
        }
    }
}