using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Interactivity;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Resources {
    public static class ResourceDropRegistry {
        public static DragDropRegistry<BaseResource> DropRegistry { get; }

        static ResourceDropRegistry() {
            DropRegistry = new DragDropRegistry<BaseResource>();

            DropRegistry.Register<ResourceFolder, List<BaseResource>>((target, items, dropType, c) => {
                if (dropType == EnumDropType.None || dropType == EnumDropType.Link) {
                    return EnumDropType.None;
                }

                if (items.Count == 1) {
                    BaseResource item = items[0];
                    if (item is ResourceFolder folder && folder.IsParentInHierarchy(target)) {
                        return EnumDropType.None;
                    }
                    else if (dropType != EnumDropType.Copy) {
                        if (target.Items.Contains(item)) {
                            return EnumDropType.None;
                        }
                    }
                }

                return dropType;
            }, (folder, resources, dropType, c) => {
                if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
                    return Task.CompletedTask;
                }

                foreach (BaseResource resource in resources) {
                    if (resource is ResourceFolder group && group.IsParentInHierarchy(folder)) {
                        continue;
                    }

                    if (dropType == EnumDropType.Copy) {
                        BaseResource clone = BaseResource.Clone(resource);
                        if (!TextIncrement.GetIncrementableString(folder.IsNameFree, clone.DisplayName, out string name))
                            name = clone.DisplayName;
                        clone.DisplayName = name;
                        folder.AddItem(clone);
                    }
                    else if (resource.Parent != null) {
                        if (resource.Parent != folder) {
                            // drag dropped a resource into the same folder
                            resource.Parent.MoveItemTo(folder, resource);
                        }
                    }
                    else {
                        // ???
                        AppLogger.Instance.WriteLine("A resource was dropped with a null parent???");
                    }
                }

                return Task.CompletedTask;
            });
        }
    }
}