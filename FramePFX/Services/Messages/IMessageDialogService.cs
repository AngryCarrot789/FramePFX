using System.Windows;

namespace FramePFX.Services.Messages {
    public interface IMessageDialogService {
        /// <summary>
        /// Shows a dialog message, blocking until it closes. This must be called on the application main thread
        /// </summary>
        /// <param name="caption">The window titlebar message</param>
        /// <param name="message">The main message content</param>
        /// <param name="buttons">The buttons to show</param>
        /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
        MessageBoxResult ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK);

        /// <summary>
        /// Shows a dialog message, blocking until it closes. This must be called on the application main thread
        /// </summary>
        /// <param name="caption">The window titlebar message</param>
        /// <param name="header">A message presented in bold above the message, a less concise caption but still short</param>
        /// <param name="message">The main message content</param>
        /// <param name="buttons">The buttons to show</param>
        /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
        MessageBoxResult ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK);
    }
}