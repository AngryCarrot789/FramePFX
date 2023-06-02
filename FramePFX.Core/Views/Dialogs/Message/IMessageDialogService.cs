using System.Threading.Tasks;

namespace FramePFX.Core.Views.Dialogs.Message {
    public interface IMessageDialogService {
        /// <summary>
        /// Shows a simple message box on the UI thread and parents itself to the current active window
        /// </summary>
        /// <param name="caption">The dialog caption/title</param>
        /// <param name="message">The main body/message for the dialog</param>
        Task ShowMessageAsync(string caption, string message);

        /// <summary>
        /// Shows an extended message box on the UI thread and parents itself to the current active window. This can be
        /// used for displaying an exception; the caption is a simple explanation, header is a deeper
        /// explanation, and message is the full exception stack trace itself
        /// </summary>
        /// <param name="caption">The dialog caption/title</param>
        /// <param name="header">A large header, placed below the title but above the body/message, typically in a large font</param>
        /// <param name="message">The main body/message for the dialog</param>
        /// <returns></returns>
        Task ShowMessageExAsync(string caption, string header, string message);

        /// <summary>
        /// Shows a dialog which can be customised by a <see cref="MsgDialogType"/>
        /// </summary>
        /// <param name="caption">The dialog caption/title</param>
        /// <param name="message">The main body/message for the dialog</param>
        /// <param name="type">The type of dialog to show (in terms of the visible buttons)</param>
        /// <param name="defaultResult">The default result for the dialog. Defaults to <see cref="MsgDialogResult.OK"/></param>
        /// <returns></returns>
        Task<MsgDialogResult> ShowDialogAsync(string caption, string message, MsgDialogType type = MsgDialogType.OK, MsgDialogResult defaultResult = MsgDialogResult.OK);

        Task<bool> ShowYesNoDialogAsync(string caption, string message, bool defaultResult = true);

        /// <summary>
        /// Shows an actual dialog view model. <see cref="MessageDialog.Dialog"/> is the only property which will
        /// be mutated, and it will be switched back to the original instance once the dialog closes
        /// </summary>
        /// <param name="dialog">The dialog instance</param>
        /// <returns>Whether the dialog was closed successfully or not</returns>
        Task<bool?> ShowDialogAsync(MessageDialog dialog);
    }
}