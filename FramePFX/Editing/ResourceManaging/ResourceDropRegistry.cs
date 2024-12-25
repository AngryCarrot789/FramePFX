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
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Editing.ResourceManaging;

public class ResourceDropRegistry {
    public const string DropTypeText = "PFXResManResources_DropType";

    public static DragDropRegistry<TreePath> TreeDropRegistry { get; } = new DragDropRegistry<TreePath>();

    static ResourceDropRegistry() {
    }

    public static EnumDropType CanDropNativeTypeIntoTreeOrNode(IResourceTreeElement tree, IResourceTreeNodeElement? node, IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        if (node != null && !(node.Resource is ResourceFolder)) {
            return EnumDropType.None;
        }

        return CanDropNativeType(obj, ctx, inputDropType);
    }

    public static EnumDropType CanDropNativeTypeIntoListOrItem(IResourceListElement list, IResourceListItemElement? item, IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        return CanDropNativeType(obj, ctx, inputDropType);
    }

    public static EnumDropType CanDropNativeType(IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        if (obj.Contains(NativeDropTypes.Files)) {
            return EnumDropType.Copy;
        }

        return EnumDropType.None;
    }

    public static Task<bool> OnDropNativeTypeIntoTreeOrNode(IResourceTreeElement tree, IResourceTreeNodeElement? node, IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        if (node != null && !(node.Resource is ResourceFolder)) {
            return Task.FromResult<bool>(false);
        }

        ResourceFolder folder = node != null ? (ResourceFolder) node.Resource! : tree.ManagerUI.ResourceManager!.RootContainer;
        return OnDropNativeType(folder, obj, ctx, inputDropType);
    }

    public static Task<bool> OnDropNativeTypeIntoListOrItem(IResourceListElement list, IResourceListItemElement? item, IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        if (item != null && !(item.Resource is ResourceFolder)) {
            return Task.FromResult<bool>(false);
        }

        ResourceFolder folder = item != null ? (ResourceFolder) item.Resource! : ((ResourceFolder?) list.CurrentFolderNode?.Resource ?? list.ManagerUI.ResourceManager!.RootContainer);
        return OnDropNativeType(folder, obj, ctx, inputDropType);
    }

    public static async Task<bool> OnDropNativeType(ResourceFolder folder, IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        if (!(obj.GetData(NativeDropTypes.Files) is string[] files) || files.Length < 1) {
            return false;
        }

        List<BaseResource> resources = new List<BaseResource>();
        foreach (string path in files) {
            switch (Path.GetExtension(path).ToLower()) {
                case ".gif":
                case ".mp3":
                case ".wav":
                case ".ogg":
                case ".mp4":
                case ".wmv":
                case ".avi":
                case ".avchd":
                case ".f4v":
                case ".swf":
                case ".mov":
                case ".mkv":
                case ".qt":
                case ".webm":
                case ".flv": {
                    ResourceAVMedia media = new ResourceAVMedia() {
                        FilePath = path, DisplayName = Path.GetFileName(path)
                    };

                    resources.Add(media);
                    folder.AddItem(media);
                    break;
                }
                case ".png":
                case ".bmp":
                case ".jpg":
                case ".jpeg": {
                    ResourceImage image = new ResourceImage() { FilePath = path, DisplayName = Path.GetFileName(path) };
                    resources.Add(image);
                    folder.AddItem(image);
                    break;
                }
            }
        }

        if (!await IResourceLoaderDialogService.Instance.TryLoadResources(resources.ToArray())) {
            foreach (BaseResource res in resources) {
                res.Destroy();
                res.Parent!.RemoveItem(res);
            }
        }

        return true;
    }

    public static bool CanDropResourceListIntoFolder(ResourceFolder folder, List<BaseResource> droppedItems, EnumDropType dropType) {
        // ResourceFolder? myParent = this.Resource.Parent;
        // if (myParent == null || (!myParent.IsRoot && droppedItems.Any(x => x is ResourceFolder cl && cl.Parent != null && cl.Parent.IsParentInHierarchy(cl)))) {
        //     return;
        // }

        if (!folder.IsRoot && droppedItems.Any(x => x is ResourceFolder cl && cl.Parent != null && cl.Parent.IsParentInHierarchy(cl))) {
            // We can't drop into the folder since we would create an infinite loop
            return false;
        }

        return true;
    }

    public static async Task OnDropResourceListIntoTreeOrNode(IResourceTreeElement tree, IResourceTreeNodeElement? node, List<BaseResource> droppedItems, IContextData ctx, EnumDropType dropType) {
        if (node != null) {
            if (node.Resource is ResourceFolder resourceFolder) {
                await OnDropResourceList(resourceFolder, droppedItems, dropType);
            }
        }
        else {
            await OnDropResourceList(tree.ManagerUI.ResourceManager!.RootContainer, droppedItems, dropType);
        }
    }

    public static async Task OnDropResourceListIntoListItem(IResourceListElement list, IResourceListItemElement? item, List<BaseResource> droppedItems, IContextData ctx, EnumDropType dropType) {
        ResourceFolder? destinationResource;
        if (item != null) {
            destinationResource = item.Resource as ResourceFolder;
        }
        else {
            destinationResource = list.CurrentFolderItem?.Resource as ResourceFolder ?? list.ManagerUI.ResourceManager!.RootContainer;
        }

        if (destinationResource != null) {
            await OnDropResourceList(destinationResource, droppedItems, dropType);
        }
    }

    private static async Task OnDropResourceList(ResourceFolder destination, List<BaseResource> droppedItems, EnumDropType dropType) {
        if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
            return;
        }

        List<BaseResource>? cloned = dropType == EnumDropType.Copy ? new List<BaseResource>() : null;
        foreach (BaseResource res in droppedItems) {
            if (res is ResourceFolder composition && composition.IsParentInHierarchy(destination)) {
                continue;
            }

            if (dropType == EnumDropType.Copy) {
                BaseResource clone = BaseResource.Clone(res);
                if (!TextIncrement.GetIncrementableString((s => true), clone.DisplayName, out string? name, canAcceptInitialInput: false))
                    name = clone.DisplayName;
                clone.DisplayName = name;
                destination.AddItem(clone);
                cloned!.Add(clone);
            }
            else if (res.Parent != null) {
                res.Parent.MoveItemTo(destination, res);
            }
            else {
                Debug.Assert(false, "No parent");
                // AppLogger.Instance.WriteLine("A resource was dropped with a null parent???");
            }
        }

        if (dropType == EnumDropType.Copy) {
            await IResourceLoaderDialogService.Instance.TryLoadResources(cloned!.ToArray());
        }
    }
}