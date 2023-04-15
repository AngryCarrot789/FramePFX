using System.Threading.Tasks;
using FramePFX.Core.Views.ViewModels;

namespace FramePFX.Core.Views.Dialogs {
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
                await this.Dialog.CloseDialogAsync(true);
                await this.OnDialogClosed();
            }
        }

        public virtual async Task CancelAction() {
            if (await this.CanCancel()) {
                await this.Dialog.CloseDialogAsync(false);
                await this.OnDialogClosed();
            }
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

        /// <summary>
        /// Called just before the cancel action is executed, to check if this
        /// dialog actually can close. This should never really return anything except true,
        /// otherwise the user will be unable to close the dialog (except for clicking the X button)
        /// </summary>
        public virtual Task<bool> CanCancel() {
            return Task.FromResult(true);
        }

        public virtual Task OnDialogClosed() {
            return Task.CompletedTask;
        }
    }
}