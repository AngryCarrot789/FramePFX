using System.Threading.Tasks;

namespace SharpPadV2.Core.Views.Dialogs {
    /// <summary>
    /// The base class for views, which is typically passed to a ViewModel, in order to access a close function while passing a custom DialogResult
    /// </summary>
    public interface IDialog : IViewBase {
        void CloseDialog(bool result);

        Task CloseDialogAsync(bool result);
    }
}