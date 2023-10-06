using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public class DeleteResourcesAction : AnAction {
        public static readonly MessageDialog ConfirmationDialog;

        static DeleteResourcesAction() {
            ConfirmationDialog = Dialogs.OkCancelDialog.Clone();
            ConfirmationDialog.IsAlwaysUseThisOptionChecked = true;
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!ResourceActionUtils.GetSelectedResources(e.DataContext, out List<BaseResourceViewModel> selection))
                return false;

            try {
                foreach (BaseResourceViewModel item in selection) {
                    item.Parent?.RemoveItem(item);
                }
            }
            catch (Exception ex) {
                await Services.DialogService.ShowMessageExAsync("Exception deleting items", "One or more items threw an exception while it was being deleted", ex.GetToString());
            }

            return true;
        }
    }
}