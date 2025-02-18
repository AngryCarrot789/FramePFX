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

using System.Diagnostics;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Editing.Timelines;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Services.Messaging;

namespace FramePFX.Editing.ResourceManaging.Commands;

public class DeleteResourcesCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!DataKeys.VideoEditorKey.IsPresent(e.ContextData)) {
            return Executability.Invalid;
        }

        if (DataKeys.ResourceTreeUIKey.IsPresent(e.ContextData) || DataKeys.ResourceListUIKey.IsPresent(e.ContextData)) {
            return Executability.Valid;
        }

        return Executability.Invalid;
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor? videoEditor)) {
            return;
        }

        List<BaseResource> selection;
        if (DataKeys.ResourceTreeUIKey.TryGetContext(e.ContextData, out IResourceTreeElement? tree)) {
            selection = tree.Selection.SelectedItems.ToList();
        }
        else if (DataKeys.ResourceListUIKey.TryGetContext(e.ContextData, out IResourceListElement? list)) {
            selection = list.Selection.SelectedItems.ToList();
        }
        else {
            await IMessageDialogService.Instance.ShowMessage("Error", "Hmm... could not find any context to delete resources");
            return;
        }

        if (selection.Count < 1) {
            return;
        }

        int folders = 0, items = 0, itemReferences = 0;
        ResourceFolder.CountHierarchy(selection, ref folders, ref items, ref itemReferences);

        int grandTotal = folders + items;
        Debug.Assert(grandTotal > 0, "Impossible, almost");

        string msg = "This includes:\n"
                     + $"   - {folders} folder{(folders == 1 ? "" : "s")}\n"
                     + $"   - {items} item{(items == 1 ? "" : "s")}\n"
                     + $"   - {itemReferences} reference{(itemReferences == 1 ? "" : "s")}, probably {(itemReferences == 1 ? "a clip" : "clips")}";

        MessageBoxInfo info = new MessageBoxInfo("Delete resources", $"Are you sure you want to delete {(grandTotal == 1 ? "this 1 item" : $"these {grandTotal} items")}?", msg) {
            Buttons = MessageBoxButton.OKCancel,
            DefaultButton = MessageBoxResult.OK,
            YesOkText = "Delete"
        };

        if (await IMessageDialogService.Instance.ShowMessage(info) != MessageBoxResult.OK) {
            return;
        }

        bool continuePlaying = false;

        if (videoEditor.Playback.PlayState == PlayState.Play) {
            videoEditor.Playback.Pause();
            continuePlaying = true;
        }

        Timeline? activeTimeline = videoEditor.Project?.ActiveTimeline;
        using (activeTimeline?.RenderManager.SuspendRenderInvalidation()) {
            if (activeTimeline != null && activeTimeline.RenderManager.IsRendering) {
                await Task.WhenAny(Task.Delay(1000), Task.Run(async () => {
                    while (activeTimeline.RenderManager.IsRendering) {
                        await Task.Delay(10);
                    }
                }));

                if (activeTimeline.RenderManager.IsRendering) {
                    await IMessageDialogService.Instance.ShowMessage("Error", "Could not pause playback");
                    if (continuePlaying) {
                        videoEditor.Playback.Play();
                    }

                    return;
                }
            }

            foreach (BaseResource item in selection) {
                // Since the tree's selected items will be unordered (hash set), we might end up removing
                // a folder containing some selected items, so parent will be null since it deletes the hierarchy
                if (item.Parent == null) {
                    continue;
                }

                ResourceFolder.ClearHierarchy(item as ResourceFolder, true);
                item.Parent.RemoveItem(item, true);
            }
        }

        if (continuePlaying) {
            videoEditor.Playback.Play();
        }
    }


    public static HashSet<ResourceFolder> FindHighestLevelFolders(List<BaseResource> resources) {
        Dictionary<ResourceFolder, List<BaseResource>> map = resources.GroupBy(r => r.Parent!).ToDictionary(g => g.Key, g => g.ToList());

        HashSet<ResourceFolder> foldersToRemove = new HashSet<ResourceFolder>();
        foreach (ResourceFolder folder in map.Keys) {
            ResourceFolder? current = folder;
            bool isCovered = false;

            // Traverse up the folder hierarchy to see if an ancestor is already marked for removal
            while (current != null) {
                if (foldersToRemove.Contains(current)) {
                    isCovered = true;
                    break;
                }

                current = current.Parent;
            }

            if (!isCovered) {
                foldersToRemove.Add(folder);
            }
        }

        return foldersToRemove;
    }
}