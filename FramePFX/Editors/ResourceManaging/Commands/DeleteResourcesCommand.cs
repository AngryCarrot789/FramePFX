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

namespace FramePFX.Editors.ResourceManaging.Commands
{
    public class DeleteResourcesCommand : Command
    {
        public override Executability CanExecute(CommandEventArgs e)
        {
            return ResourceContextRegistry.CanGetTreeSelectionContext(e.ContextData);
        }

        protected override void Execute(CommandEventArgs e)
        {
            if (!ResourceContextRegistry.GetTreeSelectionContext(e.ContextData, out BaseResource[] items))
                return;

            foreach (BaseResource item in items)
            {
                // Since the tree's selected items will be unordered (hash set), we might end up removing
                // a folder containing some selected items, so parent will be null since it deletes the hierarchy
                if (item.Parent == null)
                    continue;

                ResourceFolder.ClearHierarchy(item as ResourceFolder);
                item.Parent.RemoveItem(item);
            }
        }
    }
}