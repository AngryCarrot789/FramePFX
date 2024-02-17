using System.Collections.Generic;
using FramePFX.CommandSystem;
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
        public override void Execute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetTreeContext(e.DataContext, out BaseResource[] items)) {
                return;
            }

            ResourceLoaderDialog.TryLoadResources(items);
        }
    }

    public class DisableResourcesCommand : ChangeResourceOnlineStateCommand {
        public override void Execute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetTreeContext(e.DataContext, out BaseResource[] items)) {
                return;
            }

            DisableHierarchy(items);
        }
    }
}