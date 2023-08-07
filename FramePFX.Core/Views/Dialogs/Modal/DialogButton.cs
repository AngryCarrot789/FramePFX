using System;
using System.Threading.Tasks;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Core.Views.Dialogs.Modal
{
    public class DialogButton : BaseViewModel
    {
        /// <summary>
        /// The dialog that owns this button
        /// </summary>
        public BaseDynamicDialogViewModel Dialog { get; }

        /// <summary>
        /// A command that fires when this dialog is clicked. This is read only, as it's only
        /// meant to be fired by the UI typically when a button is clicked
        /// </summary>
        public AsyncRelayCommand Command { get; }

        /// <summary>
        /// The action that clicking the command results in
        /// </summary>
        public string ActionType { get; }

        private string text;

        public string Text
        {
            get => this.text;
            set => this.RaisePropertyChanged(ref this.text, value);
        }

        private string toolTip;

        public string ToolTip
        {
            get => this.toolTip;
            set => this.RaisePropertyChanged(ref this.toolTip, value);
        }

        public bool IsEnabled
        {
            get => this.Command.IsEnabled;
            set
            {
                this.Command.IsEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        private bool canUseAsAutomaticResult;

        /// <summary>
        /// Whether or not this button can be used as the automatic result when the owning dialog is trying to show
        /// </summary>
        public bool CanUseAsAutomaticResult
        {
            get => this.canUseAsAutomaticResult;
            set => this.RaisePropertyChanged(ref this.canUseAsAutomaticResult, value);
        }

        public DialogButton(BaseDynamicDialogViewModel dialog, string actionType, string text, bool canUseAsAutomaticResult)
        {
            this.Dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            this.ActionType = actionType;
            this.text = text ?? actionType ?? "<btn>";
            this.Command = new AsyncRelayCommand(this.OnClickedAction);
            this.canUseAsAutomaticResult = canUseAsAutomaticResult;
        }

        public virtual Task OnClickedAction()
        {
            if (this.IsEnabled)
            {
                return this.Dialog.OnButtonClicked(this);
            }

            return Task.CompletedTask;
        }

        public virtual DialogButton Clone(BaseProcessDialogViewModel dialog)
        {
            return new DialogButton(dialog, this.ActionType, this.Text, this.CanUseAsAutomaticResult) {
                IsEnabled = this.IsEnabled, ToolTip = this.ToolTip
            };
        }

        public void UpdateState()
        {
            if (this.Dialog is MessageDialog dialog)
            {
                if (dialog.IsAlwaysUseThisOptionChecked || dialog.IsAlwaysUseThisOptionForCurrentQueueChecked)
                {
                    this.IsEnabled = this.CanUseAsAutomaticResult;
                }
                else
                {
                    this.IsEnabled = true;
                }
            }
            else
            {
                this.IsEnabled = true;
            }
        }
    }
}