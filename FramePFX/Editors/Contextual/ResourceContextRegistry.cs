using FramePFX.AdvancedContextService;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Interactivity.DataContexts;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Utils;

namespace FramePFX.Editors.Contextual {
    public class ResourceContextRegistry : IContextGenerator {
        public static ResourceContextRegistry Instance { get; } = new ResourceContextRegistry();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (!context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource resource)) {
                return;
            }

            int actualSelection = resource.Manager.SelectedItems.Count;
            int itemCount = actualSelection;
            if (!resource.IsSelected)
                itemCount++;

            ActionContextEntry groupAction = null;
            if (resource.Manager != null) {
                ResourceFolder currFolder = resource.Manager.CurrentFolder;
                int groupCount = currFolder.Items.Count(x => x.IsSelected);
                if (!resource.IsSelected && currFolder.Items.Contains(resource)) {
                    groupCount++;
                }

                if (groupCount > 0) {
                    groupAction = new ActionContextEntry("actions.resources.GroupResourcesAction", groupCount == 1 ? "Group into folder" : $"Group {groupCount} items into folder", "Groups all selected items in the explorer into a folder. Grouping items in the tree is currently unsupported");
                }
            }

            if (itemCount == 1) {
                list.Add(new ActionContextEntry("actions.resources.RenameResourceAction", "Rename resource"));
                if (groupAction != null) {
                    list.Add(groupAction);
                }

                if (resource is ResourceItem item) {
                    list.Add(new SeparatorEntry());
                    if (item.IsOnline) {
                        list.Add(new EventContextEntry(this.DisableResources, "Set Offline"));
                    }
                    else {
                        list.Add(new EventContextEntry(this.EnableResources, "Set Online"));
                    }
                }
            }
            else {
                if (groupAction != null) {
                    list.Add(groupAction);
                }

                list.Add(new EventContextEntry(this.EnableResources, $"Set {itemCount} items Online"));
                list.Add(new EventContextEntry(this.DisableResources, $"Set {itemCount} items Offline"));
            }

            list.Add(new SeparatorEntry());
            list.Add(new ActionContextEntry("actions.resources.DeleteResourcesAction", itemCount == 1 ? "Delete Resource" : $"Delete {itemCount} Resources"));
        }

        private void DeleteResources(IDataContext context) {
            if (!context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource resource)) {
                return;
            }

            HashSet<BaseResource> resources = new HashSet<BaseResource>(resource.Manager.SelectedItems);
            if (!resource.IsSelected)
                resources.Add(resource);

            foreach (BaseResource item in resources) {
                // since it's a hash set, we might end up removing a folder containing some
                // selected items, so parent will be null since it deletes the hierarchy
                item.Parent?.RemoveItem(item);
                ResourceFolder.DestroyHierarchy(item);
            }
        }

        private void EnableResources(IDataContext context) {
            if (!context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource focusedResource)) {
                return;
            }

            HashSet<BaseResource> resources = new HashSet<BaseResource>(focusedResource.Manager.SelectedItems);
            if (!focusedResource.IsSelected)
                resources.Add(focusedResource);

            ResourceLoaderDialog.TryLoadResources(resources);
        }

        private void DisableResources(IDataContext context) {
            if (!context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource focusedResource)) {
                return;
            }

            HashSet<BaseResource> resources = new HashSet<BaseResource>(focusedResource.Manager.SelectedItems);
            if (!focusedResource.IsSelected)
                resources.Add(focusedResource);

            SetHierarchyOnlineState(resources, false, null);
        }

        private static void SetHierarchyOnlineState(IEnumerable<BaseResource> resources, bool state, ResourceLoader loader) {
            foreach (BaseResource obj in resources) {
                if (obj is ResourceFolder folder) {
                    SetHierarchyOnlineState(folder.Items, state, loader);
                }
                else {
                    ResourceItem item = (ResourceItem)obj;
                    if (item.IsOnline != state) {
                        if (state) {
                            item.TryAutoEnable(loader);
                        }
                        else {
                            item.Disable(true);
                        }
                    }
                }
            }
        }
    }
}