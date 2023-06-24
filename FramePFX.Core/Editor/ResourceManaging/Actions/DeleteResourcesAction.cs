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
            if (!e.DataContext.TryGetContext(out BaseResourceObjectViewModel resItem)) {
                return false;
            }

            ResourceGroupViewModel group;

            if (resItem is ResourceItemViewModel item) {
                group = item.Parent;
            }
            else if (resItem is ResourceGroupViewModel) {
                group = (ResourceGroupViewModel) resItem;
            }
            else {
                return false;
            }

            int selected = group.SelectedItems.Count;
            if (selected < 1) {
                return true;
            }

            try {
                await group.DeleteSelectionAction();
            }
            catch (Exception ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Exception deleting items", "One or more items threw an exception while it was being deleted", ex.GetToString());
            }

            return true;
        }
    }
}