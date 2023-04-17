namespace FramePFX.Core.Views.Dialogs {
    /// <summary>
    /// A helper base view model that has a reference to the dialog associated with this view model
    /// </summary>
    public class BaseDialogViewModel : BaseViewModel {
        public IDialog Dialog { get; set; }

        public BaseDialogViewModel() {

        }
    }
}