using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Views.Dialogs.Modal;

namespace FramePFX.Core.Views.Dialogs.Message
{
    public class MessageDialogUsage : IDisposable
    {
        private readonly List<DialogButton> oldButtons;
        private readonly string oldAutoResult;
        private readonly bool oldAlwaysUseNextResult;
        private readonly bool oldShowAlwaysUseNextResult;
        private readonly bool oldCanShowAlwaysUseNextResultForQueue;
        private readonly string oldDefaultResult;
        private readonly string oldPrimaryResult;
        private readonly bool isReadOnly;

        public MessageDialog DialogToModify { get; }

        public MessageDialogUsage(MessageDialog inputDialog)
        {
            this.DialogToModify = inputDialog.IsReadOnly ? inputDialog.Clone() : inputDialog;
            if (inputDialog.IsReadOnly)
            {
                this.isReadOnly = true;
            }
            else
            {
                // Store the current dialog state
                this.oldAlwaysUseNextResult = this.DialogToModify.IsAlwaysUseThisOptionChecked;
                this.oldShowAlwaysUseNextResult = this.DialogToModify.ShowAlwaysUseNextResultOption;
                this.oldCanShowAlwaysUseNextResultForQueue = this.DialogToModify.CanShowAlwaysUseNextResultForCurrentQueueOption;
                this.oldButtons = this.DialogToModify.Buttons.ToList();
                this.oldAutoResult = this.DialogToModify.AutomaticResult;
                this.oldDefaultResult = this.DialogToModify.DefaultResult;
                this.oldPrimaryResult = this.DialogToModify.PrimaryResult;
            }
        }

        /// <summary>
        /// Restores the dialog to its old state
        /// </summary>
        public void Dispose()
        {
            if (this.isReadOnly)
            {
                return;
            }

            this.DialogToModify.buttons.ClearAndAddRange(this.oldButtons);
            if (this.DialogToModify.IsAlwaysUseThisOptionForCurrentQueueChecked)
            {
                // We are only applying data for the current usage and that usage is finished now, so, revert the data
                this.DialogToModify.IsAlwaysUseThisOptionForCurrentQueueChecked = false; // this should always be false when the usage instance is created
                this.DialogToModify.AutomaticResult = this.oldAutoResult;
            }
            else
            {
                // here we handle the case where we are saving results globally for this
                // dialog; "save these results" is checked, but
                // "save for current queue only" is not checked
                // -------------------------------------------------------
                // nothing needs to be handled here
                // -------------------------------------------------------
            }

            this.DialogToModify.ShowAlwaysUseNextResultOption = this.oldShowAlwaysUseNextResult;
            this.DialogToModify.CanShowAlwaysUseNextResultForCurrentQueueOption = this.oldCanShowAlwaysUseNextResultForQueue;
            this.DialogToModify.IsAlwaysUseThisOptionChecked = this.oldAlwaysUseNextResult;
            this.DialogToModify.DefaultResult = this.oldDefaultResult;
            this.DialogToModify.PrimaryResult = this.oldPrimaryResult;

            if (this.isReadOnly)
            {
                this.DialogToModify.IsReadOnly = true;
            }
        }
    }
}