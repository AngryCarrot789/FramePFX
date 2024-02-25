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
using FramePFX.CommandSystem;
using FramePFX.Editors.Contextual;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public class GroupResourcesCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return ResourceContextRegistry.CanGetSingleFolderSelection(e.ContextData);
        }

        public override Task Execute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetFolderSelectionContext(e.ContextData, out ResourceFolder currFolder, out BaseResource[] items)) {
                return Task.CompletedTask;
            }

            foreach (BaseResource resource in items) {
                resource.IsSelected = false;
            }

            string displayName = "Grouped Folder";
            if (e.IsUserInitiated) {
                displayName = IoC.UserInputService.ShowSingleInputDialog("New Folder", "What do you want to call this folder?", displayName, (x) => !string.IsNullOrWhiteSpace(x)) ?? "Grouped Folder";
            }

            ResourceFolder folder = new ResourceFolder(displayName);
            currFolder.AddItem(folder);
            foreach (BaseResource resource in items) {
                currFolder.MoveItemTo(folder, resource);
            }

            return Task.CompletedTask;
        }
    }
}