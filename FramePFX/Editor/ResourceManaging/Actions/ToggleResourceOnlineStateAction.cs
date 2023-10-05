using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ViewModels;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public class ToggleResourceOnlineStateAction : ToggleAction {
        protected override async Task<bool> OnToggled(AnActionEventArgs e, bool isToggled) {
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

        protected override async Task<bool> ExecuteNoToggle(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out ResourceItemViewModel resItem)) {
                List<ResourceItemViewModel> items = resItem.Manager.SelectedItems.OfType<ResourceItemViewModel>().ToList();
                if (items.Count > 0) {
                    if (!items.Contains(resItem))
                        items.Add(resItem);

                    await SetOnlineState(items, null);
                    return true;
                }
            }

            return false;
        }

        private static async Task SetOnlineState(IEnumerable<ResourceItemViewModel> items, bool? state) {
            List<ResourceItemViewModel> altList = items.ToList();
            List<ResourceItemViewModel> list = new List<ResourceItemViewModel>();
            using (ErrorList stack = new ErrorList(false)) {
                foreach (ResourceItemViewModel item in altList) {
                    if (state == false || (state == null && item.IsOnline)) {
                        await item.SetOfflineAsync(true);
                    }
                    else {
                        list.Add(item);
                    }
                }

                if (stack.TryGetException(out Exception exception)) {
                    await Services.DialogService.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting one or more resource to offline", exception.GetToString());
                }
            }

            if (list.Count > 0) {
                ResourceCheckerViewModel checker = new ResourceCheckerViewModel() {
                    Caption = (list.Count == 1 ? "This resource" : "These resources") + " could not be loaded (e.g. missing files)"
                };

                await ResourceCheckerViewModel.LoadResources(checker, list, true);
            }

            VideoEditorViewModel editor = altList.FirstOrDefault(x => x.Manager != null)?.Manager?.Project?.Editor;
            if (editor != null && editor.SelectedTimeline != null) {
                await editor.DoDrawRenderFrame(editor.SelectedTimeline);
            }
        }
    }
}