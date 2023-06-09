using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ResourceManaging.Actions {
    [ActionRegistration("actions.resources.ToggleOnlineState")]
    public class ToggleResourceStateAction : ToggleAction {
        public override async Task<bool> OnToggled(AnActionEventArgs e, bool isToggled) {
            if (!e.DataContext.TryGetContext(out ResourceManagerViewModel manager)) {
                if (!e.DataContext.TryGetContext(out ResourceItemViewModel resItem)) {
                    return false;
                }

                manager = resItem.Manager;
            }

            await SetOnlineState(manager, isToggled);
            return true;
        }

        public override async Task<bool> ExecuteNoToggle(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out ResourceManagerViewModel manager)) {
                if (!e.DataContext.TryGetContext(out ResourceItemViewModel resItem)) {
                    return false;
                }

                manager = resItem.Manager;
            }

            await SetOnlineState(manager, null);
            return true;
        }

        private static async Task SetOnlineState(ResourceManagerViewModel manager, bool? state) {
            List<ResourceItemViewModel> list = new List<ResourceItemViewModel>();
            foreach (ResourceItemViewModel item in manager.SelectedItems) {
                if (state == true || (state == null && item.IsOnline)) {
                    await item.Model.SetOffline();
                }
                else {
                    list.Add(item);
                }
            }

            if (list.Count > 0) {
                await ResourceCheckerViewModel.ProcessResources(list);
            }
        }
    }
}