using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public class ToggleResourceOnlineStateAction : ToggleAction {
        public override async Task<bool> OnToggled(AnActionEventArgs e, bool isToggled) {
            if (e.DataContext.TryGetContext(out ResourceItemViewModel resItem)) {
                List<ResourceItemViewModel> items = resItem.Manager.SelectedItems.OfType<ResourceItemViewModel>().ToList();
                if (!items.Contains(resItem)) {
                    items.Add(resItem);
                }

                if (items.Count > 0) {
                    await SetOnlineState(items, isToggled);
                    return true;
                }
            }

            return true;
        }

        public override async Task<bool> ExecuteNoToggle(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out ResourceItemViewModel resItem)) {
                List<ResourceItemViewModel> items = resItem.Manager.SelectedItems.OfType<ResourceItemViewModel>().ToList();
                if (items.Count > 0) {
                    await SetOnlineState(items, null);
                    return true;
                }
            }

            return false;
        }

        private static async Task SetOnlineState(IEnumerable<ResourceItemViewModel> items, bool? state) {
            List<ResourceItemViewModel> list = new List<ResourceItemViewModel>();
            using (ErrorList stack = new ErrorList(false)) {
                foreach (ResourceItemViewModel item in items) {
                    if (state == false || (state == null && item.IsOnline)) {
                        await item.SetOfflineAsync(true);
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
                await ResourceCheckerViewModel.LoadResources(list, true);
            }
        }
    }
}