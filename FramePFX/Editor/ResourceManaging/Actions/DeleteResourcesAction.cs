using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Logger;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public class DeleteResourcesAction : ExecutableAction {
        public static readonly MessageDialog ConfirmationDialog;

        static DeleteResourcesAction() {
            ConfirmationDialog = Dialogs.OkCancelDialog.Clone();
            ConfirmationDialog.IsAlwaysUseThisOptionChecked = true;
        }

        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!ResourceActionUtils.GetSelectedResources(e.DataContext, out List<BaseResourceViewModel> selection)) {
                return false;
            }

            if (selection.Count < 1) {
                return true;
            }

            int clips = 0, unknown = 0, resCount = selection.Count;
            foreach (BaseResourceViewModel resource in selection) {
                if (resource.Model is ResourceItem) {
                    ((ResourceItem) resource.Model).GetReferenceInfo(out int nClips, out int nUnknown);
                    clips += nClips;
                    unknown += nUnknown;
                }
            }

            if (clips + unknown > 0) {
                // is there even a better way to do this?
                string msg = string.Format("There {0} {1} clip{2}reference{3} to {4} resource{5}. Do you want to delete {4} resource{5}?",
                    Lang.IsAre(clips), clips,
                    unknown < 1 ? " " : $" and {unknown} unknown ",
                    Lang.S(unknown > 0 ? unknown : clips),
                    Lang.ThisThese(resCount),
                    Lang.S(resCount));
                if (!await IoC.DialogService.ShowYesNoDialogAsync("Delete resources", msg)) {
                    return true;
                }
            }

            try {
                foreach (BaseResourceViewModel item in selection) {
                    if (item.Model.Parent == null) {
                        AppLogger.WriteLine($"[FATAL] Item in selection had no parent: {item.Model}");
                        continue;
                    }

                    if (!item.Model.Parent.UnregisterRemoveAndDisposeItem(item.Model)) {
                        throw new Exception("Corrupted resource item: valid parent, but was not stored in the parent list");
                    }
                }
            }
            catch (Exception ex) {
                await IoC.DialogService.ShowMessageExAsync("Exception deleting items", "One or more items threw an exception while it was being deleted", ex.GetToString());
            }

            return true;
        }
    }
}