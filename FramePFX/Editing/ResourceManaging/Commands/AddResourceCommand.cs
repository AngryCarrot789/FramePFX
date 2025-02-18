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

using FramePFX.Editing.Factories;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.ResourceManaging.UI;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Services.FilePicking;
using PFXToolKitUI.Utils;
using SkiaSharp;

namespace FramePFX.Editing.ResourceManaging.Commands;

public abstract class AddResourceCommand<T> : Command where T : BaseResource {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return DataKeys.ResourceManagerUIKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!DataKeys.ResourceManagerUIKey.TryGetContext(e.ContextData, out IResourceManagerElement? manager)) {
            return;
        }

        ResourceManager resMan = manager.ResourceManager!;
        IResourceTreeNodeElement? current = manager.List.CurrentFolderTreeNode;
        ResourceFolder targetFolder = (ResourceFolder) (current?.Resource ?? resMan.RootContainer);

        T resource = this.NewInstance();
        await this.OnPreAddToFolder(targetFolder, resource, current, e.ContextData);

        // Is this way too over secure???

        using ErrorList list = new ErrorList("Exception adding resource to track");

        bool success = false;
        try {
            targetFolder.AddItem(resource);
            success = true;
        }
        catch (Exception ex) {
            list.Add(ex);
        }
        finally {
            try {
                await this.OnPostAddToFolder(targetFolder, resource, current, !success ? null : manager.GetTreeNode(resource), success, e.ContextData);
            }
            catch (Exception ex) {
                list.Add(ex);
            }
        }
    }

    protected virtual T NewInstance() {
        return (T) ResourceTypeFactory.Instance.NewResource(ResourceTypeFactory.Instance.GetId(typeof(T)));
    }

    protected virtual Task OnPreAddToFolder(ResourceFolder folder, T resource, IResourceTreeNodeElement? folderUI, IContextData ctx) {
        return Task.CompletedTask;
    }

    protected virtual Task OnPostAddToFolder(ResourceFolder folder, T resource, IResourceTreeNodeElement? folderUI, IResourceTreeNodeElement? resourceUI, bool success, IContextData ctx) {
        return Task.CompletedTask;
    }
}

public class AddResourceImageCommand : AddResourceCommand<ResourceImage> {
    protected override async Task OnPostAddToFolder(ResourceFolder folder, ResourceImage resource, IResourceTreeNodeElement? folderUI, IResourceTreeNodeElement? resourceUI, bool success, IContextData ctx) {
        if (!success)
            return;

        string? path = await IFilePickDialogService.Instance.OpenFile("Open an image file", Filters.CombinedImageTypesAndAll);
        if (path != null) {
            resource.FilePath = path;
            resource.DisplayName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(resource.DisplayName))
                resource.DisplayName = "New Image";

            await IResourceLoaderDialogService.Instance.TryLoadResource(resource);
        }
    }
}

public class AddResourceColourCommand : AddResourceCommand<ResourceColour> {
    protected override async Task OnPostAddToFolder(ResourceFolder folder, ResourceColour resource, IResourceTreeNodeElement? folderUI, IResourceTreeNodeElement? resourceUI, bool success, IContextData ctx) {
        if (!success)
            return;

        SKColor? colour = await IColourPickerDialogService.Instance.PickColourAsync(SKColors.DodgerBlue);
        if (colour.HasValue) {
            resource.Colour = colour.Value;
        }
    }
}

public class AddResourceCompositionCommand : AddResourceCommand<ResourceComposition>;