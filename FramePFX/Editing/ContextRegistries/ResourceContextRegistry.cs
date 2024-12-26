// 
// Copyright (c) 2024-2024 REghZy
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

using System.Text;
using FramePFX.AdvancedMenuService;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Commands;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.ContextRegistries;

public class ResourceContextRegistry {
    public static readonly ContextRegistry ResourceItemContextRegistry;
    public static readonly ContextRegistry ResourceFolderContextRegistry;
    public static readonly ContextRegistry ResourceSurfaceContextRegistry;

    static ResourceContextRegistry() {
        // For the ListBox and TreeView
        ResourceSurfaceContextRegistry = new ContextRegistry("Resource Manager");

        // For ResourceFolder only
        ResourceFolderContextRegistry = new ContextRegistry("Resource Folder(s)");
        ResourceFolderContextRegistry.Opened += (r, ctx) => {
            int selected;
            if (DataKeys.ResourceListUIKey.TryGetContext(ctx, out IResourceListElement? list))
                selected = list.Selection.SelectedItems.Count(x => x is ResourceFolder);
            else if (DataKeys.ResourceTreeUIKey.TryGetContext(ctx, out IResourceTreeElement? tree))
                selected = tree.Selection.SelectedItems.Count(x => x is ResourceFolder);
            else
                return;

            if (selected > 0)
                r.Caption = selected == 1 ? "1 Folder" : $"{selected} Folders";
        };

        ResourceFolderContextRegistry.Closed += (r) => r.Caption = "Resource Folder(s)";

        // For ResourceItem only
        ResourceItemContextRegistry = new ContextRegistry("Resource Item(s)");
        ResourceItemContextRegistry.Opened += (r, ctx) => {
            List<BaseResource> selected;
            if (DataKeys.ResourceListUIKey.TryGetContext(ctx, out IResourceListElement? list))
                selected = list.Selection.SelectedItems.ToList();
            else if (DataKeys.ResourceTreeUIKey.TryGetContext(ctx, out IResourceTreeElement? tree))
                selected = tree.Selection.SelectedItems.ToList();
            else
                return;

            if (selected.Count == 1) {
                string? name = selected[0].DisplayName;
                if (string.IsNullOrWhiteSpace(name))
                    name = null;

                string typeName = selected[0].GetType().Name;
                r.Caption = name != null ? $"{typeName} ({name})" : typeName;
            }
            else {
                int fC = selected.Count(x => x is ResourceFolder);
                int iC = selected.Count(x => x is ResourceItem);
                StringBuilder sb = new StringBuilder();
                if (fC > 0)
                    sb.Append(fC).Append(" Folder").Append(fC == 1 ? "" : "s");
                if (iC > 0)
                    (fC > 0 ? sb.Append(", ") : sb).Append(iC).Append(" Resource").Append(fC == 1 ? "" : "s");
                r.Caption = sb.ToString();
            }
        };

        ResourceItemContextRegistry.Closed += r => r.Caption = "Resource Item(s)";

        ApplyModifyGeneral(ResourceItemContextRegistry.GetFixedGroup("modify.general"));
        ApplyModifyGeneral(ResourceFolderContextRegistry.GetFixedGroup("modify.general"));
        ApplyNewItemEntries(ResourceSurfaceContextRegistry.GetFixedGroup("modify.subcreation"));
        ApplyNewItemEntries(ResourceFolderContextRegistry.GetFixedGroup("modify.subcreation"));

        ResourceItemContextRegistry.GetFixedGroup("modify.general").AddDynamicSubGroup((group, ctx, items) => {
            if (ResourceCommandUtils.GetSingleItem(ctx, out BaseResource? resource)) {
                switch (resource) {
                    case ResourceColour:      items.Add(new CommandContextEntry("commands.resources.ChangeResourceColour", "Change Colour", "Change the colour of the resource")); break;
                    case ResourceComposition: items.Add(new CommandContextEntry("commands.editor.OpenCompositionTimeline", "Open Timeline", "Opens this composition resource's timeline in the editor")); break;
                }
            }
        });

        const string groupCmd = "commands.resources.GroupResources";
        ResourceSurfaceContextRegistry.GetFixedGroup("modify.general").AddCommand(groupCmd, "Group Item(s)");
        ResourceItemContextRegistry.GetFixedGroup("modify.general").AddCommand(groupCmd, "Group Item(s)");
        ResourceFolderContextRegistry.GetFixedGroup("modify.general").AddCommand(groupCmd, "Group Item(s)");

        ResourceItemContextRegistry.CreateDynamicGroup("ModifyOnlineStates", (g, ctx, items) => {
            if (!ToggleOnlineStateCommand.GetTargetItems(ctx, out List<ResourceItem>? list)) {
                return;
            }

            items.Add(new CaptionEntry("Modify Online State"));
            if (list.Count == 1) {
                if (list[0].IsOnline) {
                    items.Add(new CommandContextEntry("commands.resources.SetResourcesOffline", "Set Offline", "Set the selected resources offline"));
                }
                else {
                    items.Add(new CommandContextEntry("commands.resources.SetResourcesOnline", "Set Online", "Set the selected resources online"));
                }
            }
            else {
                items.Add(new CommandContextEntry("commands.resources.SetResourcesOnline", "Set Online", "Set the selected resources online"));
                items.Add(new CommandContextEntry("commands.resources.SetResourcesOffline", "Set Offline", "Set the selected resources offline"));
                items.Add(new CommandContextEntry("commands.resources.ToggleOnlineState", "Toggle Online", "Toggles the online state of the selected resources"));
            }
        });

        ApplyModifyDestruction(ResourceItemContextRegistry.GetFixedGroup("modify.destruction", 100000));
        ApplyModifyDestruction(ResourceFolderContextRegistry.GetFixedGroup("modify.destruction", 100000));
        return;

        static void ApplyNewItemEntries(FixedContextGroup g) {
            g.AddHeader("Create Resources");
            g.AddEntry(new CommandContextEntry("commands.resources.AddResourceImage", "Add Image", "Create a new image resource"));
            g.AddEntry(new CommandContextEntry("commands.resources.AddResourceAVMedia", "Add Media", "Create a new media resource"));
            g.AddEntry(new CommandContextEntry("commands.resources.AddResourceColour", "Add Colour", "Create a new colour resource"));
            g.AddEntry(new CommandContextEntry("commands.resources.AddResourceComposition", "Add Composition Timeline", "Create a composition timeline new resource"));
        }

        static void ApplyModifyGeneral(FixedContextGroup g) {
            g.AddHeader("General");
            g.AddCommand("commands.resources.RenameResource", "Rename", "Rename this resource");
        }

        static void ApplyModifyDestruction(FixedContextGroup g) {
            g.AddCommand("commands.resources.DeleteResources", "Delete", "Delete this/these resource(s)");
        }
    }
}