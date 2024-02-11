using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public abstract class ChangeResourceOnlineStateAction : AnAction {
        protected static void DisableHierarchy(IEnumerable<BaseResource> resources) {
            foreach (BaseResource obj in resources) {
                if (obj is ResourceFolder folder) {
                    DisableHierarchy(folder.Items);
                }
                else if (obj is ResourceItem item && item.IsOnline) {
                    item.Disable(true);
                }
            }
        }
    }

    public class EnableResourcesAction : ChangeResourceOnlineStateAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!ResourceContextRegistry.GetTreeContext(e.DataContext, out BaseResource[] items)) {
                return Task.CompletedTask;
            }

            ResourceLoaderDialog.TryLoadResources(items);
            return Task.CompletedTask;
        }
    }

    public class DisableResourcesAction : ChangeResourceOnlineStateAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!ResourceContextRegistry.GetTreeContext(e.DataContext, out BaseResource[] items)) {
                return Task.CompletedTask;
            }

            DisableHierarchy(items);
            return Task.CompletedTask;
        }
    }
}