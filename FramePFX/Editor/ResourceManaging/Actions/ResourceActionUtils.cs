using System.Collections.Generic;
using System.Linq;
using FramePFX.Actions.Contexts;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public static class ResourceActionUtils {
        // Optimised contains function
        private static bool Contains(IList<BaseResourceViewModel> list, BaseResourceViewModel item) {
            return list.Count > 0 && (ReferenceEquals(list[0], item) || ReferenceEquals(list[list.Count - 1], item) || ContainsSlow(list, item));
        }

        private static bool ContainsSlow(IList<BaseResourceViewModel> list, BaseResourceViewModel item) {
            for (int i = 1, count = list.Count - 1; i < count; i++) {
                if (ReferenceEquals(list[i], item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a list of selected items (may be empty when no specific item is focused) and a resource manager
        /// </summary>
        /// <returns>True when a resource manager was found, otherwise false</returns>
        public static bool GetSelectedResources(IDataContext context, out ResourceManagerViewModel manager, out List<BaseResourceViewModel> resources) {
            return GetSelectedResourcesInternal(context, out manager, out resources, true);
        }

        /// <summary>
        /// Gets a list of selected items (may be empty when no specific item is focused)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resources"></param>
        /// <returns>True when a resource manager was found, otherwise false</returns>
        public static bool GetSelectedResources(IDataContext context, out List<BaseResourceViewModel> resources) {
            return GetSelectedResourcesInternal(context, out _, out resources, false);
        }

        private static bool GetSelectedResourcesInternal(IDataContext context, out ResourceManagerViewModel manager, out List<BaseResourceViewModel> resources, bool requireManagerForSingleItem) {
            if (context.TryGetContext(out BaseResourceViewModel resource)) {
                if ((manager = resource.Manager) != null || context.TryGetContext(out manager)) {
                    resources = manager.SelectedItems.ToList();
                    if (!Contains(resources, resource)) {
                        resources.Add(resource);
                    }

                    return true;
                }
                else if (!requireManagerForSingleItem) {
                    resources = new List<BaseResourceViewModel> {resource};
                    return true;
                }
            }
            else if (context.TryGetContext(out manager)) {
                resources = manager.SelectedItems.ToList();
                return true;
            }

            manager = null;
            resources = null;
            return false;
        }
    }
}