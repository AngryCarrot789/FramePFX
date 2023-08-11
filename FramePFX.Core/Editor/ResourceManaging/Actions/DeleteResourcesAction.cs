using System;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Core.Editor.ResourceManaging.Actions {
    public class DeleteResourcesAction : AnAction {
        public static readonly MessageDialog ConfirmationDialog;

        static DeleteResourcesAction() {
            ConfirmationDialog = Dialogs.OkCancelDialog.Clone();
            ConfirmationDialog.IsAlwaysUseThisOptionChecked = true;
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out BaseResourceObjectViewModel resItem))
                return false;

            if (!(resItem is BaseResourceObjectViewModel item))
                return false;

            ResourceGroupViewModel parent = item.Parent;
            if (parent == null || parent.SelectedItems.Count < 1)
                return true;

            try {
                parent.RemoveRange(parent.SelectedItems.ToList());
            }
            catch (Exception ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Exception deleting items", "One or more items threw an exception while it was being deleted", ex.GetToString());
            }

            return true;
        }
    }
}