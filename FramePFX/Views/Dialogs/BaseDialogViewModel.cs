using FramePFX.Commands;

namespace FramePFX.Views.Dialogs
{
    public class BaseDialogViewModel : BaseViewModel
    {
        public IDialog Dialog { get; set; }

        public RelayCommand CloseCommand { get; }

        public BaseDialogViewModel()
        {
            this.CloseCommand = new RelayCommand(this.CloseDialogAction, this.CanCloseDialog);
        }

        protected virtual bool CanCloseDialog()
        {
            return this.Dialog != null;
        }

        protected virtual void CloseDialogAction()
        {
            this.Dialog?.CloseDialog(false);
        }
    }
}