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

    public static EnumDropType CanDropNativeTypeIntoList(IResourceListElement list, IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        return CanDropNativeType(obj, ctx, inputDropType);
    }

    public static EnumDropType CanDropNativeType(IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        if (obj.Contains(NativeDropTypes.Files)) {
            return (inputDropType & EnumDropType.Copy);
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

    public static Task<bool> OnDropNativeTypeIntoList(IResourceListElement list, IDataObjekt obj, IContextData ctx, EnumDropType inputDropType) {
        ResourceFolder folder = (list.CurrentFolder?.Resource as ResourceFolder) ?? (list.ManagerUI.ResourceManager!.RootContainer);
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

        if (!await IoC.ResourceLoaderService.TryLoadResources(resources.ToArray())) {
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

    public static EnumDropType CanDropResourceListIntoTreeOrNode(IResourceTreeElement tree, IResourceTreeNodeElement node, List<BaseResource> droppedItems, IContextData ctx, EnumDropType dropType) {
        // ResourceFolder? myParent = this.Resource.Parent;
        // if (myParent == null || (!myParent.IsRoot && droppedItems.Any(x => x is ResourceFolder cl && cl.Parent != null && cl.Parent.IsParentInHierarchy(cl)))) {
        //     return;
        // }
        
        if (node.Resource is ResourceFolder folder) {
            return CanDropResourceListIntoFolder(folder, droppedItems, dropType) ? dropType : EnumDropType.None;
        }

        // No other type of resource supports dropping a list of other resources into it
        return EnumDropType.None;
    }
    
    public static async Task OnDropResourceListIntoTreeOrNode(IResourceTreeElement tree, IResourceTreeNodeElement node, List<BaseResource> droppedItems, IContextData ctx, EnumDropType dropType) {
        if (node.Resource is ResourceFolder resourceFolder) {
            if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
                return;
            }

            foreach (BaseResource item in droppedItems) {
                if (item is ResourceFolder composition && composition.IsParentInHierarchy(resourceFolder)) {
                    continue;
                }

                if (dropType == EnumDropType.Copy) {
                    BaseResource clone = BaseResource.Clone(item);
                    if (!TextIncrement.GetIncrementableString((s => true), clone.DisplayName, out string name))
                        name = clone.DisplayName;
                    clone.DisplayName = name;
                    resourceFolder.AddItem(clone);
                }
                else if (item.Parent != null) {
                    if (item.Parent != resourceFolder) {
                        item.Parent.MoveItemTo(resourceFolder, item);
                    }
                }
                else {
                    Debug.Assert(false, "No parent");
                    // AppLogger.Instance.WriteLine("A resource was dropped with a null parent???");
                }
            }
        }
    }
}