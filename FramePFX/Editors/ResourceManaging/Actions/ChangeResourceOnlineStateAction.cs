using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public abstract class ChangeResourceOnlineStateAction : AnAction {
        protected static void SetHierarchyOnlineState(IEnumerable<BaseResource> resources, bool state, ResourceLoader loader) {
            foreach (BaseResource obj in resources) {
                if (obj is ResourceFolder folder) {
                    SetHierarchyOnlineState(folder.Items, state, loader);
                }
                else {
                    ResourceItem item = (ResourceItem)obj;
                    if (item.IsOnline != state) {
                        if (state) {
                            item.Enable(loader);
                        }
                        else {
                            item.Disable(true);
                        }
                    }
                }
            }
        }
    }

    public class EnableResourcesAction : ChangeResourceOnlineStateAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            IDataContext context = e.DataContext;
            if (context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource focusedResource)) {
                HashSet<BaseResource> resources = new HashSet<BaseResource>(focusedResource.Manager.SelectedItems);
                if (!focusedResource.IsSelected)
                    resources.Add(focusedResource);

                ResourceLoader loader = new ResourceLoader();
                SetHierarchyOnlineState(resources, true, loader);
                ResourceLoaderDialog.ShowLoaderDialog(loader);
            }

            return Task.CompletedTask;
        }
    }

    public class DisableResourcesAction : ChangeResourceOnlineStateAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            IDataContext context = e.DataContext;
            if (context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource focusedResource)) {
                HashSet<BaseResource> resources = new HashSet<BaseResource>(focusedResource.Manager.SelectedItems);
                if (!focusedResource.IsSelected)
                    resources.Add(focusedResource);

                SetHierarchyOnlineState(resources, false, null);
            }

            return Task.CompletedTask;
        }
    }
}