using FramePFX.AdvancedContextService;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Interactivity.DataContexts;
using System.Collections.Generic;

namespace FramePFX.Editors.Contextual {
    public class ResourceContextRegistry : IContextGenerator {
        public static ResourceContextRegistry Instance { get; } = new ResourceContextRegistry();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (!context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource resource)) {
                return;
            }

            int selectedCount = resource.Manager.SelectedItems.Count;
            if (!resource.IsSelected)
                selectedCount++;

            list.Add(new EventContextEntry(this.EnableResources, selectedCount == 1 ? "Enable Resource" : "Enable Resources"));
            list.Add(new EventContextEntry(this.DisableResources, selectedCount == 1 ? "Disable Resource" : "Disable Resources"));
            list.Add(new SeparatorEntry());
            list.Add(new EventContextEntry(this.DeleteResources, selectedCount == 1 ? "Delete Resource" : "Delete Resources"));
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

            SetHierarchyOnlineState(resources, true, new ResourceLoader());
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