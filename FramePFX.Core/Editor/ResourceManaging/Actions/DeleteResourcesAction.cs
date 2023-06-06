using System;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Core.Editor.ResourceManaging.Actions {
    [ActionRegistration("actions.resources.DeleteItems")]
    public class DeleteResourcesAction : AnAction {
        public static readonly MessageDialog ConfirmationDialog;

        static DeleteResourcesAction() {
            ConfirmationDialog = Dialogs.OkCancelDialog.Clone();
            ConfirmationDialog.IsAlwaysUseThisOptionChecked = true;
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out ResourceManagerViewModel manager)) {
                if (!e.DataContext.TryGetContext(out ResourceItemViewModel resItem)) {
                    return false;
                }

                manager = resItem.Manager;
            }

            int selected = manager.SelectedItems.Count;
            if (selected < 1) {
                return true;
            }

            string caption = $"Delete {selected} item{(selected == 1 ? "" : "s")}";
            string message = $"Are you sure you want to delete {(selected == 1 ? "this 1 item" : $"these {selected} items")}?";
            string result = await ConfirmationDialog.ShowAsync(caption, message);
            if (result != "ok") {
                return true;
            }

            try {
                manager.DeleteSelection();
            }
            catch (Exception ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Exception deleting items", "One or more items threw an exception while it was being deleted", ex.GetToString());
            }

            return true;
        }
    }
}