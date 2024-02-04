using FramePFX.AdvancedContextService;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Interactivity.DataContexts;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Utils;

namespace FramePFX.Editors.Contextual {
    public class ResourceContextRegistry : IContextGenerator {
        public static ResourceContextRegistry Instance { get; } = new ResourceContextRegistry();

        public static void GenerateNewResourceEntries(List<IContextEntry> list){
            list.Add(new EventContextEntry(AddColourResource, "Add new Colour Resource"));
        }

        private static void AddColourResource(IDataContext ctx) {
            if (ctx.TryGetContext(DataKeys.ResourceManagerKey, out ResourceManager manager)) {
                manager.RootContainer.AddItem(new ResourceColour() {Colour = RenderUtils.RandomColour(), DisplayName = "New Colour"});
            }
        }

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (!context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource resource)) {
                if (context.ContainsKey(DataKeys.ResourceManagerKey)) {
                    GenerateNewResourceEntries(list);
                }

                return;
            }
            else if (context.ContainsKey(DataKeys.ResourceManagerKey)) {
                GenerateNewResourceEntries(list);
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
                    groupAction = new ActionContextEntry("actions.resources.GroupResourcesAction", "Group item(s) into folder", "Groups all selected items in the explorer into a folder. Grouping items in the tree is currently unsupported");
                }
            }

            if (list.Count > 0) {
                list.Add(new SeparatorEntry());
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

                list.Add(new EventContextEntry(this.EnableResources, $"Set All Online"));
                list.Add(new EventContextEntry(this.DisableResources, $"Set All Offline"));
            }

            list.Add(new SeparatorEntry());
            list.Add(new ActionContextEntry("actions.resources.DeleteResourcesAction", itemCount == 1 ? "Delete Resource" : $"Delete Resources"));
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