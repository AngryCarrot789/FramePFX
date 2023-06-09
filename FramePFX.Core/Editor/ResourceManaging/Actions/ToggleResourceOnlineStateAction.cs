using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.Actions {
    [ActionRegistration("actions.resources.ToggleOnlineState")]
    public class ToggleResourceOnlineStateAction : ToggleAction {
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
            using (ExceptionStack stack = new ExceptionStack(false)) {
                foreach (ResourceItemViewModel item in manager.SelectedItems) {
                    if (state == true || (state == null && item.IsOnline)) {
                        await item.Model.SetOfflineAsync(stack);
                    }
                    else {
                        list.Add(item);
                    }
                }

                if (stack.TryGetException(out Exception exception)) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting one or more resource to offline", exception.GetToString());
                }
            }

            if (list.Count > 0) {
                await ResourceCheckerViewModel.ProcessResources(list, true);
            }
        }
    }
}