using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Interactivity;
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

                List<BaseResource> loadList = new List<BaseResource>();
                foreach (BaseResource resource in resources) {
                    if (resource is ResourceFolder group && group.IsParentInHierarchy(folder)) {
                        continue;
                    }

                    if (dropType == EnumDropType.Copy) {
                        BaseResource clone = BaseResource.CloneAndRegister(resource);
                        if (!TextIncrement.GetIncrementableString(folder.IsNameFree, clone.DisplayName, out string name))
                            name = clone.DisplayName;
                        clone.DisplayName = name;
                        folder.AddItem(clone);
                        loadList.Add(clone);
                    }
                    else if (resource.Parent != null) {
                        if (resource.Parent != folder) {
                            // might drag drop a resource in the same group
                            resource.Parent.MoveItemTo(folder, resource);
                            loadList.Add(resource);
                        }
                    }
                    else {
                        if (resource is ResourceItem item && item.Manager != null && !item.IsRegistered()) {
                            item.Manager.RegisterEntry(item);
                            Debug.WriteLine("Unexpected unregistered item dropped\n" + new StackTrace(true));
                            Debugger.Break();
                        }

                        folder.AddItem(resource);
                        loadList.Add(resource);
                    }
                }

                return Task.CompletedTask;
            });
        }
    }
}