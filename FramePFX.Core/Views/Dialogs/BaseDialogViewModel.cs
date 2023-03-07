namespace FrameControl.Core.Views.Dialogs {
    public abstract class BaseDialogViewModel : BaseViewModel {
        public IDialog Dialog { get; }

        protected BaseDialogViewModel(IDialog dialog) {
            this.Dialog = dialog;
        }
    }
}