using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public abstract class ChangeResourceOnlineStateCommand : Command {
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

    public class EnableResourcesCommand : ChangeResourceOnlineStateCommand {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetTreeContext(e.DataContext, out BaseResource[] items)) {
                return Task.CompletedTask;
            }

            ResourceLoaderDialog.TryLoadResources(items);
            return Task.CompletedTask;
        }
    }

    public class DisableResourcesCommand : ChangeResourceOnlineStateCommand {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetTreeContext(e.DataContext, out BaseResource[] items)) {
                return Task.CompletedTask;
            }

            DisableHierarchy(items);
            return Task.CompletedTask;
        }
    }
}