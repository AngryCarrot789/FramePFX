using System.Threading.Tasks;
using FramePFX.Core.Views.ViewModels;

namespace FramePFX.Core.Views.Dialogs {
    /// <summary>
    /// A helper base view model for managing a dialog and the standard Confirm/Cancel behaviour. In order to disable
    /// the confirm button when errors are present, implement and override <see cref="IErrorInfoHandler.OnErrorsUpdated(System.Collections.Generic.Dictionary{string, object})"/>,
    /// and set the <see cref="BaseRelayCommand.IsEnabled"/> state to true/false depending if the dictionary is empty/not empty, respectively
    /// </summary>
    public class BaseConfirmableDialogViewModel : BaseDialogViewModel {
        public RelayCommand ConfirmCommand { get; }
        public RelayCommand CancelCommand { get; }

        public BaseConfirmableDialogViewModel() {
            this.ConfirmCommand = new RelayCommand(async () => await this.ConfirmAction());
            this.CancelCommand = new RelayCommand(async () => await this.CancelAction());
        }

        public BaseConfirmableDialogViewModel(IDialog dialog) : this() {
            this.Dialog = dialog;
        }

        public virtual async Task ConfirmAction() {
            if (await this.CanConfirm()) {
                await this.OnDialogClosing(true);
                await this.Dialog.CloseDialogAsync(true);
                await this.OnDialogClosed(true);
            }
        }

        public virtual async Task CancelAction() {
            await this.OnDialogClosing(false);
            await this.Dialog.CloseDialogAsync(false);
            await this.OnDialogClosed(false);
        }

        /// <summary>
        /// Called just before the confirm action is executed, to check if this
        /// dialog actually can close. If this returns true, the dialog closes
        /// <para>
        /// This method can also be used to set some final state in the dialog
        /// </para>
        /// </summary>
        public virtual Task<bool> CanConfirm() {
            if (this.Dialog is IHasErrorInfo errors && errors.Errors.Count > 0) {
                // Should a window really be shown? It's probably better just use WPF's
                // validation templates + adorners and just disable the confirm command

                // await IoC.ErrorInfo.ShowDialogAsync(dictionary.Select(x => new Tuple<string, string>(x.Key, x.Value)));
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public virtual Task OnDialogClosing(bool result) {
            return Task.CompletedTask;
        }

        public virtual Task OnDialogClosed(bool result) {
            return Task.CompletedTask;
        }
    }
}